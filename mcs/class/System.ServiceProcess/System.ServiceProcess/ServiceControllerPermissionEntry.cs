//
// System.ServiceProcess.ServiceControllerPermissionEntry.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.ComponentModel;

namespace System.ServiceProcess {

        [Serializable]
        public class ServiceControllerPermissionEntry
        {
                string machine_name;
                string service_name;
                ServiceControllerPermissionAccess permission_access;
                
                public ServiceControllerPermissionEntry ()
                {
                        machine_name = ".";
                        service_name = "*";
                        permission_access = ServiceControllerPermissionAccess.Browse;
                }

                public ServiceControllerPermissionEntry (
                        ServiceControllerPermissionAccess permissionAccess,
                        string machineName,
                        string serviceName)
                {
                        permission_access = permissionAccess;
                        machine_name = machineName;
                        service_name = serviceName;
                }

                public string MachineName {

                        get { return machine_name; }

                }

                public string ServiceName {

                        get { return service_name; }

                }

                public ServiceControllerPermissionAccess PermissionAccess {

                        get { return permission_access; }

                }
        }
}
