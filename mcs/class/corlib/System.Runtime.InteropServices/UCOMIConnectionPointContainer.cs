//
// System.Runtime.InteropServices.UCOMIConnectionPointContainer.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIConnectionPointContainer
	{
		void EnumConnectionPoints (ref UCOMIEnumConnectionPoints ppEnum);
		void FindConnectionPoint (ref Guid riid, ref UCOMIConnectionPoint ppCP);
	}
}
