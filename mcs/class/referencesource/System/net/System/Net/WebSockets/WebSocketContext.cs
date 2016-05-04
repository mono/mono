//------------------------------------------------------------------------------
// <copyright file="WebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Security.Principal;

    public abstract class WebSocketContext
    {
        public abstract Uri RequestUri { get; }
        public abstract NameValueCollection Headers { get; }
        public abstract string Origin { get; }
        public abstract IEnumerable<string> SecWebSocketProtocols { get; }
        public abstract string SecWebSocketVersion { get; }
        public abstract string SecWebSocketKey { get; }
        public abstract CookieCollection CookieCollection { get; }
        public abstract IPrincipal User { get; }
        public abstract bool IsAuthenticated { get; }
        public abstract bool IsLocal { get; }
        public abstract bool IsSecureConnection { get; }
        public abstract WebSocket WebSocket { get; }
    }
}
