//
// System.Runtime.InteropServices.ExporterEventKind.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices
{
	[Serializable] public enum ExporterEventKind
	{
		NOTIF_TYPECONVERTED = 0,
		NOTIF_CONVERTWARNING = 1,
		ERROR_REFTOINVALIDASSEMBLY = 2,	      
	}
}
