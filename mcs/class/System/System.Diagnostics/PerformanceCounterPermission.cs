//
// System.Diagnostics.PerformanceCounterPermission.cs
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

namespace System.Diagnostics {

	[Serializable]
	public sealed class PerformanceCounterPermission : ResourcePermissionBase {

		public PerformanceCounterPermission ()
		{
		}

		public PerformanceCounterPermission (PerformanceCounterPermissionEntry[] permissionAccessEntries)
		{
			if (permissionAccessEntries == null)
				throw new ArgumentNullException("permissionAccessEntries");
			foreach (PerformanceCounterPermissionEntry entry in permissionAccessEntries)
				AddPermissionAccess (entry.CreateResourcePermissionBaseEntry ());
		}

		public PerformanceCounterPermission (PermissionState state)
			: base (state)
		{
		}

		public PerformanceCounterPermission (
			PerformanceCounterPermissionAccess permissionAccess, 
			string machineName, 
			string categoryName)
		{
			AddPermissionAccess (new PerformanceCounterPermissionEntry (permissionAccess, machineName, categoryName).CreateResourcePermissionBaseEntry ());
		}

		public PerformanceCounterPermissionEntryCollection PermissionEntries {
			get {return new PerformanceCounterPermissionEntryCollection (base.GetPermissionEntries()); }
		}
	}
}

