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
	[Guid("89a86e7b-c229-4008-9baa-2f5c8411d7e0")]
	public sealed class RegistrationHelper : MarshalByRefObject, IRegistrationHelper {

		#region Constructors

		public RegistrationHelper ()
		{
		}

		#endregion

		#region Methods

		public void InstallAssembly (string assembly, ref string application, ref string tlb, InstallationFlags installFlags)
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

#if NET_1_1
		[MonoTODO]
		public void InstallAssemblyFromConfig (ref RegistrationConfig regConfig)
		{
			throw new NotImplementedException ();
		}
#endif

		public void UninstallAssembly (string assembly, string application)
		{
			UninstallAssembly (assembly, application, null);
		}

		[MonoTODO]
		public void UninstallAssembly (string assembly, string application, string partition)
		{
			throw new NotImplementedException ();
		}

#if NET_1_1
		[MonoTODO]
		public void UninstallAssemblyFromConfig (ref RegistrationConfig regConfig)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}
