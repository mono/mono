
// System.Runtime.InteropServices/BINDPTR.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[ComVisible(false)]
	public struct BINDPTR {
		public IntPtr lpfuncdesc;
		public IntPtr lptcomp;
		public IntPtr lpvardesc;
	}
}

