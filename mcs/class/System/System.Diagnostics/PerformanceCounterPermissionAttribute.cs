//
// System.Diagnostics.PerformanceCounterPermissionAttribute.cs
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
		AttributeTargets.Assembly |
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Constructor |
		AttributeTargets.Method |
		AttributeTargets.Event )]
	[Serializable]
	public class PerformanceCounterPermissionAttribute : CodeAccessSecurityAttribute 
	{
		private string categoryName;
		private string machineName;
		private PerformanceCounterPermissionAccess permissionAccess;

		public PerformanceCounterPermissionAttribute (SecurityAction action) 
			: base (action)
		{
			categoryName = "*";
			machineName = ".";
			permissionAccess = PerformanceCounterPermissionAccess.Browse;
		}

		public string CategoryName {
			get {return categoryName;}
			set {categoryName = value;}
		}

		// May throw ArgumentException if computer name is invalid
		public string MachineName {
			get {return machineName;}
			set {
				// TODO check machine name
				machineName = value;
			}
		}

		public PerformanceCounterPermissionAccess PermissionAccess {
			get {return permissionAccess;}
			set {permissionAccess = value;}
		}

		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted) {
				return new PerformanceCounterPermission (PermissionState.Unrestricted); 
			}
			return new PerformanceCounterPermission (PermissionAccess, MachineName, categoryName); 
		}
	}
}

