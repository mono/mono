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
	[Serializable]
	public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		AspNetHostingPermissionLevel _level;

		public AspNetHostingPermission (AspNetHostingPermissionLevel level)
		{
			_level = level;
		}
		public AspNetHostingPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					_level = AspNetHostingPermissionLevel.None;
					break;
				case PermissionState.Unrestricted:
					_level = AspNetHostingPermissionLevel.Unrestricted;
					break;
			}
		}

		public AspNetHostingPermissionLevel Level {
			get { return _level; }
			set { _level = value; }
		}

		public bool IsUnrestricted ()
		{
			return (_level == AspNetHostingPermissionLevel.Unrestricted);
		}

		public override IPermission Copy ()
		{
			return new AspNetHostingPermission (_level);
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