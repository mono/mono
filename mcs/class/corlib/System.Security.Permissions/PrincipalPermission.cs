//
// System.Security.Permissions.PrincipalPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class PrincipalPermission : IPermission, IUnrestrictedPermission {

		private string _name;
		private string _role;
		private bool _isAuthenticated;

		// Constructors

		public PrincipalPermission (PermissionState state)
		{
		}

		public PrincipalPermission (string name, string role)
		{
		}

		public PrincipalPermission (string name, string role, bool isAuthenticated)
		{
		}

		// Properties

		// Methods

		public IPermission Copy () 
		{
			return new PrincipalPermission (_name, _role, _isAuthenticated);
		}

		[MonoTODO]
		public void Demand () 
		{
		}

		[MonoTODO]
		public void FromXml (SecurityElement esd) 
		{
		}

		[MonoTODO]
		public IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		[MonoTODO]
		public bool IsUnrestricted () 
		{
			return false;
		}

		[MonoTODO]
		public override string ToString () 
		{
			return null;
		}

		[MonoTODO]
		public SecurityElement ToXml () 
		{
			return null;
		}

		[MonoTODO]
		public IPermission Union (IPermission target)
		{
			return null;
		}
	}
}