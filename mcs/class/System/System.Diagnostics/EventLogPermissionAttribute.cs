//
// System.Diagnostics.EventLogPermissionAttribute.cs
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
using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics 
{

	[AttributeUsage(
		AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Struct | AttributeTargets.Constructor |
		AttributeTargets.Method | AttributeTargets.Event)]
	[Serializable]
	public class EventLogPermissionAttribute : CodeAccessSecurityAttribute 
	{
		private string machineName;
		private EventLogPermissionAccess permissionAccess;

		public EventLogPermissionAttribute(SecurityAction action)
			: base(action)
		{
			machineName = ".";
			permissionAccess = EventLogPermissionAccess.Browse;
		}

		// May throw ArgumentException if computer name is invalid
		public string MachineName {
			get {return machineName;}
			set {
				// TODO check machine name
				machineName = value;
			}
		}

		public EventLogPermissionAccess PermissionAccess {
			get {return permissionAccess;}
			set {permissionAccess = value;}
		}

		public override IPermission CreatePermission()
		{
			if (base.Unrestricted) {
				return new EventLogPermission (PermissionState.Unrestricted); 
			}
			return new EventLogPermission (PermissionAccess, MachineName); 
		}
	}
}

