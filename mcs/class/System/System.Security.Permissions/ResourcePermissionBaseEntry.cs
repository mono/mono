//
// System.Security.Permissions.ResourcePermissionBaseEntry.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[Serializable]
	public class ResourcePermissionBaseEntry { 

		private int permissionAccess;
		private string[] permissionAccessPath;

		public ResourcePermissionBaseEntry ()
		{
		}

		public ResourcePermissionBaseEntry (int permissionAccess,
			string[] permissionAccessPath)
		{
			if (permissionAccessPath == null)
				throw new ArgumentNullException (
					"permissionAccessPath");
		}

		public int PermissionAccess {
			get {return permissionAccess;}
		}

		public string[] PermissionAccessPath {
			get {return permissionAccessPath;}
		}
	}
}

