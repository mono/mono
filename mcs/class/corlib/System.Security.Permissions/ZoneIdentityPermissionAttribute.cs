//
// System.Security.Permissions.ZoneIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class ZoneIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private SecurityZone zone;
		
		// Constructor
		public ZoneIdentityPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public SecurityZone Zone
		{
			get { return zone; }
			set { zone = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			return new ZoneIdentityPermission (zone);
		}
	}
}
