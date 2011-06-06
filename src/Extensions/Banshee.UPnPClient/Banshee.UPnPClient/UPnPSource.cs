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

            UPnPTrackInfo track = new UPnPTrackInfo (this);
            track.Save();
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
    }
}
