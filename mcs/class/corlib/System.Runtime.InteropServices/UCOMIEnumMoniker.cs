//
// System.Runtime.InteropServices.UCOMIEnumMoniker.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("00000102-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumMoniker
	{
		void Clone (ref UCOMIEnumMoniker ppenum);
		int Next (int celt, out UCOMIMoniker[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
