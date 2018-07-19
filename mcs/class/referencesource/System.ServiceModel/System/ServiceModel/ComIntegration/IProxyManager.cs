//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    
    [System.Security.SuppressUnmanagedCodeSecurity,
     ComImport,
     Guid("C05307A7-70CE-4670-92C9-52A757744A02"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IProxyManager
    {
        void GetIDsOfNames([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr dispid);
        [PreserveSig]
        int Invoke(UInt32 dispIdMember, IntPtr outerProxy, IntPtr pVarResult, IntPtr pExcepInfo);
        [PreserveSig]
        int FindOrCreateProxy(IntPtr outerProxy, ref Guid riid, out IntPtr tearOff);
        void TearDownChannels();
        [PreserveSig]
        int InterfaceSupportsErrorInfo(ref Guid riid);
        [PreserveSig]
        int SupportsDispatch();
    }

}
