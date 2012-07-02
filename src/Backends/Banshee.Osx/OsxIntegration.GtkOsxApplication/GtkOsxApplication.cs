// 
// GtkOsxApplication.cs
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

using System;
using System.Runtime.InteropServices;
using Gtk;

namespace OsxIntegration.GtkOsxApplication
{
    /// <summary>
    /// Wraps the native GtkOsxApplicationBinding function to C# style API
    /// </summary>
    public class GtkOsxApplication
    {
        // Main application handle
        private IntPtr theApp;

        public GtkOsxApplication ()
        {
            IntPtr osx_app = gtk_osxapplication_get_type ();
            theApp = new GLib.GType (osx_app).Val;

        }

        // Takes the Gtk.MenuShell and sets it as OS X application menu
        public void SetMenu (MenuShell shell)
        {
            gtk_osxapplication_set_menu_bar (theApp, shell.Handle);
        }

        // Places MenuItems into the OS X specific "Application" menu
        // (the menu right next to the Apple-Menu)
        // It's convention on OS X to put the about and the preferences
        // dialog in there
        public void InsertIntoAppMenu (MenuItem item, int index)
        {
            gtk_osxapplication_insert_app_menu_item (theApp, item.Handle, index);
            gtk_osxapplication_sync_menubar (theApp);
        }

        public void SetWindowMenu (MenuItem item)
        {
            gtk_osxapplication_set_window_menu (theApp, item.Handle);
            gtk_osxapplication_sync_menubar (theApp);
        }
        public void Ready ()
        {
            gtk_osxapplication_ready (theApp);
        }

        // Bindings against native gtk-mac-integration/GtkOSXApplication
        // which uses cocoa instead of deprecated carbon
        // for documentation of these functions, see:
        // http://gtk-osx.sourceforge.net/ige-mac-integration/GtkOSXApplication.html

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static IntPtr gtk_osxapplication_get_type ();

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_ready (IntPtr app);

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_set_menu_bar (IntPtr app, IntPtr menu_shell);

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_insert_app_menu_item (IntPtr app, IntPtr menu_item, int index);

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_sync_menubar (IntPtr app);

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_set_dock_menu  (IntPtr app, IntPtr menu_shell);

        [DllImport ("libgtkmacintegration.dylib")]
        protected extern static void gtk_osxapplication_set_window_menu (IntPtr app, IntPtr menu_item);

        // TODO add more functions from GtkOsxApplication

    }
}