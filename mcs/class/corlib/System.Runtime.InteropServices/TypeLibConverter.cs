//
// System.Runtime.InteropServices.TypeLibConverter.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices
{
	[Guid ("f1c3bf79-c3e4-11d3-88e7-00902754c43a")]
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class TypeLibConverter : ITypeLibConverter
	{
		public TypeLibConverter ()
		{
		}

		[MonoTODO ("implement")]
		public object ConvertAssemblyToTypeLib (Assembly assembly, string strTypeLibName, TypeLibExporterFlags flags, ITypeLibExporterNotifySink notifySink)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public AssemblyBuilder ConvertTypeLibToAssembly (object typeLib, string asmFileName, int flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, bool unsafeInterfaces)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public AssemblyBuilder ConvertTypeLibToAssembly (object typeLib, string asmFileName, TypeLibImporterFlags flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, string asmNamespace, Version asmVersion)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public bool GetPrimaryInteropAssembly (Guid g, int major, int minor, int lcid, out string asmName, out string asmCodeBase)
		{
			throw new NotImplementedException ();
		}	
	}
}
