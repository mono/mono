// 
// System.EnterpriseServices.IRegistrationHelper.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Guid("55e3ea25-55cb-4650-8887-18e8d30bb4bc")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IRegistrationHelper {

		#region Methods

		void InstallAssembly ([In, MarshalAs(UnmanagedType.BStr)] string assembly, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string application, [In, Out, MarshalAs(UnmanagedType.BStr)] ref string tlb, [In] InstallationFlags installFlags);
		void UninstallAssembly ([In, MarshalAs (UnmanagedType.BStr)] string assembly, [In, MarshalAs (UnmanagedType.BStr)] string application);

		#endregion

	}
}
