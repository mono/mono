// <copyright file="SRef.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.Caching {
    internal class SRef {
        private static Type s_type = Type.GetType("System.SizedReference", true, false);
        private Object _sizedRef;

        internal SRef(Object target) {
            _sizedRef = Activator.CreateInstance(s_type,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    new object[] { target },
                    null);
        }

        internal long ApproximateSize {
            [SecuritySafeCritical]
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            get {
                object o = s_type.InvokeMember("ApproximateSize",
                                               BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                                               null, // binder
                                               _sizedRef, // target
                                               null, // args
                                               CultureInfo.InvariantCulture);
                return (long)o;
            }
        }

        // 
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Grandfathered suppression from original caching code checkin")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void Dispose() {
            s_type.InvokeMember("Dispose",
                                BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                null, // binder
                                _sizedRef, // target
                                null, // args
                                CultureInfo.InvariantCulture);
        }
    }
}
