//
// System.Runtime.InteropServices.UCOMIEnumString.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("00000101-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumString
	{
		void Clone (out UCOMIEnumString ppenum);
		int Next (int celt, [Out] string[] rgelt, out int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
