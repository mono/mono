//
// System.Runtime.InteropServices.ITypeLibExporterNameProvider.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices {

	[Guid("fa1f3615-acb9-486d-9eac-1bef87e36b09")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeLibExporterNameProvider {
		string[] GetNames ();

	}
}
