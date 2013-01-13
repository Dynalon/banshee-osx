// 
// SymbianDevice.cs
// 
// Author:
//   Nicholas Little <arealityfarbetween@googlemail.com>
// 
// Copyright (C) 2012 Nicholas Little
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Banshee.IO;
using Banshee.Hardware;
using Banshee.Sources;
using Hyena;

namespace Banshee.Dap.MassStorage
{
    public class SymbianDevice : CustomMassStorageDevice
    {
        #region MassStorageDevice Property Overrides
        private static string [] icons = {
                "phone-nokia-n95",
                DapSource.FallbackIcon
        };
        public override string [] GetIconNames ()
        {
            return icons;
        }

        private Uri root_path = new Uri ("E:\\");
        internal override Uri RootPath {
            get { return root_path; }
        }

        protected override string DefaultFolderSeparator {
            get { return Paths.Folder.DosSeparator.ToString (); }
        }

        private static string [] playlist_formats = { "audio/x-mpegurl" };
        protected override string[] DefaultPlaylistFormats {
            get { return playlist_formats; }
        }

        private static string [] audio_folders = { "Music/" };
        protected override string [] DefaultAudioFolders {
            get { return audio_folders; }
        }

        protected override int DefaultFolderDepth {
            get { return 2; }
        }

        protected override string DefaultPlaylistPath {
            get { return "Playlists/"; }
        }

        private static string [] video_folders = { "My Videos/" };
        protected override string [] DefaultVideoFolders {
            get { return video_folders; }
        }

        private static string [] playback_mime_types = {
            "audio/mp3",
            "video/mp4"
        };
        protected override string [] DefaultPlaybackMimeTypes {
            get { return playback_mime_types; }
        }

        private string default_name;
        protected override string DefaultName {
            get {
                if (string.IsNullOrEmpty (default_name))
                    default_name = string.Format ("{0} {1}",
                                                  VendorProductInfo.VendorName,
                                                  VendorProductInfo.ProductName);
                return default_name;
            }
        }

        public override bool LoadDeviceConfiguration ()
        {
            LoadConfig (null);
            return true;
        }
        #endregion
    }
}

