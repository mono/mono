//------------------------------------------------------------------------------
// <copyright file="IAsyncAbortableWebSocket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Threading.Tasks;

    // Represents a WebSocket that can be asynchronously aborted.

    internal interface IAsyncAbortableWebSocket {

        // Asynchronously aborts a WebSocket.
        Task AbortAsync();

    }
}
