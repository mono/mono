//
// System.Security.Permissions.PrincipalPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
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
			authenticated = true; // strange but true ;)
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
			PrincipalPermission perm = null;
			if (this.Unrestricted)
				perm = new PrincipalPermission (PermissionState.Unrestricted);
			else
				perm = new PrincipalPermission (name, role, authenticated);
			return perm;
		}
	}
}
