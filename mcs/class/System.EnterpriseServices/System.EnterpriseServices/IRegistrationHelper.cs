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

		void InstallAssembly ([In] string assembly, [In, Out] ref string application, [In, Out] ref string tlb, [In] InstallationFlags installFlags);
		void UninstallAssembly ([In] string assembly, [In] string application);

		#endregion

	}
}
