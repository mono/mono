
// System.Runtime.InteropServices/DISPPARAMS.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[ComVisible(false)]
	public struct DISPPARAMS {
		public int cArgs;
		public int cNamedArgs;
		public IntPtr rgdispidNamedArgs;
		public IntPtr rgvarg;
	}
}

