//
// System.Runtime.InteropServices.ITypeLibExporterNotifySink.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

using System.Reflection;

namespace System.Runtime.InteropServices {

	//[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeLibExporterNotifySink {
		void ReportEvent (ExporterEventKind eventKind, int eventCode, string eventMsg);
		object ResolveRef (Assembly assembly);
	}
}
