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
    public class OsxFileChooserDialog : IBansheeFileChooser
    {
        public NSOpenPanel openPanel;
        public static IBansheeFileChooser CreateForImport (string title, bool files)
        {
            var chooser = new NSOpenPanel () {
                Title = title,
                CanChooseDirectories = !files,
                CanChooseFiles = files,
                AllowsMultipleSelection = true,
                // Translators: verb 
                Prompt = Mono.Unix.Catalog.GetString("Import")
            };
            return new OsxFileChooserDialog (title, chooser);
        }
        public OsxFileChooserDialog (string title, NSOpenPanel panel)
        {
            this.openPanel = panel;
        }
        public OsxFileChooserDialog (string title, Window parent, FileChooserAction action)
        {
            //LocalOnly = Banshee.IO.Provider.LocalOnly;
            //string fallback = SafeUri.FilenameToUri (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
            //SetCurrentFolderUri (LastFileChooserUri.Get (fallback));
            //WindowPosition = WindowPosition.Center;
        }

        #region Gtk.FileChooserDialog implementation
        public void Destroy ()
        {
            openPanel.Close ();
        }
        public int Run ()
        {
            int ret = openPanel.RunModal ();
            // TODO someday MonoMac should provide NSOKButton constant
            if (ret == 1)
                return (int) Gtk.ResponseType.Ok;
            else
                return (int) Gtk.ResponseType.Cancel;
        }
        #endregion

        #region IBansheeFileChooser implementation
        public string[] Filenames {
            get {
                return openPanel.Urls.Select (uri => SafeUri.UriToFilename (uri.ToString ())).ToArray<string> ();
            }
        }
        public string[] Uris {
            get {
                return openPanel.Urls.Select (uri => uri.ToString ()).ToArray<string> ();
            }
        }
        #endregion

        #region IBansheeFileChooser implementation
        public void AddFilter (FileFilter filter)
        {
            //penPanel.F
        }
        #endregion
        //public event EventHandler Close;
    }
}
