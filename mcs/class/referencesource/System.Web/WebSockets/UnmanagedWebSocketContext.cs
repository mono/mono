//------------------------------------------------------------------------------
// <copyright file="UnmanagedWebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    // Concrete implementation of IUnmanagedWebSocketContext, implemented by IIS8 unmanaged WebSocket module
    // SECURITY NOTE: All parameters are assumed to have been validated by the caller

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class UnmanagedWebSocketContext : IUnmanagedWebSocketContext {

        // the 'this' parameter for the unmanaged interface
        private readonly IntPtr _pWebSocketContext;

        internal UnmanagedWebSocketContext(IntPtr pWebSocketContext) {
            _pWebSocketContext = pWebSocketContext;
        }

        public int WriteFragment(IntPtr pData, ref int pcbSent, bool fAsync, bool fUtf8Encoded, bool fFinalFragment, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected) {
            return IIS.MgdWebSocketWriteFragment(_pWebSocketContext, pData, ref pcbSent, fAsync, fUtf8Encoded, fFinalFragment, pfnCompletion, pvCompletionContext, out pfCompletionExpected);
        }

        public int ReadFragment(IntPtr pData, ref int pcbData, bool fAsync, out bool pfUtf8Encoded, out bool pfFinalFragment, out bool pfConnectionClose, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected) {
            return IIS.MgdWebSocketReadFragment(_pWebSocketContext, pData, ref pcbData, fAsync, out pfUtf8Encoded, out pfFinalFragment, out pfConnectionClose, pfnCompletion, pvCompletionContext, out pfCompletionExpected);
        }

        public int SendConnectionClose(bool fAsync, ushort uStatusCode, string szReason, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected) {
            return IIS.MgdWebSocketSendConnectionClose(_pWebSocketContext, fAsync, uStatusCode, szReason, pfnCompletion, pvCompletionContext, out pfCompletionExpected);
        }

        public int GetCloseStatus(out ushort pStatusCode, out IntPtr ppszReason, out ushort pcchReason) {
            return IIS.MgdWebSocketGetCloseStatus(_pWebSocketContext, out pStatusCode, out ppszReason, out pcchReason);
        }

        public void CloseTcpConnection() {
            IIS.MgdWebSocketCloseTcpConnection(_pWebSocketContext);
        }

        // API documentation for each method can be found in ndp/fx/src/xsp/webengine/mgdexports.cxx.
        [SuppressUnmanagedCodeSecurity]
        private static class IIS {
            private const string _IIS_NATIVE_DLL = ModName.MGDENG_FULL_NAME;

            // Write a data fragment to the provided IWebSocketContext.
            [DllImport(_IIS_NATIVE_DLL)]
            internal static extern int MgdWebSocketWriteFragment(
                IntPtr pContext,
                IntPtr pData,
                ref int pcbSent,
                bool fAsync,
                bool fUTF8Encoded,
                bool fFinalFragment,
                IntPtr pfnCompletion,
                IntPtr pvCompletionContext,
                out bool pfCompletionExpected);

            // Reads a data fragment from the provided IWebSocketContext.
            [DllImport(_IIS_NATIVE_DLL)]
            internal static extern int MgdWebSocketReadFragment(
                IntPtr pContext,
                IntPtr pData,
                ref int pcbData,
                bool fAsync,
                out bool pfUTF8Encoded,
                out bool pfFinalFragment,
                out bool pfConnectionClose,
                IntPtr pfnCompletion,
                IntPtr pvCompletionContext,
                out bool pfCompletionExpected);

            // Sends a CLOSE frame to the provided IWebSocketContext.
            [DllImport(_IIS_NATIVE_DLL)]
            internal static extern int MgdWebSocketSendConnectionClose(
                IntPtr pContext,
                bool fAsync,
                ushort pStatusCode,
                [MarshalAs(UnmanagedType.LPWStr)] string pszReason,
                IntPtr pfnCompletion,
                IntPtr pvCompletionContext,
                out bool pfCompletionExpected);

            // Gets information on the CLOSE frame sent from the client to the server.
            [DllImport(_IIS_NATIVE_DLL)]
            internal static extern int MgdWebSocketGetCloseStatus(
                IntPtr pContext,
                out ushort pStatusCode,
                out IntPtr ppszReason,
                out ushort pcchReason);

            // Closes the TCP connection used by the provided IWebSocketContext.
            [DllImport(_IIS_NATIVE_DLL)]
            internal static extern void MgdWebSocketCloseTcpConnection(
                IntPtr pContext);
        }
    }
}
