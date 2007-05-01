//
// System.Threading.NativeOverlapped.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Threading
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public struct NativeOverlapped {
#if NET_2_0
		public IntPtr EventHandle;
		public IntPtr InternalHigh;
		public IntPtr InternalLow;
#else
		public int EventHandle;
		public int InternalHigh;
		public int InternalLow;
#endif
		public int OffsetHigh;
		public int OffsetLow;

		// (fields disappeared beta2 -> 1.0)
		// public GCHandle ReservedClassLib;
		// public int ReservedCOR1;
		// public GCHandle ReservedCOR2;

		// P.S. (Gonzalo): try this:
                //	Console.WriteLine (Marshal.SizeOf (typeof (NativeOverlapped)));
		//
		// And you'll get a nice 36. So probably those fields are out there but are not public.
		// So I'm adding some internal fields that are used in the runtime
		internal int Handle1;
		internal int Handle2;
	}
}

