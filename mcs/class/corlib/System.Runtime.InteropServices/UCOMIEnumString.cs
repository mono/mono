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
		void Clone (ref UCOMIEnumString ppenum);
		int Next (int celt, out string[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
