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

using Mono.Unix;
using Mono.Addins;

using Mono.Upnp;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1;
using Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.AV;

using Hyena.Collections;

using Banshee.Base;
using Banshee.Sources;
using Banshee.Sources.Gui;
using Banshee.Collection.Database;
using Banshee.ServiceStack;
using Banshee.Preferences;
using Banshee.MediaEngine;
using Banshee.PlaybackController;

namespace Banshee.UPnPClient
{
    public class UPnPSource : PrimarySource
    {
        const int sort_order = 190;

        public UPnPSource (Device device, RemoteContentDirectory contentDirectory) : base (Catalog.GetString ("Music Share"), device.FriendlyName, device.Udn, sort_order)
        {
            Hyena.Log.Information ("UPnPSource.Added(\"" + this.Name + "\", \"" + this.UniqueId + "\")");

            Properties.SetStringList ("Icon.Name", "computer", "network-server");

            // Remove tracks previously associated with this source, we do this to be sure they are non-existant before we refresh.
            PurgeTracks();

            AfterInitialized ();

            try
            {
                Container root = contentDirectory.GetRootObject();
                /*
                if (root.IsSearchable)
                {
                    Hyena.Log.Debug("UPnPSource: " + this.UniqueId + " have searchable root");

                    foreach (MusicTrack track in contentDirectory.Search<MusicTrack>(root, visitor => visitor.VisitAllResults(), new ResultsSettings()))
                        AddTrack(track);
                }
                else
                */
                {
                    Hyena.Log.Debug("UPnPSource: " + this.UniqueId + " does not contain a searchable root, need to recursive browse");

                    ParseContainer(contentDirectory, root, 0);
                }
            }
            catch (Exception exception)
            {
                Hyena.Log.DebugException(exception);
            }

            Hyena.Log.Information ("UPnPSource \"" + this.Name + "\", \"" + this.UniqueId + "\" parsed");
        }

        ~UPnPSource()
        {
            Dispose ();
        }

        public override void Dispose ()
        {
            Disconnect ();
            base.Dispose ();
        }

        public void Disconnect()
        {
            Hyena.Log.Information ("UPnPSource.Disconnect(\"" + this.Name + "\", \"" + this.UniqueId + "\")");

            // Stop currently playing track if its from us.
            try {
                if (ServiceManager.PlayerEngine.CurrentState == Banshee.MediaEngine.PlayerState.Playing) {
                    DatabaseTrackInfo track = ServiceManager.PlayerEngine.CurrentTrack as DatabaseTrackInfo;
                    if (track != null && track.PrimarySource == this) {
                        ServiceManager.PlayerEngine.Close ();
                    }
                }
            } catch {}

            // Remove tracks associated with this source, we will refetch them on next connect
            PurgeTracks();
        }

        public override bool CanDeleteTracks
        {
            get { return false; }
        }

        private void ParseContainer(RemoteContentDirectory contentDirectory, Container container, int depth)
        {
            if (depth > 10 || (container.ChildCount.HasValue && container.ChildCount == 0))
                return;
            
            foreach (var item in contentDirectory.GetChildren<Mono.Upnp.Dcp.MediaServer1.ContentDirectory1.Object>(container))
            {
                if (item is AudioItem)
                {
                    if (item is MusicTrack)
                        AddMusicTrack(item as MusicTrack);
                    else
                        AddAudioItem(item as AudioItem);
                }
                else if (item is Container)
                    ParseContainer(contentDirectory, item as Container, depth + 1);
            }
        }

        private void AddMusicTrack(MusicTrack basetrack)
        {
            UPnPTrackInfo track = new UPnPTrackInfo (basetrack, this);
            track.Save();
        }

        private void AddAudioItem(AudioItem basetrack)
        {
            UPnPTrackInfo track = new UPnPTrackInfo (basetrack, this);
            track.Save();
        }
    }
}
