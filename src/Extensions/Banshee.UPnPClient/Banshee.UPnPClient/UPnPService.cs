//
// UPnPClientSource.cs
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

using Mono.Addins;

using Mono.Upnp;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV;

using Banshee.Base;
using Banshee.Sources.Gui;
using Banshee.ServiceStack;
using Banshee.Preferences;
using Banshee.MediaEngine;
using Banshee.PlaybackController;

namespace Banshee.UPnPClient
{
    public class UPnPService : IExtensionService, IDisposable
    {
        private Mono.Upnp.Client client;
        private UPnPContainerSource container;

        void IExtensionService.Initialize ()
        {
            container = new UPnPContainerSource();
            ServiceManager.SourceManager.AddSource(container);

            client = new Mono.Upnp.Client ();
            client.DeviceAdded += DeviceAdded;

            client.Browse(Mono.Upnp.Dcp.MediaServer1.MediaServer.DeviceType);
        }
    
        public void Dispose ()
        {
            if (container != null)
            {
                foreach (UPnPSource source in container.Children)
                    source.Disconnect();

                ServiceManager.SourceManager.RemoveSource(container);
                container = null;
            }
        }

        void DeviceAdded (object sender, DeviceEventArgs e)
        {
            Hyena.Log.Debug ("UPnPService.DeviceAdded (" + e.Device.ToString() + ") (" + e.Device.Type + ")");
            Device device = e.Device.GetDevice();
            
            ContentDirectoryController contentDirectory = null;
            
            foreach (Service service in device.Services) {
                Hyena.Log.Debug ("UPnPService \"" + device.FriendlyName + "\" Implements " + service.Type);
                if (service.Type.Equals (Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.ContentDirectory.ServiceType))
                    contentDirectory = new ContentDirectoryController (service.GetController());
            }

            if (contentDirectory != null)
            {
                UPnPSource source = new UPnPSource(device);
                container.AddChildSource (source);
                Parse(source, contentDirectory);
            }
        }


        static void Parse (UPnPSource source, ContentDirectoryController contentDirectory)
        {
            RemoteContentDirectory remoteContentDirectory = new RemoteContentDirectory (contentDirectory);
            List<MusicTrack> musicTracks = new List<MusicTrack>();
            DateTime begin = DateTime.Now;
            Container root = remoteContentDirectory.GetRootObject();
            bool recursiveBrowse = !contentDirectory.CanSearch;

            if (!recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Searchable, lets search");
					foreach (var item in remoteContentDirectory.Search<MusicTrack>(root, visitor => visitor.VisitDerivedFrom("upnp:class", "object.item.audioItem.musicTrack"), new ResultsSettings())) {
                        musicTracks.Add(item as MusicTrack);
					}
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                    recursiveBrowse = true;
                }
            }
            if (recursiveBrowse) {
                try {
                    Hyena.Log.Debug ("Not searchable, lets recursive browse");
                    ParseContainer (source, remoteContentDirectory, root, 0, musicTracks);
                } catch (Exception exception) {
                    Hyena.Log.Exception (exception);
                }
            }

            source.AddTracks (musicTracks);
            Hyena.Log.Debug ("Found all items on the service, took " + (DateTime.Now - begin).ToString());
        }

        static void ParseContainer (UPnPSource source, RemoteContentDirectory remoteContentDirectory, Container container, int depth, List<MusicTrack> musicTracks)
        {
            if (depth > 10 || (container.ChildCount != null && container.ChildCount == 0))
                return;

            foreach (var upnp_object in remoteContentDirectory.GetChildren<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object>(container)) {
                if (upnp_object is Item) {
                    Item item = upnp_object as Item;

                    if (item.IsReference || item.Resources.Count == 0)
                      continue;

                    if (item is MusicTrack) {
                        musicTracks.Add(item as MusicTrack);
                    }
                }
                else if (upnp_object is Container) {
                    ParseContainer (source, remoteContentDirectory, upnp_object as Container, depth + 1, musicTracks);
                }
            }
        }
    
        string IService.ServiceName {
            get { return "uPnP Client service"; }
        }
  }
}
