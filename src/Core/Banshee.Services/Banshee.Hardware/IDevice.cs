//
// IDevice.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
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

namespace Banshee.Hardware
{
    public interface IDevice
    {
        string Uuid { get; }
        string Serial { get; }
        string Name { get; }

        string Product { get; }
        string Vendor { get; }

        IDeviceMediaCapabilities MediaCapabilities { get; }

#if LEGACY_IPOD_SUPPORT
        bool PropertyExists (string key);

        string GetPropertyString (string key);
        double GetPropertyDouble (string key);
        bool GetPropertyBoolean (string key);
        int GetPropertyInteger (string key);
        ulong GetPropertyUInt64 (string key);
        string [] GetPropertyStringList (string key);
#endif

        IUsbDevice ResolveRootUsbDevice ();
        IUsbPortInfo ResolveUsbPortInfo ();
    }
}
