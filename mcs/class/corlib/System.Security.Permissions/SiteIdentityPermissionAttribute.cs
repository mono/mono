//
// System.Security.Permissions.SiteIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class SiteIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string site;
		
		// Constructor
		public SiteIdentityPermissionAttribute (SecurityAction action)
			: base (action) {}
		
		// Properties
		public string Site
		{
			get { return site; }
			set { site = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			return new SiteIdentityPermission (site);
		}
	}
}
