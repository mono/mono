//
// System.Threading.NativeOverlapped.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Threading
{
	public struct NativeOverlapped {
		public int EventHandle;
		public int InternalHigh;
		public int InternalLow;
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

