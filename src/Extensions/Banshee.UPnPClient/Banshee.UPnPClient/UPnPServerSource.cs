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

namespace Banshee.UPnPClient
{
    public class UPnPServerSource : Source
    {
        UPnPMusicSource music_source;
        private SchemaEntry<bool> expanded_schema;

        public UPnPServerSource (Device device) :  base (Catalog.GetString ("UPnP Share"), device.FriendlyName, 300)
        {
            Properties.SetStringList ("Icon.Name", "applications-internet", "network-server");
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

            Parse (contentDirectory);
        }

        void Parse (ContentDirectoryController contentDirectory)
        {
            RemoteContentDirectory remoteContentDirectory = new RemoteContentDirectory (contentDirectory);
            List<MusicTrack> musicTracks = new List<MusicTrack>();
            DateTime begin = DateTime.Now;
            Container root = remoteContentDirectory.GetRootObject();
            bool recursiveBrowse = !contentDirectory.CanSearch;

            if (!recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Searchable, lets search");
                    Results<MusicTrack> results = remoteContentDirectory.Search<MusicTrack>(root, visitor => visitor.VisitDerivedFrom("upnp:class", "object.item.audioItem.musicTrack"), new ResultsSettings());
                    bool hasresults = results.Count > 0;

                    while (hasresults) {
					    foreach (var item in results) {
                            musicTracks.Add(item as MusicTrack);
					    }

                        if (results.HasMoreResults) {
                            results = results.GetMoreResults(remoteContentDirectory);
                            music_source.AddTracks (musicTracks);
                            musicTracks.Clear();
                        }
                        else
                            hasresults = false;
                    }
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                    recursiveBrowse = true;
                }
            }
            if (recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Not searchable, lets recursive browse");
                    ParseContainer (remoteContentDirectory, root, 0, musicTracks);
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                }
            }

            if (musicTracks.Count > 0)
                music_source.AddTracks (musicTracks);

            Hyena.Log.Debug ("Found all items on the service, took " + (DateTime.Now - begin).ToString());
        }

        void ParseContainer (RemoteContentDirectory remoteContentDirectory, Container container, int depth, List<MusicTrack> musicTracks)
        {
            if (depth > 10 || (container.ChildCount != null && container.ChildCount == 0))
                return;
            Results<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object> results = remoteContentDirectory.GetChildren<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object>(container);
            bool hasresults = results.Count > 0;
            while (hasresults) {
                foreach (var upnp_object in results) {
                    if (upnp_object is Item) {
                        Item item = upnp_object as Item;

                        if (item.IsReference || item.Resources.Count == 0)
                          continue;

                        if (item is MusicTrack) {
                            musicTracks.Add(item as MusicTrack);
                        }
                    }
                    else if (upnp_object is Container) {
                        ParseContainer (remoteContentDirectory, upnp_object as Container, depth + 1, musicTracks);
                    }

                    if (musicTracks.Count > 500) {
                        music_source.AddTracks (musicTracks);
                        musicTracks.Clear();
                    }
                }

                if (results.HasMoreResults)
                    results = results.GetMoreResults(remoteContentDirectory);
                else
                    hasresults = false;
            }
        }

        public void Disconnect ()
        {
            music_source.Disconnect ();
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
