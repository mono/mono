//
// System.Runtime.InteropServices.UCOMIEnumConnections.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumConnections
	{
		void Clone (ref UCOMIEnumConnections ppenum);
		int Next (int celt, CONNECTDATA[] rgelt, ref int pceltFetched);
		void Reset ();
		int Skip (int celt);
	}
}
