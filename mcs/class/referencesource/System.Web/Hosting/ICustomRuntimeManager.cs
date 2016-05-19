//------------------------------------------------------------------------------
// <copyright file="ICustomRuntimeManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("A0BBBDFF-5AF5-42E3-9753-34441F764A6B")]
    internal interface ICustomRuntimeManager {

        // Registers an ICustomRuntime so that we can keep track of them.
        [return: MarshalAs(UnmanagedType.Interface)]
        ICustomRuntimeRegistrationToken Register(
            [In, MarshalAs(UnmanagedType.Interface)] ICustomRuntime customRuntime);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3A8E9CED-D3C9-4C4B-8956-6F15E2F559D9")]
    internal interface ICustomRuntimeRegistrationToken {
        // Unregisters an ICustomRuntime.
        void Unregister();
    }
}
