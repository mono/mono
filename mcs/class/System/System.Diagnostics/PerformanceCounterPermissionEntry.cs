//
// System.Diagnostics.PerformanceCounterPermissionEntry.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

using System;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.Diagnostics 
{
	[Serializable]
	public class PerformanceCounterPermissionEntry 
	{
		private PerformanceCounterPermissionAccess permissionAccess;
		private string machineName;
		private string categoryName;

		public PerformanceCounterPermissionEntry (
			PerformanceCounterPermissionAccess permissionAccess,
			string machineName,
			string categoryName)
		{
			this.permissionAccess = permissionAccess;
			this.machineName = machineName;
			this.categoryName = categoryName;
		}

		public string CategoryName {
			get {return categoryName; }
		}

		public string MachineName {
			get {return machineName; }
		}

		public PerformanceCounterPermissionAccess PermissionAccess {
			get {return permissionAccess; }
		}

		internal ResourcePermissionBaseEntry CreateResourcePermissionBaseEntry ()
		{
			return new ResourcePermissionBaseEntry ((int) permissionAccess, new string[] {machineName, categoryName});
		} 
	}
}

