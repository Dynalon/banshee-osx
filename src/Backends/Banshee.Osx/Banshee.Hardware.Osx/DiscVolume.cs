// 
// DiscVolume.cs
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
using MonoMac.Foundation;
using Banshee.Hardware.Osx.LowLevel;


namespace Banshee.Hardware.Osx
{

    public class DiscVolume : Volume, IDiscVolume
    {
        public DiscVolume (DeviceArguments arguments, IBlockDevice b) : base(arguments, b)
        {
        }
        #region IDiscVolume implementation
        public bool HasAudio {
            get {
                return true;
            }
        }

        public bool HasData {
            get {
                return false;
            }
        }

        public bool HasVideo {
            get {
                return false;
            }
        }

        public bool IsRewritable {
            get {
                return false;
            }
        }

        public bool IsBlank {
            get {
                return false;
            }
        }

        public ulong MediaCapacity {
            get {
                return 128338384858;
            }
        }
        #endregion
    }
}
