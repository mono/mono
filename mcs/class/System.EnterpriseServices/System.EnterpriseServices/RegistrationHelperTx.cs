// 
// System.EnterpriseServices.RegistrationHelperTx.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {
	[Guid("9e31421c-2f15-4f35-ad20-66fb9d4cd428")]
	public sealed class RegistrationHelperTx : ServicedComponent {

		#region Constructors

		[MonoTODO]
		public RegistrationHelperTx ()
		{
		}

		#endregion

		#region Methods

		[MonoTODO]
		protected internal override void Activate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Deactivate ()
		{
			throw new NotImplementedException ();
		}

		public void InstallAssembly (string assembly, ref string application, ref string tlb, InstallationFlags installFlags, object sync)
		{
			InstallAssembly (assembly, ref application, null, ref tlb, installFlags, sync);
		}

		[MonoTODO]
		public void InstallAssembly (string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags, object sync)
		{
			throw new NotImplementedException ();
		}

#if NET_1_1
		[MonoTODO]
		public void InstallAssemblyFromConfig (ref RegistrationConfig regConfig, object sync)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public bool IsInTransaction ()
		{
			throw new NotImplementedException ();
		}

		public void UninstallAssembly (string assembly, string application, object sync)
		{
			UninstallAssembly (assembly, application, null, sync);
		}

		[MonoTODO]
		public void UninstallAssembly (string assembly, string application, string partition, object sync)
		{
			throw new NotImplementedException ();
		}

#if NET_1_1
		[MonoTODO]
		public void UninstallAssemblyFromConfig (ref RegistrationConfig regConfig, object sync)
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods
	}
}
