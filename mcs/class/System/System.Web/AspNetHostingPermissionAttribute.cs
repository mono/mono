//
// System.Web.AspNetHostingPermissionAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

#if NET_1_1

using System.Security;
using System.Security.Permissions;

namespace System.Web
{
	public sealed class AspNetHostingPermissionAttribute : CodeAccessSecurityAttribute
	{
		AspNetHostingPermissionLevel level;

		public AspNetHostingPermissionAttribute (SecurityAction action)
			: base (action)
		{
			// LAMESPEC: seems to initialize to None
			level = AspNetHostingPermissionLevel.None;
		}

		[MonoTODO("implement")]
		public override IPermission CreatePermission ()
		{
			throw new NotImplementedException ();
		}

		public AspNetHostingPermissionLevel Level {
			get { return level; }
			set { level = value; }
		}
	}
}

#endif