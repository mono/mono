//
// System.Runtime.InteropServices.ITypeLibConverter.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices {

	[Guid("F1C3BF78-C3E4-11D3-88E7-00902754C43A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeLibConverter {
		object ConvertAssemblyToTypeLib (Assembly assembly, string typeLibName, TypeLibExporterFlags flags, ITypeLibExporterNotifySink notifySink);
		AssemblyBuilder ConvertTypeLibToAssembly (object typeLib, string asmFileName, int flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, bool unsafeInterfaces);
		AssemblyBuilder ConvertTypeLibToAssembly (object typeLib, string asmFileName, TypeLibImporterFlags flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, string asmNamespace, Version asmVersion);
		bool GetPrimaryInteropAssembly (Guid g, int major, int minor, int lcid, out string asmName, out string asmCodeBase);
	}
}
