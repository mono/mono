//
// System.Diagnostics.EventLogPermissionEntry.cs
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
	public class EventLogPermissionEntry
	{
		private EventLogPermissionAccess permissionAccess;
		private string machineName;

		public EventLogPermissionEntry (
			EventLogPermissionAccess permissionAccess,
			string machineName)
		{
			this.permissionAccess = permissionAccess;
			this.machineName = machineName;
		}

		public string MachineName {
			get {return machineName; }
		}

		public EventLogPermissionAccess PermissionAccess {
			get {return permissionAccess; }
		}

		internal ResourcePermissionBaseEntry CreateResourcePermissionBaseEntry ()
		{
			return new ResourcePermissionBaseEntry ((int) permissionAccess, new string[] {machineName});
		} 
	}
}

