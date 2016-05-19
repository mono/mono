//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;

    [System.Security.SuppressUnmanagedCodeSecurity,
     ComImport,
     Guid("11281BB7-1253-45ef-B98F-D551F79499FD"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IProxyProvider
    {
        [PreserveSig]
        int CreateOuterProxyInstance(IProxyManager proxyManager, [In()] ref Guid riid, out IntPtr ppv);
        [PreserveSig]
        int CreateDispatchProxyInstance(IntPtr outer, IPseudoDispatch proxy, out IntPtr ppvInner);
    }

}
