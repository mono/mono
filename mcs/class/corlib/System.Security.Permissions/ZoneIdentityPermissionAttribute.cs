//
// System.Security.Permissions.ZoneIdentityPermissionAttribute.cs
//
// Author:
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class ZoneIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private SecurityZone zone;
		
		// Constructor
		public ZoneIdentityPermissionAttribute (SecurityAction action) : base (action) 
		{
			zone = SecurityZone.NoZone;
		}
		
		// Properties
		public SecurityZone Zone
		{
			get { return zone; }
			set { zone = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			if (this.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted");

			return new ZoneIdentityPermission (zone);
		}
	}
}
