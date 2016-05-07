//------------------------------------------------------------------------------
// <copyright file="HeapAllocHandle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Win32.SafeHandles;

    internal class HeapAllocHandle : SafeHandleZeroOrMinusOneIsInvalid {
        [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"This pointer is valid for the lifetime of the process; never needs to be released.")]
        private static readonly IntPtr ProcessHeap = UnsafeNativeMethods.GetProcessHeap();

        // Called by P/Invoke when returning SafeHandles
        protected HeapAllocHandle()
            : base(ownsHandle: true) {
        }

        // Do not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle for you.
        protected override bool ReleaseHandle() {
            return UnsafeNativeMethods.HeapFree(ProcessHeap, 0, handle);
        }
    }
}
