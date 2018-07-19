//------------------------------------------------------------------------------
// <copyright file="INotifySink2.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Interop {
    using System;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security;
      
    [ComImport(), Guid("C43CC2F3-90AF-4e93-9112-DFB8B36749B5"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    internal interface INotifySink2 {
        
        void OnSyncCallOut(
            [In] CallId callId,
            [Out] out IntPtr out_ppBuffer, // byte**
            [In, Out] ref int inout_pBufferSize); // DWORD*

        
        void OnSyncCallEnter(
            [In] CallId callId,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] in_pBuffer, // byte*
            [In] int in_BufferSize);

        
        void OnSyncCallReturn(
            [In] CallId callId,
            [In, MarshalAs(UnmanagedType.LPArray)] byte[] in_pBuffer, // byte*
            [In] int in_BufferSize);

        
        void OnSyncCallExit(
            [In] CallId callId,
            [Out] out IntPtr out_ppBuffer, // byte**
            [In, Out] ref int inout_pBufferSize);
    }   
}
