//
// OsxDiskArbiter.cs
//
// Author:
//   Timo Dörr <timo@latecrew.de>
//
// Copyright (C) 2012 Timo Dörr
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
using System.Runtime.InteropServices;

using System.Linq;

using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Threading;
using MonoMac.ObjCRuntime;

namespace Banshee.Hardware.Osx.LowLevel
{
	// missing pieces that are not present in MonoMac.CoreFoundation
	internal class CoreFoundationWrapper
	{
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern IntPtr CFAllocatorGetDefault ();

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern IntPtr CFRunLoopGetCurrent ();

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern IntPtr CFRunLoopCopyCurrentMode (IntPtr runloop);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern void CFRunLoopRun ();

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern void CFRunLoopStop (IntPtr runloop);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern IntPtr CFURLCopyFileSystemPath (IntPtr url, uint style);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern void CFRelease (IntPtr ptr);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern bool CFNumberGetValue (IntPtr number, int numberType, out Int32 val);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern void CFShow (IntPtr obj);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		public static extern IntPtr CFStringCreateWithCString (IntPtr number, string str, int encoding);


	}

	/// <summary>
	/// Wrapper against the OS X IOKit framework.
	/// Especially helpful for is the "IORegistryExplorer" program that ships with Xcode to browse
	/// connected devices and review their properties.
	/// </summary>

	/// <summary>
	/// NS dictionary helper. Allows easier access of keys in the semi-native NSDictionary
	/// </summary>
		/// <summary>
	/// NS dictionary helper. Allows easier access of keys in the semi-native NSDictionary
	/// </summary>
		
}