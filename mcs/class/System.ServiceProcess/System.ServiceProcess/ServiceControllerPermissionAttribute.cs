//
// System.ServiceProcess.ServiceControllerPermissionAttribute.cs
//
// Authors:
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003, Ximian Inc.
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

namespace System.ServiceProcess {

	[Serializable]
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
			AttributeTargets.Struct   | AttributeTargets.Constructor |
			AttributeTargets.Method   | AttributeTargets.Event,
			AllowMultiple=true, Inherited=false)]
	public class ServiceControllerPermissionAttribute : CodeAccessSecurityAttribute {

		string machine_name;
		string service_name;
		ServiceControllerPermissionAccess permission_access;
		
		public ServiceControllerPermissionAttribute (SecurityAction action)
			: base (action)
		{
			machine_name = ResourcePermissionBase.Local;
			service_name = ResourcePermissionBase.Any;
			permission_access = ServiceControllerPermissionAccess.Browse;
		}

		public string MachineName {
			get { return machine_name; }
			set { 
				ServiceControllerPermission.ValidateMachineName (value);
				machine_name = value;
			}
		}

		public ServiceControllerPermissionAccess PermissionAccess {
			get { return permission_access; }
			set {
				permission_access = value;
			}
		}

		public string ServiceName {
			get { return service_name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("ServiceName");
				ServiceControllerPermission.ValidateServiceName (value);
				service_name = value;
			}
		}

		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted)
				return new ServiceControllerPermission (PermissionState.Unrestricted);
			else
				return new ServiceControllerPermission (PermissionState.None);
		}
	}
}
