//
// System.Runtime.InteropServices.UCOMIConnectionPointContainer.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("b196b284-bab4-101a-b69c-00aa00341d07")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIConnectionPointContainer
	{
		void EnumConnectionPoints (ref UCOMIEnumConnectionPoints ppEnum);
		void FindConnectionPoint (ref Guid riid, ref UCOMIConnectionPoint ppCP);
	}
}
