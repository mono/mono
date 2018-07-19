//------------------------------------------------------------------------------
// <copyright file="SystemWebProxy.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Web.Security;
    using System.Security.Permissions;    

    // Because this assembly ships in the client SKU, whereas System.Web.dll ships in the Extended SKU, we need to
    // proxy any usage of System.Web.dll. This allows us to fail gracefully if the Extended SKU is not present. 
    // Users can avoid this failure by overridding all virtual members on both of these types which will prevent 
    // this class, and hence, System.Web.dll, from ever being called.
    internal static class SystemWebProxy {

        public static readonly IMembershipAdapter Membership = GetMembershipAdapter();

        private static IMembershipAdapter GetMembershipAdapter() {
            IMembershipAdapter membership = CreateSystemWebMembershipAdapter();
            if (membership == null) {
                membership = new DefaultMembershipAdapter();
            }

            return membership;
        }

        private static IMembershipAdapter CreateSystemWebMembershipAdapter() {
            Type type = Type.GetType("System.Web.Security.MembershipAdapter, " + AssemblyRef.SystemWeb, throwOnError:false);
            if (type != null) {
                // Running on Extended SKU

                return (IMembershipAdapter)DangerousCreateInstance(type);
            }

            // Running on Client SKU
            return null;            
        }

        // Partially trusted callers might not have permissions to create an instance of 
        // System.Web’s MembershipAdapter type (it’s an internal type), so give them access.
        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private static object DangerousCreateInstance(Type type) {

            return Activator.CreateInstance(type);
        }
    }
}
