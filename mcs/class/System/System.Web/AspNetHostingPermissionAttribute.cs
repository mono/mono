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
	[Serializable]
	[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class AspNetHostingPermissionAttribute : CodeAccessSecurityAttribute
	{
		AspNetHostingPermissionLevel _level;

		public AspNetHostingPermissionAttribute (SecurityAction action)
			: base (action)
		{
			// LAMESPEC: seems to initialize to None
			_level = AspNetHostingPermissionLevel.None;
		}

		[MonoTODO("implement")]
		public override IPermission CreatePermission ()
		{
			throw new NotImplementedException ();
		}

		public AspNetHostingPermissionLevel Level {
			get { return _level; }
			set { _level = value; }
		}
	}
}

#endif