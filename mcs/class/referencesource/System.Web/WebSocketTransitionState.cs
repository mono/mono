//------------------------------------------------------------------------------
// <copyright file="WebSocketTransitionState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {

    // Represents the transition state of a WebSocket request.
    // Any state can be a terminal state, but if a state transition does take place it will go in the
    // order Inactive -> AcceptWebSocketRequestCalled -> TransitionStarted -> TransitionCompleted.

    internal enum WebSocketTransitionState : byte {

        // This is not a WebSocket request, or if it is HttpContext.AcceptWebSocketRequest() hasn't yet been called.
        Inactive = 0,

        // HttpContext.AcceptWebSocketRequest() has been called, but we haven't yet started the transition.
        // This means that the request handler or ASP.NET modules may still be running.
        AcceptWebSocketRequestCalled,

        // We have started the transition, e.g. we're in the process of tearing down request state and releasing
        // objects (like HttpApplication instances) back into their respective pools. The handshake with the client
        // will also be performed during this time. Asynchronous module-level events (like SendResponse) should
        // not be fired after this point.
        TransitionStarted,

        // We have completed the transition, e.g. the handshake is completed and we have an active connection
        // with the client. The callback the developer passed to HttpContext.AcceptWebSocketRequest() is executing.
        TransitionCompleted

    }
}
