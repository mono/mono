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
	
	[MonoTODO]
	public sealed class SecurityPermission :
		CodeAccessPermission, IUnrestrictedPermission
	{
		public SecurityPermission (PermissionState state) {
			this.flags = SecurityPermissionFlag.NoFlags;
		}

		public SecurityPermission (SecurityPermissionFlag flags) {
			this.flags = flags;
		}

		public SecurityPermissionFlag Flags {
			get { return flags; }
			set { flags = value; }
		}

		public bool IsUnrestricted () {
			return false;
		}

		public override IPermission Copy () {
			return null;
		}

		public override IPermission Intersect (IPermission target) {
			return null;
		}

		public override IPermission Union (IPermission target) {
			return null;
		}

		public override bool IsSubsetOf (IPermission target) {
			return false;
		}

		public override void FromXml (SecurityElement e) {
		}

		public override SecurityElement ToXml () {
			return null;
		}

		// private 
		
		private SecurityPermissionFlag flags;
	}
}
