// System.EnterpriseServices.Internal.IComManagedImportUtil.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) 2002 Alejandro Sánchez Acosta
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("c3f8f66b-91be-4c99-a94f-ce3b0a951039")]
	public interface IComManagedImportUtil 
	{
		[DispId(4)] 
		void GetComponentInfo ([MarshalAs(UnmanagedType.BStr)] string assemblyPath, [MarshalAs(UnmanagedType.BStr)] out string numComponents, [MarshalAs(UnmanagedType.BStr)] out string componentInfo);
		[DispId(5)] 
		void InstallAssembly ([MarshalAs(UnmanagedType.BStr)] string filename, [MarshalAs(UnmanagedType.BStr)] string parname, [MarshalAs(UnmanagedType.BStr)] string appname);
	}
#endif
}
