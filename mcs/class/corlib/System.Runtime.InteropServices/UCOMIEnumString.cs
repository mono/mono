//
// System.Runtime.InteropServices.UCOMIEnumString.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumString
	{
		void Clone (ref UCOMIEnumString ppenum);
		int Next (int celt, string[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
