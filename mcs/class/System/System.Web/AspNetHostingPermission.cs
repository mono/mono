//
// System.Web.AspNetHostingPermission.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

#if NET_1_1

using System.Security;
using System.Security.Permissions;

namespace System.Web
{
	public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		AspNetHostingPermissionLevel level;

		public AspNetHostingPermission (AspNetHostingPermissionLevel level)
		{
			this.level = level;
		}
		public AspNetHostingPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					level = AspNetHostingPermissionLevel.None;
					break;
				case PermissionState.Unrestricted:
					level = AspNetHostingPermissionLevel.Unrestricted;
					break;
			}
		}

		public AspNetHostingPermissionLevel Level {
			get { return level; }
			set { level = value; }
		}

		public bool IsUnrestricted ()
		{
			return (level == AspNetHostingPermissionLevel.Unrestricted);
		}

		public override IPermission Copy ()
		{
			return new AspNetHostingPermission (level);
		}

		[MonoTODO ("implement")]
		public override void FromXml (SecurityElement securityElement)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override SecurityElement ToXml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("implement")]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif