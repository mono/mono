//
// System.Security.Permissions.SiteIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
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
			if (this.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted");

			SiteIdentityPermission perm = null;
			if (site == null)
				perm = new SiteIdentityPermission (PermissionState.None);
			else
				perm = new SiteIdentityPermission (site);
			return perm;
		}
	}
}
