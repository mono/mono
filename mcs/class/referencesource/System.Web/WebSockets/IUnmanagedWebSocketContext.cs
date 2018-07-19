//------------------------------------------------------------------------------
// <copyright file="IUnmanagedWebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Security.Permissions;

    // This interface matches the unmanaged IWebSocketContext interface

    internal interface IUnmanagedWebSocketContext {

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        int WriteFragment(IntPtr pData, ref int pcbSent, bool fAsync, bool fUtf8Encoded, bool fFinalFragment, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        int ReadFragment(IntPtr pData, ref int pcbData, bool fAsync, out bool pfUtf8Encoded, out bool pfFinalFragment, out bool pfConnectionClose, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        int SendConnectionClose(bool fAsync, ushort uStatusCode, string szReason, IntPtr pfnCompletion, IntPtr pvCompletionContext, out bool pfCompletionExpected);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        int GetCloseStatus(out ushort pStatusCode, out IntPtr ppszReason, out ushort pcchReason);

        // can be used for both normal + exception operation
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        void CloseTcpConnection();

    }
}
