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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class HIObjectHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventHIObjectConstruct = 1;
		internal const uint kEventHIObjectInitialize = 2;
		internal const uint kEventHIObjectDestruct = 3;

		internal HIObjectHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr callref, IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			switch (kind) {
				case kEventHIObjectConstruct:
					IntPtr v = IntPtr.Zero;
					GetEventParameter (eventref, (uint)1751740265, (uint)1751740258, IntPtr.Zero, 4, IntPtr.Zero, ref v);
					return false;
				case kEventHIObjectInitialize:
					CallNextEventHandler (callref, eventref);
					return false;
				case kEventHIObjectDestruct:
					return false;
			}
			return false;
		}
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CallNextEventHandler (IntPtr callref, IntPtr eventref);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref IntPtr data);
	}
}
