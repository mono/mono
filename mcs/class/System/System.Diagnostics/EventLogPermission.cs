//
// System.Diagnostics.EventLogPermission.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Andreas Nahr
//

using System;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.Diagnostics {

	[Serializable]
	public sealed class EventLogPermission : ResourcePermissionBase
	{

		public EventLogPermission()
		{
		}

		public EventLogPermission (EventLogPermissionEntry[] permissionAccessEntries)
		{
			if (permissionAccessEntries == null)
				throw new ArgumentNullException("permissionAccessEntries");
			foreach (EventLogPermissionEntry entry in permissionAccessEntries)
				AddPermissionAccess (entry.CreateResourcePermissionBaseEntry ());
		}

		public EventLogPermission (PermissionState state)
			: base (state)
		{
		}

		public EventLogPermission (EventLogPermissionAccess permissionAccess, string machineName)
		{
			AddPermissionAccess (new EventLogPermissionEntry (permissionAccess, machineName).CreateResourcePermissionBaseEntry ());
		}

		public EventLogPermissionEntryCollection PermissionEntries {
			get {return new EventLogPermissionEntryCollection (base.GetPermissionEntries()); }
		}
	}
}

