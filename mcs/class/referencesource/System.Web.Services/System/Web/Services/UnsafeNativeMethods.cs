//------------------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.Services.Interop;
    using System.Security;    

    [ComVisible(false), SuppressUnmanagedCodeSecurity,
    SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]

    internal class UnsafeNativeMethods {
        private UnsafeNativeMethods() { }

        [DllImport(ExternDll.Ole32, ExactSpelling = true)]
        internal static extern int CoCreateInstance([In] ref Guid clsid,
                                                    [MarshalAs(UnmanagedType.Interface)] object punkOuter,
                                                    int context,
                                                    [In] ref Guid iid,
                                                    [MarshalAs(UnmanagedType.Interface)] out object punk);

        internal static INotifySink2 RegisterNotifySource(INotifyConnection2 connection, INotifySource2 source) {
            return connection.RegisterNotifySource(source);
        }                                                    

        internal static void UnregisterNotifySource(INotifyConnection2 connection, INotifySource2 source) {
            connection.UnregisterNotifySource(source);
        }   
        
        internal static void OnSyncCallOut(INotifySink2 sink, CallId callId, out IntPtr out_ppBuffer, ref int inout_pBufferSize) {
            sink.OnSyncCallOut(callId, out out_ppBuffer, ref inout_pBufferSize);
        }

        internal static void OnSyncCallEnter(INotifySink2 sink, CallId callId, byte[] in_pBuffer, int in_BufferSize) {
            sink.OnSyncCallEnter(callId, in_pBuffer, in_BufferSize);
        }
        
        internal static void OnSyncCallReturn(INotifySink2 sink, CallId callId, byte[] in_pBuffer, int in_BufferSize) {
            sink.OnSyncCallReturn(callId, in_pBuffer, in_BufferSize);
        }

        internal static void OnSyncCallExit(INotifySink2 sink, CallId callId, out IntPtr out_ppBuffer, ref int inout_pBufferSize) {
            sink.OnSyncCallExit(callId, out out_ppBuffer, ref inout_pBufferSize);
        }
    }
}

