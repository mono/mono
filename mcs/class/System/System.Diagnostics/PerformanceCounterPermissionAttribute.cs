//
// System.Diagnostics.PerformanceCounterPermissionAttribute.cs
//
// Authors:
//	Jonathan Pryor (jonpryor@vt.edu)
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002
// (C) 2003 Andreas Nahr
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics {

	[AttributeUsage (AttributeTargets.Assembly |
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Constructor |
		AttributeTargets.Method |
		AttributeTargets.Event, AllowMultiple=true,
		Inherited=false)]
	[Serializable]
	public class PerformanceCounterPermissionAttribute : CodeAccessSecurityAttribute {

		private string categoryName;
		private string machineName;
		private PerformanceCounterPermissionAccess permissionAccess;

		public PerformanceCounterPermissionAttribute (SecurityAction action) 
			: base (action)
		{
			categoryName = ResourcePermissionBase.Any;
			machineName = ResourcePermissionBase.Local;
			permissionAccess = PerformanceCounterPermissionAccess.Write;
		}

		public string CategoryName {
			get { return categoryName; }
			set {
				if (value == null)
					throw new ArgumentNullException ("CategoryName");
				categoryName = value;
			}
		}

		public string MachineName {
			get { return machineName; }
			set {
				ResourcePermissionBase.ValidateMachineName (value);
				machineName = value;
			}
		}

		public PerformanceCounterPermissionAccess PermissionAccess {
			get { return permissionAccess; }
			set { permissionAccess = value; }
		}

		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted) {
				return new PerformanceCounterPermission (PermissionState.Unrestricted); 
			}
			return new PerformanceCounterPermission (permissionAccess, machineName, categoryName); 
		}
	}
}

