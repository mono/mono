//
// System.Security.Permissions.RegistryPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class RegistryPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private RegistryPermissionAccess _access;
		private string _pathList;

		// Constructors

		public RegistryPermission (PermissionState state)
		{
		}

		public RegistryPermission (RegistryPermissionAccess access, string pathList)
		{
		}

		// Properties

		// Methods

		[MonoTODO]
		public void AddPathList (RegistryPermissionAccess access, string pathList) 
		{
		}

		[MonoTODO]
		public string GetPathList (RegistryPermissionAccess access)
		{
			return null;
		}

		[MonoTODO]
		public void SetPathList (RegistryPermissionAccess access, string pathList)
		{
		}

		public override IPermission Copy () 
		{
			return new RegistryPermission (_access, _pathList);
		}

		[MonoTODO]
		public override void FromXml (SecurityElement esd) 
		{
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
		public bool IsUnrestricted () 
		{
			return false;
		}

		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			return null;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			return null;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 5;
		}
	}
}