//
// System.Security.Permissions.SecurityPermission.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {
	
	[Serializable]
	public sealed class SecurityPermission :
		CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		[MonoTODO]
		public SecurityPermission (PermissionState state) 
		{
			this.flags = SecurityPermissionFlag.NoFlags;
		}

		public SecurityPermission (SecurityPermissionFlag flags) 
		{
			this.flags = flags;
		}

		public SecurityPermissionFlag Flags {
			get { return flags; }
			set { flags = value; }
		}

		[MonoTODO]
		public bool IsUnrestricted () 
		{
			return false;
		}

		public override IPermission Copy () 
		{
			return new SecurityPermission (flags);
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		[MonoTODO]
		public override void FromXml (SecurityElement e) 
		{
		}

		[MonoTODO]
		public override SecurityElement ToXml () 
		{
			return null;
		}

		// private 
		
		private SecurityPermissionFlag flags;

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 6;
		}
	}
}
