//
// System.Runtime.InteropServices.UCOMIEnumMoniker.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumMoniker
	{
		void Clone (ref UCOMIEnumMoniker ppenum);
		int Next (int celt, UCOMIMoniker[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
