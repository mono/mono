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
		public GCHandle ReservedClassLib;
		public int ReservedCOR1;
		public GCHandle ReservedCOR2;
	}
}
