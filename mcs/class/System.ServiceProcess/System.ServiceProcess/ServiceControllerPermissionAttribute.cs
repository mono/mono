//
// System.ServiceProcess.ServiceControllerPermissionAttribute.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

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

using System;
using System.Security;
using System.Security.Permissions;

namespace System.ServiceProcess {

        [Serializable]
        [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class |
                        AttributeTargets.Struct   | AttributeTargets.Constructor |
                        AttributeTargets.Method   | AttributeTargets.Event)]
        public class ServiceControllerPermissionAttribute : CodeAccessSecurityAttribute
        {
                string machine_name;
                string service_name;
                ServiceControllerPermissionAccess permission_access;
                
                public ServiceControllerPermissionAttribute (SecurityAction action)
                        : base (action)
                {
                        machine_name = ".";
                        service_name = "*";
                        permission_access = ServiceControllerPermissionAccess.Browse;
                }

                public string MachineName {

                        get { return machine_name; }
                                

                        set { machine_name = value; }
                }

                public ServiceControllerPermissionAccess PermissionAccess {

                        get { return permission_access; }

                        set { permission_access = value; }
                }

                public string ServiceName {

                        get { return service_name; }

                        set {
                                if (value == null)
                                        throw new ArgumentNullException (
                                                Locale.GetText ("Argument is null"));

                                service_name = value;
                        }
                }

                [MonoTODO]
                public override IPermission CreatePermission ()
                {
                        throw new NotImplementedException ();
                }
        }
}
