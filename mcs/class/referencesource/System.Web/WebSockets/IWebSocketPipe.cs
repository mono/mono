//------------------------------------------------------------------------------
// <copyright file="IWebSocketPipe.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Net.WebSockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using System.Web.Util;

    // Provides an abstraction over the WebSocketPipe

    internal interface IWebSocketPipe {

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        void CloseTcpConnection();

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        Task<WebSocketReceiveResult> ReadFragmentAsync(ArraySegment<byte> buffer);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        Task WriteCloseFragmentAsync(WebSocketCloseStatus closeStatus, string statusDescription);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        Task WriteFragmentAsync(ArraySegment<byte> buffer, bool isUtf8Encoded, bool isFinalFragment);

    }
}
