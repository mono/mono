
// System.Runtime.InteropServices/EXCEPINFO.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[ComVisible(false)]
	public struct EXCEPINFO {
		public string bstrDescription;
		public string bstrHelpFile;
		public string bstrSource;
		public int dwHelpContext;
		public IntPtr pfnDeferredFillIn;
		public IntPtr pvReserved;
		public short wCode;
		public short wReserved;
	}
}

