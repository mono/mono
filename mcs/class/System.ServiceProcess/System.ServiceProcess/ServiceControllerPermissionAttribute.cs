//
// System.ServiceProcess.ServiceControllerPermissionAttribute.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
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
