//
// System.Security.Permissions.PrincipalPermissionAttribute.cs
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
	public sealed class PrincipalPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private bool authenticated;
		private string name;
		private string role;
		
		// Constructor
		public PrincipalPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		// Properties
		public bool Authenticated
		{
			get { return authenticated; }
			set { authenticated = value; }
		}
			 			 
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
			 
		public string Role
		{
			get { return role; }
			set { role = value; }
		}

		// Method
		public override IPermission CreatePermission ()
		{
			return new PrincipalPermission (name, role, authenticated);
		}
	}
}
