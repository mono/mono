// 
// System.EnterpriseServices.RegistrationHelper.cs
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
	public sealed class RegistrationHelper : MarshalByRefObject, IRegistrationHelper {

		#region Constructors

		public RegistrationHelper ()
		{
		}

		#endregion

		#region Methods

		public void InstallAssembly (string assembly, out string application, out string tlb, InstallationFlags installFlags)
		{
			application = String.Empty;
			tlb = String.Empty;

			InstallAssembly (assembly, ref application, null, ref tlb, installFlags);
		}

		[MonoTODO]
		public void InstallAssembly (string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags)
		{
			throw new NotImplementedException ();
		}

		public void UninstallAssembly (string assembly, string application)
		{
			UninstallAssembly (assembly, application, null);
		}

		[MonoTODO]
		public void UninstallAssembly (string assembly, string application, string partition)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
