//
// System.Security.Permissions.ResourcePermissionBaseEntry.cs
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[Serializable]
	public class ResourcePermissionBaseEntry { 

		private int permissionAccess;
		private string[] permissionAccessPath;

		public ResourcePermissionBaseEntry ()
			: this (0, new string[0])
		{
		}

		public ResourcePermissionBaseEntry (int permissionAccess,
			string[] permissionAccessPath)
		{
			if (permissionAccessPath == null)
				throw new ArgumentNullException ("permissionAccessPath");

			this.permissionAccess = permissionAccess;
			this.permissionAccessPath = permissionAccessPath;
		}

		public int PermissionAccess {
			get {return permissionAccess;}
		}

		public string[] PermissionAccessPath {
			get {return permissionAccessPath;}
		}
	}
}

