//
// System.Diagnostics.EventLogPermissionAttribute.cs
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

	[AttributeUsage (
		AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Struct | AttributeTargets.Constructor |
		AttributeTargets.Method | AttributeTargets.Event,
		AllowMultiple=true, Inherited=false)]
	[Serializable]
	public class EventLogPermissionAttribute : CodeAccessSecurityAttribute {

		private string machineName;
		private EventLogPermissionAccess permissionAccess;

		public EventLogPermissionAttribute (SecurityAction action)
			: base (action)
		{
			machineName = ResourcePermissionBase.Local;
			permissionAccess = EventLogPermissionAccess.Write;
		}

		public string MachineName {
			get { return machineName; }
			set {
				ResourcePermissionBase.ValidateMachineName (value);
				machineName = value;
			}
		}

		public EventLogPermissionAccess PermissionAccess {
			get { return permissionAccess; }
			set { permissionAccess = value; }
		}

		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted) {
				return new EventLogPermission (PermissionState.Unrestricted); 
			}
			return new EventLogPermission (permissionAccess, machineName); 
		}
	}
}

