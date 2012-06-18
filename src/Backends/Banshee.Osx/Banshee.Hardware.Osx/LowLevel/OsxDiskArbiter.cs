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
	public delegate void DiskAppearedHandler (object o, DeviceArguments args);
	public delegate void DiskDisappearedHandler (object o, DeviceArguments args);
	public delegate void DiskDescriptionChangedHandler (object o, DeviceArguments args);

	public class DeviceArguments
	{
		public DeviceArguments (NSDictionary properties)
		{
			DeviceProperties = properties;
		}
		public DeviceArguments (NSDictionary properties, OsxUsbData usbdata) : this (properties)
		{
			UsbInfo = usbdata;
		}
		public NSDictionary DeviceProperties;
		public OsxUsbData UsbInfo;
	}

	/// <summary>
	/// Wrapper against the OS X DiskArbitation framework. Some very usefull links for a better understanding:
	/// <see href="http://www.thoughtstuff.com/rme/weblog/?p=3">This blog</see>,
	/// <see href="http://www.cocoaintheshell.com/2011/03/dadiskmountapprovalcallback-double-callback/">this</see>,
	/// <see href="http://joubert.posterous.com/notification-when-usb-storage-device-is-conne">this</see> as well as
	/// <see href="http://developer.apple.com/library/mac/#documentation/Darwin/Reference/DiscArbitrationFramework/DiskArbitration_h/index.html">
	/// Apples DiskArbitation framework documentation</see>,
	/// <see href="http://developer.apple.com/library/mac/#samplecode/USBPrivateDataSample/Introduction/Intro.html">This C sample</see>,
	/// and your local copy of /System/Library/Framework/DiskArbitration/DiskArbitration.h.
	/// Especially helpful for is the "IORegistryExplorer" program that ships with Xcode.
	/// </summary>
	public class OsxDiskArbiter : IDisposable
	{
		public NSAutoreleasePool pool;
		public OsxDiskArbiter () 
		{
			pool = new NSAutoreleasePool ();
		}
		/// <summary>
		/// Called when a disk/volume "appears" to the system. This can be an USB Drive plugged in, as well as network volumes, FUSE filesystems etc.
		/// Note that this callback is triggered for the main disk (i.e. The USB Drive itself) as well as all Volumes on it (i.e. all partitions on an USB
		/// drive).
		/// </summary>
		/// <remarks>This function should not be used to retrieve the mountpoint of a newly plugged disk/volume.
		/// It is totally indeterministic whether at the time the callback fires the disk already is mounted
		/// in the system, and the DAVolumePath field may be null. Use <see cref='DiskDescriptionChanged'/>
		/// instead.
		/// </remarks>
		public event DiskAppearedHandler DeviceAppeared;
		/// <summary>
		/// Occurs when device disappeared, i.e. is disconnected or unmounted.
		/// </summary>
		public event DiskDisappearedHandler DeviceDisappeared;
		/// <summary>
		/// Occurs when device description changed. This event should be used to watch for newly added usb sticks/drives, as it will have the DAVolumePath
		/// (=mountpoint) in it.
		/// </summary>
		public event DiskDescriptionChangedHandler DeviceChanged;

        private delegate void DiskAppearedCallback (IntPtr diskRef, IntPtr context);
        private delegate void DiskDisappearedCallback (IntPtr diskRef, IntPtr context);
        private delegate void DiskChangedCallback (IntPtr diskRef, IntPtr keys, IntPtr context);

		private Thread listenThread;
		private IntPtr da_session;
		private IntPtr runloop;

		private IntPtr callback_appeared;
		private IntPtr callback_disappeared;
		private IntPtr callback_changed;

		public void StartListening ()
		{
			listenThread = new Thread( () => {
				using (var arp = new NSAutoreleasePool ()) {
					startArbiter ();
				}
			});
			listenThread.Start ();
		}


		/// <summary>
		/// Called when a disk/volume "appears" to the system. This can be an USB Drive plugged in, as well as network volumes, FUSE filesystems etc.
		/// Note that this callback is triggered for the main disk (i.e. The USB Drive itself) as well as all Volumes on it (i.e. all partitions on an USB
		/// drive).
		/// </summary>
		/// 
		/// <param name='disk'>
		///  A reference of type DADiskRef
		/// </param>
		/// <param name='context'>
		/// Application-specific context. Currently not in use.
		/// </param>
		/// 
		/// <remarks>This function should not be used to retrieve the mountpoint of a newly plugged disk/volume.
		/// It is totally indeterministic whether at the time the /// callback fires the disk already is mounted
		/// in the system, and the DAVolumePath field may be null. Use <see cref='DiskDescriptionChanged'/>
		/// instead.
		/// </remarks>
		private void NativeDiskAppeared (IntPtr disk, IntPtr context)

		{
            if (this.DeviceAppeared == null)
                // if no-one subscribed to this event, do nothing
                return;

			IntPtr device = DADiskCopyIOMedia (disk);
			IntPtr propertiesRef = DADiskCopyDescription (disk);
		
			// using MonoMac we can get a managed NSDictionary from the pointer
			NSDictionary properties = new NSDictionary (propertiesRef);
			DeviceArguments deviceArguments = new DeviceArguments (properties);

			// get usb data
			if (properties.HasKey ("DADeviceProtocol") && properties.GetStringValue ("DADeviceProtocol") == "USB") {
				OsxUsbData usb = new OsxUsbData (device);
				deviceArguments.UsbInfo = usb;
			}
			IOKit.IOObjectRelease (device);

			// trigger the public event for any subscribers
			this.DeviceAppeared (this, deviceArguments);
            GC.KeepAlive (this);
		}
		private void NativeDiskChanged (IntPtr disk, IntPtr keys, IntPtr context)
		{
            if (this.DeviceChanged == null)
                // if no-one subscribed to this event, do nothing
                return;

			IntPtr device = DADiskCopyIOMedia (disk);
			IntPtr propertiesRef = DADiskCopyDescription (disk);


			// using MonoMac we can get a managed NSDictionary from the pointer
			NSDictionary properties = new NSDictionary (propertiesRef);
			DeviceArguments deviceArguments = new DeviceArguments (properties);

			if (properties.HasKey ("DADeviceProtocol") && properties.GetStringValue ("DADeviceProtocol") == "USB") {
				OsxUsbData usb = new OsxUsbData (device);
				deviceArguments.UsbInfo = usb;
			}

			IOKit.IOObjectRelease (device);

			// trigger the public event for any subscribers
			this.DeviceChanged (this, deviceArguments);
            GC.KeepAlive (this);
		}
		private void NativeDiskDisappeared (IntPtr disk, IntPtr context)
		{
             if (this.DeviceDisappeared == null)
                // if no-one subscribed to this event, do nothing
                return;

			IntPtr device = DADiskCopyIOMedia (disk);
			IntPtr propertiesRef = DADiskCopyDescription (disk);

			NSDictionary properties = new NSDictionary (propertiesRef);

				
			DeviceArguments deviceArguments = new DeviceArguments (properties);

			if (properties.HasKey ("DADeviceProtocol") && properties.GetStringValue ("DADeviceProtocol") == "USB") {
				OsxUsbData usb = new OsxUsbData (device);
				deviceArguments.UsbInfo = usb;
			}

			IOKit.IOObjectRelease (device);

			this.DeviceDisappeared (this, deviceArguments);
            GC.KeepAlive (this);
		}
        private void startArbiter ()
		{
            DiskAppearedCallback disk_appeared_callback = new DiskAppearedCallback (NativeDiskAppeared);
            DiskChangedCallback disk_changed_callback = new DiskChangedCallback (NativeDiskChanged);
            DiskDisappearedCallback disk_disappeared_callback = new DiskDisappearedCallback (NativeDiskDisappeared);

			// create a DiskArbitration session
			IntPtr allocator = CoreFoundationWrapper.CFAllocatorGetDefault ();
            da_session = DASessionCreate (allocator);

            this.callback_appeared = Marshal.GetFunctionPointerForDelegate (disk_appeared_callback);
            this.callback_changed = Marshal.GetFunctionPointerForDelegate (disk_changed_callback);
            this.callback_disappeared = Marshal.GetFunctionPointerForDelegate (disk_disappeared_callback);

            DARegisterDiskAppearedCallback (da_session, IntPtr.Zero, callback_appeared, IntPtr.Zero);
            DARegisterDiskDescriptionChangedCallback (da_session, IntPtr.Zero, IntPtr.Zero, callback_changed, IntPtr.Zero);
			DARegisterDiskDisappearedCallback (da_session, IntPtr.Zero, callback_disappeared, IntPtr.Zero);

			//IntPtr runloop = CFRunLoopGetCurrent ();
			runloop = MonoMac.CoreFoundation.CFRunLoop.Current.Handle;

			var mode = MonoMac.CoreFoundation.CFRunLoop.CFDefaultRunLoopMode.Handle;
			DASessionScheduleWithRunLoop (da_session, runloop, mode);

			// this blocks the thread
			CoreFoundationWrapper.CFRunLoopRun ();

            // this code is actually never run, but keeps our native references
            // and callbacks alive to prevent the GC from removing it
            GC.KeepAlive (allocator);
            GC.KeepAlive (da_session);
            GC.KeepAlive (callback_changed);
            GC.KeepAlive (callback_appeared);
            GC.KeepAlive (callback_disappeared);
            GC.KeepAlive (disk_appeared_callback);
            GC.KeepAlive (disk_changed_callback);
            GC.KeepAlive (disk_disappeared_callback);
		}
		public void Dispose ()
		{
			// unregister our callbacks
			DAUnregisterCallback (da_session, callback_appeared, IntPtr.Zero);
			DAUnregisterCallback (da_session, callback_changed, IntPtr.Zero);
			DAUnregisterCallback (da_session, callback_disappeared, IntPtr.Zero);

			var mode = MonoMac.CoreFoundation.CFRunLoop.CFDefaultRunLoopMode.Handle;
			DASessionUnscheduleFromRunLoop (da_session, runloop, mode);
			CoreFoundationWrapper.CFRelease (da_session);

			// stop the main run loop which blocks the thread
			CoreFoundationWrapper.CFRunLoopStop (runloop);
			listenThread.Join ();
            GC.SuppressFinalize (this);
		}
        // we need to map the volumeUrl's to pathes
        // like file://localhost/Volumes/Mountpoint -> /Volumes/MountPoint
        public static string UrlToFileSystemPath (string url, uint mode = 0) {
            if (url == null) throw new ArgumentException ("url cannot be null");
            using (var arp = new NSAutoreleasePool ()){
                NSUrl nsurl =  new NSUrl (url);
                NSString path = new NSString (
					CoreFoundationWrapper.CFURLCopyFileSystemPath (nsurl.Handle, mode)
				);
                return path.ToString ();
            }
        }  

        private const string DiskArbitrationLibrary = "/SystemS/Library/Frameworks/DiskArbitration.framework/DiskArbitration";
		// BEGIN native functions


		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DASessionCreate (IntPtr allocator);
	
		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DARegisterDiskAppearedCallback (IntPtr session, IntPtr match, IntPtr callback, IntPtr context);
	
		[DllImport (DiskArbitrationLibrary)]
        private static extern IntPtr DARegisterDiskDescriptionChangedCallback (IntPtr session, IntPtr match, IntPtr watch, IntPtr callback, IntPtr context);
	
		[DllImport (DiskArbitrationLibrary)]
        private static extern IntPtr DARegisterDiskDisappearedCallback (IntPtr session, IntPtr match, IntPtr callback, IntPtr context);
 		
		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DAUnregisterCallback (IntPtr session, IntPtr callback, IntPtr context);

		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DASessionScheduleWithRunLoop (IntPtr session , IntPtr runLoop , IntPtr runloopMode);
	
		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DASessionUnscheduleFromRunLoop (IntPtr session , IntPtr runLoop , IntPtr runloopMode);

		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DADiskCopyDescription (IntPtr disk);

		[DllImport (DiskArbitrationLibrary)]
		public static extern IntPtr DADiskCopyIOMedia (IntPtr disk);
	}

	/// <summary>
	/// NS dictionary helper. Allows easier access of keys in the semi-native NSDictionary
	/// </summary>
		/// <summary>
	/// NS dictionary helper. Allows easier access of keys in the semi-native NSDictionary
	/// </summary>
	public static class NSDictionaryHelper {
		public static string GetStringValue (this NSDictionary dict, string key)
		{
            using (var arp = new NSAutoreleasePool ()) {
			var searchKey = dict.Keys.Where (k => k.ToString () == key).FirstOrDefault ();
				if (searchKey == null)
					return null;
				else
					return dict[searchKey].ToString ();
            }
		}
		public static bool HasKey (this NSDictionary dict, string key)
		{
			using (var arp = new NSAutoreleasePool ()) {
				var searchKey = dict.Keys.Where (k => k.ToString () == key).FirstOrDefault ();
				return (searchKey != null) ? true : false;
			}
		}

	}	
}