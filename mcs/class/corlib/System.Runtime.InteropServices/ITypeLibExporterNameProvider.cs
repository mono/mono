//
// System.Runtime.InteropServices.ITypeLibExporterNameProvider.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices {

	//[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeLibExporterNameProvider {
		string[] GetNames ();

	}
}
