//
// System.Security.Permissions.UrlIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute
	{
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
			return new UrlIdentityPermission (url);
		}
	}
}
