//
// System.Security.Permissions.UrlIdentityPermissionAttribute.cs
//
// Authors:
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
	public sealed class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string url;
		
		// Constructor
		public UrlIdentityPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string Url
		{
			get { return url; }
			set { url = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			if (this.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted for Identity Permissions");

			// Note: It is possible to create a permission with a 
			// null URL but not to create a UrlIdentityPermission (null)
			if (url == null)
				return new UrlIdentityPermission (PermissionState.None);
			else
				return new UrlIdentityPermission (url);
		}
	}
}
