//------------------------------------------------------------------------------
// <copyright file="GCUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    // Provides helper methods for wrapping managed objects so that they can be passed to and from unmanaged code.
    // Unmanaged code can't actually inspect these objects at all since they're just GCHandles. And these handles
    // cannot be reused (they're single-shot).

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal static class GCUtil {

        public static IntPtr RootObject(object obj) {
            return (obj != null)
                ? (IntPtr)GCHandle.Alloc(obj)
                : IntPtr.Zero;
        }

        public static object UnrootObject(IntPtr pointer) {
            if (pointer != IntPtr.Zero) {
                GCHandle gcHandle = (GCHandle)pointer;
                if (gcHandle.IsAllocated) {
                    object target = gcHandle.Target;
                    gcHandle.Free();
                    return target;
                }
            }

            return null;
        }

    }
}
