//
// System.Security.Permissions.IsolatedStorageFilePermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class IsolatedStorageFilePermission : IsolatedStoragePermission {

		// Constructors

		public IsolatedStorageFilePermission (PermissionState state) : base (state) {}

		// Properties

		// Methods

		[MonoTODO]
		public override IPermission Copy () 
		{
			IsolatedStorageFilePermission p = new IsolatedStorageFilePermission (PermissionState.None);
			// TODO add stuff into p
			return p;
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}
	}
}