//
// System.Runtime.InteropServices.ComInterfaceType.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.InteropServices
{
	[Serializable] public enum ComInterfaceType
	{
		InterfaceIsDual = 0,
		InterfaceIsIUnknown = 1,
		InterfaceIsIDispatch = 2,		
	}
}
