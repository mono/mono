//------------------------------------------------------------------------------
// <copyright file="WebAdminConfigurationHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/************************************************************************************************************/

namespace System.Web.Administration {

    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.Util;
    using System.Web.UI;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class WebAdminConfigurationHelper : MarshalByRefObject, IRegisteredObject {

        public WebAdminConfigurationHelper() {
            HostingEnvironment.RegisterObject(this);
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        public VirtualDirectory GetVirtualDirectory(string path) {

            if (HttpRuntime.NamedPermissionSet != null) {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }

            return HostingEnvironment.VirtualPathProvider.GetDirectory(path);
        }

        public object CallMembershipProviderMethod (string methodName, object[] parameters, Type[] paramTypes) {
            Type tempType = typeof(HttpContext).Assembly.GetType("System.Web.Security.Membership");

            object returnObject = null;
            BindingFlags allBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo method = null;
            if (paramTypes != null) {
                method = tempType.GetMethod(methodName, allBindingFlags, null, paramTypes, null);
            } else {
                method = tempType.GetMethod(methodName, allBindingFlags);
            }

            if (method != null) {
                if (HttpRuntime.NamedPermissionSet != null) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }

                returnObject = method.Invoke(null, parameters);
            }

            object[] newValues = new object[parameters.Length + 1];
            newValues[0] = returnObject;
            int j = 1;
            for (int i = 0; i < (parameters.Length); i++) {
                newValues[j++] = parameters[i];
            }

            returnObject = (object) newValues;
            return returnObject;
        }

        public object GetMembershipProviderProperty(string propertyName) {
            Type tempType = typeof(HttpContext).Assembly.GetType("System.Web.Security.Membership");

            object returnObject = null;

            BindingFlags allBindingFlags = BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (HttpRuntime.NamedPermissionSet != null) {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }

            returnObject = tempType.InvokeMember(propertyName, allBindingFlags, null, null, null, System.Globalization.CultureInfo.InvariantCulture);
            return returnObject;
        }

        public object CallRoleProviderMethod (string methodName, object[] parameters, Type[] paramTypes) {

            Type tempType = typeof(HttpContext).Assembly.GetType("System.Web.Security.Roles");

            object returnObject = null;
            BindingFlags allBindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MethodInfo method = null;
            if (paramTypes != null) {
                method = tempType.GetMethod(methodName, allBindingFlags, null, paramTypes, null);
            } else {
                method = tempType.GetMethod(methodName, allBindingFlags);
            }

            if (method != null) {
                if (HttpRuntime.NamedPermissionSet != null) {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }

                returnObject = method.Invoke(null, parameters);
            }

            object[] newValues = new object[parameters.Length + 1];
            newValues[0] = returnObject;
            int j = 1;
            for (int i = 0; i < (parameters.Length); i++) {
                newValues[j++] = parameters[i];
            }

            returnObject = (object) newValues;
            return returnObject;
        }


        void IRegisteredObject.Stop(bool immediate) {
            HostingEnvironment.UnregisterObject(this);
        }
    }
}
