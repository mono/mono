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
	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IRegistrationHelper {

		#region Methods

		void InstallAssembly (string assembly, out string application, out string tlb, InstallationFlags installFlags);
		void UninstallAssembly (string assembly, string application);

		#endregion

	}
}
