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

		#endregion // Methods
	}
}
