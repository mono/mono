//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.IdentityModel.Selectors
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
    using Microsoft.InfoCards.Diagnostics;
    using Microsoft.Win32;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    //
    // Summary:
    // A class to wrap a library handle for reliability.
    // When LoadLibrary returns, the runtime stores the resulting IntPtr 
    // into the already created SafeLibraryHandle. The runtime guarantees that 
    // this operation is atomic, meaning that if the P/Invoke method successfully returns, 
    // the IntPtr will be stored safely inside the SafeHandle. Once inside the SafeHandle, 
    // even if an asynchronous exception occurs and prevents LoadLibrary's SafeLibraryHandle return 
    // value from being stored, the relevant IntPtr is already stored within a managed object 
    // whose critical finalizer will ensure its proper release.
    //
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {

        private SafeLibraryHandle()
            : base(true)
        {

        }

        protected override bool ReleaseHandle()
        {
#pragma warning suppress 56523
            return FreeLibrary(handle);
        }


        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern SafeLibraryHandle LoadLibraryW(
                    [MarshalAs(UnmanagedType.LPWStr)]
                    string dllname);


        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        internal static extern bool FreeLibrary(IntPtr hModule);
    }
}
