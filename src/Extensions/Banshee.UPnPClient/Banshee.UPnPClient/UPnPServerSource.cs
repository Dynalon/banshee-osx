//
// UPnPContainerSource.cs
//
// Authors:
//   Tobias 'topfs2' Arrskog <tobias.arrskog@gmail.com>
//
// Copyright (C) 2011 Tobias 'topfs2' Arrskog
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Mono.Unix;

using Mono.Upnp;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV;

using Banshee.Base;
using Banshee.Collection;
using Banshee.Configuration;
using Banshee.Sources;
using Banshee.ServiceStack;

using Hyena;

namespace Banshee.UPnPClient
{
    public class UPnPServerSource : Source
    {
        UPnPMusicSource music_source;
        private UPnPVideoSource video_source;
        private SchemaEntry<bool> expanded_schema;

        public UPnPServerSource (Device device) :  base (Catalog.GetString ("UPnP Share"), device.FriendlyName, 300)
        {
            Properties.SetStringList ("Icon.Name", "computer", "network-server");
            TypeUniqueId = "upnp-container";
            expanded_schema = new SchemaEntry<bool> ("plugins.upnp." + device.Udn, "expanded", true, "UPnP Share expanded", "UPnP Share expanded" );

            ContentDirectoryController contentDirectory = null;
            
            foreach (Service service in device.Services) {
                Hyena.Log.Debug ("UPnPService \"" + device.FriendlyName + "\" Implements " + service.Type);
                if (service.Type.Equals (Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.ContentDirectory.ServiceType))
                    contentDirectory = new ContentDirectoryController (service.GetController());
            }

            if (contentDirectory == null)
                throw new ArgumentNullException("contentDirectory");

            music_source = new UPnPMusicSource(device.Udn);
            AddChildSource (music_source);

            video_source = new UPnPVideoSource(device.Udn);
            AddChildSource (video_source);

            ThreadAssist.Spawn (delegate {
                Parse (contentDirectory);
            });
            
        }

        ~UPnPServerSource ()
        {
            RemoveChildSource (music_source);
            music_source = null;
        }

        delegate void ChunkHandler<T> (Results<T> results);
        void HandleResults<T> (Results<T> results, RemoteContentDirectory remoteContentDirectory, ChunkHandler<T> chunkHandler)
        {
            bool hasresults = results.Count > 0;

            while (hasresults) {
                chunkHandler(results);

                hasresults = results.HasMoreResults;
                if (hasresults)
                    results = results.GetMoreResults(remoteContentDirectory);
            }
        }

        void Parse (ContentDirectoryController contentDirectory)
        {
            RemoteContentDirectory remoteContentDirectory = new RemoteContentDirectory (contentDirectory);
            DateTime begin = DateTime.Now;
            Container root = remoteContentDirectory.GetRootObject();
            bool recursiveBrowse = !contentDirectory.CanSearch;

            if (!recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Searchable, lets search");

                    HandleResults<MusicTrack> (remoteContentDirectory.Search<MusicTrack>(root, visitor => visitor.VisitDerivedFrom("upnp:class", "object.item.audioItem.musicTrack"), new ResultsSettings()),
                                               remoteContentDirectory,
                                               chunk => {
                                                            List<MusicTrack> musicTracks = new List<MusicTrack>();
                                                            foreach (var item in chunk)
                                                                musicTracks.Add(item as MusicTrack);

                                                            music_source.AddTracks (musicTracks);
                                                        });

                    HandleResults<VideoItem>  (remoteContentDirectory.Search<VideoItem>(root, visitor => visitor.VisitDerivedFrom("upnp:class", "object.item.videoItem"), new ResultsSettings()),
                                               remoteContentDirectory,
                                               chunk => {
                                                            List<VideoItem> videoTracks = new List<VideoItem>();
                                                            foreach (var item in chunk)
                                                                videoTracks.Add(item as VideoItem);

                                                            video_source.AddTracks (videoTracks);
                                                        });
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                    recursiveBrowse = true;
                }
            }
            if (recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Not searchable, lets recursive browse");
                    List<MusicTrack> musicTracks = new List<MusicTrack>();
                    List<VideoItem> videoTracks = new List<VideoItem>();

                    ParseContainer (remoteContentDirectory, root, 0, musicTracks, videoTracks);

                    if (musicTracks.Count > 0)
                        music_source.AddTracks (musicTracks);
                    if (videoTracks.Count > 0)
                        video_source.AddTracks (videoTracks);
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                }
            }

            Hyena.Log.Debug ("Found all items on the service, took " + (DateTime.Now - begin).ToString());
        }

        void ParseContainer (RemoteContentDirectory remoteContentDirectory, Container container, int depth, List<MusicTrack> musicTracks, List<VideoItem> videoTracks)
        {
            if (depth > 10 || (container.ChildCount != null && container.ChildCount == 0))
                return;

            HandleResults<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object> (
                                       remoteContentDirectory.GetChildren<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object>(container),
                                       remoteContentDirectory,
                                       chunk => {
                                                    foreach (var upnp_object in chunk) {
                                                        if (upnp_object is Item) {
                                                            Item item = upnp_object as Item;

                                                            if (item.IsReference || item.Resources.Count == 0)
                                                                continue;

                                                            if (item is MusicTrack) {
                                                                musicTracks.Add(item as MusicTrack);
                                                            } else if (item is VideoItem) {
                                                                videoTracks.Add(item as VideoItem);
                                                            }
                                                        }
                                                        else if (upnp_object is Container) {
                                                            ParseContainer (remoteContentDirectory, upnp_object as Container, depth + 1, musicTracks, videoTracks);
                                                        }

                                                        if (musicTracks.Count > 500) {
                                                            music_source.AddTracks (musicTracks);
                                                            musicTracks.Clear();
                                                        }
                                                        if (videoTracks.Count > 100) {
                                                            video_source.AddTracks (videoTracks);
                                                            videoTracks.Clear();
                                                        }
                                                    }
                                                });
        }

        public void Disconnect ()
        {
            music_source.Disconnect ();
            video_source.Disconnect ();
        }

        public override bool? AutoExpand {
            get { return expanded_schema.Get (); }
        }

        public override bool Expanded {
            get { return expanded_schema.Get (); }
            set { expanded_schema.Set (value); }
        }

        public override bool CanActivate {
            get { return false; }
        }

        public override bool CanRename {
            get { return false; }
        }
    }
}
