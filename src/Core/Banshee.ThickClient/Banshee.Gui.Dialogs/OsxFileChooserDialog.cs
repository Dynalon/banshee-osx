//
// FileChooserDialog.cs
//
// Author:
//   Timo Dörr <timo@latecrew.de>
//
// Copyright (C) 2006-2007 Novell, Inc.
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

using Banshee.Configuration;
using Banshee.ServiceStack;
using Hyena;
using MonoMac.AppKit;
using Gtk;

using System.Linq;
using System.Collections.Generic;

namespace Banshee.Gui.Dialogs
{
    public partial class FileChooserDialog : Gtk.FileChooser
    {
        public NSOpenPanel openPanel;
        public static FileChooserDialog CreateForImport (string title, bool files)
        {
            var chooser = new NSOpenPanel () {
                Title = title,
                CanChooseDirectories = !files,
                CanChooseFiles = files,
                AllowsMultipleSelection = true,
                // Translators: verb 
                Prompt = Mono.Unix.Catalog.GetString("Import")
            };
            var fcd = new FileChooserDialog (title, chooser);
            fcd.openPanel = chooser;
            return fcd;
        }
        public FileChooserDialog (string title, NSOpenPanel panel)
        {
            this.openPanel = panel;
        }
        public FileChooserDialog (string title, Window parent, FileChooserAction action)
        {
            //LocalOnly = Banshee.IO.Provider.LocalOnly;
            //string fallback = SafeUri.FilenameToUri (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
            //SetCurrentFolderUri (LastFileChooserUri.Get (fallback));
            //WindowPosition = WindowPosition.Center;
        }

        public static string GetPhotosFolder ()
        {
            string personal = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
            string desktop = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);

            var photo_folders = new string [] {
                Environment.GetFolderPath (Environment.SpecialFolder.MyPictures),
                Paths.Combine (desktop, "Photos"), Paths.Combine (desktop, "photos"),
                Paths.Combine (personal, "Photos"), Paths.Combine (personal, "photos")
            };

            // Make sure we don't accidentally scan the entire home or desktop directory
            for (int i = 0; i < photo_folders.Length; i++) {
                if (photo_folders[i] == personal || photo_folders[i] == desktop) {
                    photo_folders[i] = null;
                }
            }

            foreach (string folder in photo_folders) {
                if (folder != null && folder != personal && folder != desktop && Banshee.IO.Directory.Exists (folder)) {
                    return folder;
                }
            }

            return null;
        }

        protected void OnResponse (ResponseType response)
        {
            //base.OnResponse (response);

            //if (CurrentFolderUri != null) {
             //   LastFileChooserUri.Set (CurrentFolderUri);
            //}
        }

        public static readonly SchemaEntry<string> LastFileChooserUri = new SchemaEntry<string> (
            "player_window", "last_file_chooser_uri",
            String.Empty,
            "URI",
            "URI of last file folder"
        );
        #region Gtk.FileChooserDialog implementation
        public void Destroy ()
        {
        }
        public int Run ()
        {
            int ret = openPanel.RunModal ();
            if (ret != 1) ret =1;
            return ret;
        }
        #endregion
        #region IWrapper implementation
        public IntPtr Handle {
            get {
                throw new System.NotImplementedException ();
            }
        }
        public void Close ()
        {

        }
        #endregion

        #region FileChooser implementation
        public event EventHandler SelectionChanged;

        public event ConfirmOverwriteHandler ConfirmOverwrite;

        public event EventHandler FileActivated;

        public event EventHandler UpdatePreview;

        public event EventHandler CurrentFolderChanged;

        public bool SetCurrentFolderUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public void UnselectAll ()
        {
            throw new System.NotImplementedException ();
        }

        public bool SetFilename (string filename)
        {
            throw new System.NotImplementedException ();
        }

        public bool AddShortcutFolderUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public void SelectAll ()
        {
            throw new System.NotImplementedException ();
        }

        public bool RemoveShortcutFolderUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public void RemoveFilter (FileFilter filter)
        {
            throw new System.NotImplementedException ();
        }

        public bool AddShortcutFolder (string folder)
        {
            throw new System.NotImplementedException ();
        }

        public bool SelectFilename (string filename)
        {
            throw new System.NotImplementedException ();
        }

        public void UnselectFilename (string filename)
        {
            throw new System.NotImplementedException ();
        }

        public void AddFilter (FileFilter filter)
        {
            throw new System.NotImplementedException ();
        }

        public bool RemoveShortcutFolder (string folder)
        {
            throw new System.NotImplementedException ();
        }

        public void UnselectUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public bool SetCurrentFolder (string filename)
        {
            throw new System.NotImplementedException ();
        }

        public bool SelectUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public bool SetUri (string uri)
        {
            throw new System.NotImplementedException ();
        }

        public string[] ShortcutFolderUris {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public FileFilter[] Filters {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string CurrentFolderUri {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string CurrentName {
            set {
                throw new System.NotImplementedException ();
            }
        }

        public string PreviewFilename {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string Uri {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string CurrentFolder {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string[] Uris {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string Filename {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public string PreviewUri {
            get {
                throw new System.NotImplementedException ();
            }
        }

        public bool UsePreviewLabel {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public bool PreviewWidgetActive {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public FileChooserAction Action {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public bool DoOverwriteConfirmation {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public Widget ExtraWidget {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public bool ShowHidden {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public bool LocalOnly {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public FileFilter Filter {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public Widget PreviewWidget {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public bool SelectMultiple {
            get {
                throw new System.NotImplementedException ();
            }
            set {
                throw new System.NotImplementedException ();
            }
        }

        public string[] Filenames {
            get {

                List<string> filenames = new List<string> ();
                foreach (var url in openPanel.Urls) {
                    filenames.Add (url.ToString ());
                }
                return filenames.ToArray ();
            }
        }

        public string[] ShortcutFolders {
            get {
                throw new System.NotImplementedException ();
            }
        }
        #endregion

    }
}
