//------------------------------------------------------------------------------
// <copyright file="IntSecurity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    internal static class IntSecurity {
#if !DISABLE_CAS_USE
        public static readonly CodeAccessPermission UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
        public static readonly CodeAccessPermission FullReflection = new ReflectionPermission(PermissionState.Unrestricted);
#endif

        public static string UnsafeGetFullPath(string fileName) {
#if DISABLE_CAS_USE
			return System.IO.Path.GetFullPath(fileName);
#else
            string full = fileName;

            FileIOPermission fiop = new FileIOPermission(PermissionState.None);
            fiop.AllFiles = FileIOPermissionAccess.PathDiscovery;
            fiop.Assert();
            try {
                full = System.IO.Path.GetFullPath(fileName);
            }
            finally {
                CodeAccessPermission.RevertAssert();
            }
            return full;
#endif
        }
    }
}
