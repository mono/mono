//
// System.Runtime.InteropServices.UCOMIEnumConnectionPoints.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumConnectionPoints
	{
		void Clone (ref UCOMIEnumConnectionPoints ppenum);
		int Next (int celt, UCOMIConnectionPoint[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
