// <copyright file="SafeRegistryHandle.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Runtime.Caching {
    [SecurityCritical]
    internal class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid {
        // Note: Officially -1 is the recommended invalid handle value for
        // registry keys, but we'll also get back 0 as an invalid handle from
        // RegOpenKeyEx.
        
        [ResourceExposure(ResourceScope.Machine)]
        internal SafeRegistryHandle() : base(true) {}
        
        [SecurityCritical]
        protected override bool ReleaseHandle() {
            // Returns a Win32 error code, 0 for success
            int r = UnsafeNativeMethods.RegCloseKey(handle);
            return r == 0;
        }
    }
}
