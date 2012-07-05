// 
// Device.cs
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
using Banshee.Hardware;
using MonoMac.Foundation;
using System.Security.Cryptography;
using Banshee.Hardware.Osx.LowLevel;

namespace Banshee.Hardware.Osx
{
    public class Device : IDevice, IDisposable
    {
        // this is a low-level NSDictionary the OS X DiskArbitration framework
        // gives us back for any disk devices or volumes and holds ALL information
        // we need for a given device
        protected DeviceArguments deviceArguments;

        public Device (DeviceArguments arguments)
        {
            this.deviceArguments = arguments;
        }
        #region IDevice implementation
        public IUsbDevice ResolveRootUsbDevice ()
        {
            // TODO this should be refactored - devices don't need to be usb devices 
            // if one thin of firewire, thunderbolt, etc.
            if ((this as IUsbDevice) != null)
                return (IUsbDevice) this;
            else
                return null;
        }

        public IUsbPortInfo ResolveUsbPortInfo ()
        {
            return null;
        }
        public void Dispose ()
        {

        }
        public static string GetUUIDFromProperties (NSDictionary properties)
        {
             // this is somewhat troublesome
             // some devices have a filesystem UUID (i.e. HFS+ formated ones), but most other devices don't.
             // As the different devices/volumes have not really always a key in common, we use different keys
             // depending on the device type, and generate a UUID conforming 16byte value out of it

             string uuid_src = 
                properties.GetStringValue ("DAMediaBSDName") ??
                properties.GetStringValue ("DADevicePath")  ??
                properties.GetStringValue ("DAVolumePath");

            // TODO actually transform into a real UUID 
            return uuid_src;
        }
        public string Uuid {
            get {
                return GetUUIDFromProperties (deviceArguments.DeviceProperties);
            }
        }
        public string Serial {
            get {
                return "123456789";
            }
        }

        public string Name {
            get {
                return deviceArguments.DeviceProperties.GetStringValue ("DAMediaName");
            }
        }

        public string Product {
            get {
                return deviceArguments.DeviceProperties.GetStringValue("DADeviceModel");
            }
        }

        public string Vendor {
            get {
                return deviceArguments.DeviceProperties.GetStringValue("DADeviceVendor");
            }
        }

        public IDeviceMediaCapabilities MediaCapabilities {
            get {
                return null;
            }
        }
        #endregion

        }
}

