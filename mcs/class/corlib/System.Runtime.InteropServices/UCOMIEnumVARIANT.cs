//
// System.Runtime.InteropServices.UCOMIEnumVARIANT.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumVARIANT
	{
		void Clone (int ppenum);
		int Next (int celt, int rgvar, int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
