//
// FileChooserDialog.cs
//
// Author:
//   Timo Dörr <timo@latecrew.de>
//
// Copyright 2012 Timo Dörr
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

using System.Linq;
using System;

namespace Banshee.Gui.Dialogs
{
    public class FileChooserDialog : IBansheeFileChooser
    {
        protected IBansheeFileChooser nativeChooser;

        static FileChooserDialog ()
        {
            // initialization within static constructor
            // get available backend dialogs
            // if non available, use GtkFileChooser
            //var s = new GtkFileChooserDialog ();
        }
        #region IBansheeFileChooser implementation
        public int Run ()
        {
            return nativeChooser.Run ();
        }

        public void Destroy ()
        {
            nativeChooser.Destroy ();
        }

        public string[] Filenames {
            get {
                return nativeChooser.Filenames;
            }
        }
        public static IBansheeFileChooser CreateForImport (string title, bool files)
        {
            // fallback to GtkFileChoooser
            if (Hyena.PlatformDetection.IsMac)
                return OsxFileChooserDialog.CreateForImport (title, files);
            else
                return GtkFileChooserDialog.CreateForImport (title, files);

        }
        public void AddFilter (Gtk.FileFilter filter)
        {
            nativeChooser.AddFilter (filter);
        }
        public string[] Uris 
        {
            get {
                return nativeChooser.Uris;
            }
        }
        /*public event EventHandler Close {
            add {
                nativeChooser.Close += value;
            }
            remove {
                nativeChooser.Close -= value;
            }
        } */
        #endregion
  


    }
    public interface IBansheeFileChooser
    {
        int Run ();
        void Destroy ();
        //event EventHandler Close;
        string[] Filenames { get; }
        string[] Uris { get; }
        void AddFilter (Gtk.FileFilter filter);
    }
}