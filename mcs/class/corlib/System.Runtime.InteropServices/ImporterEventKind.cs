//
// System.Runtime.InteropServices.ImporterEventKind.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.InteropServices
{
	[Serializable] public enum ImporterEventKind
	{
		NOTIF_TYPECONVERTED = 0,
		NOTIF_CONVERTWARNING = 1,
		ERROR_REFTOINVALIDTYPELIB = 2,	      
	}
}
