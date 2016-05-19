// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Security.Principal;
    using System.Text;

    class ServiceWebSocketContext : WebSocketContext
    {
        WebSocketContext context;
        IPrincipal user;

        public ServiceWebSocketContext(WebSocketContext context, IPrincipal user)
        {
            Fx.Assert(context != null, "context should not be null.");
            this.context = context;
            this.user = user;
        }

        public override CookieCollection CookieCollection
        {
            get { return this.context.CookieCollection; }
        }

        public override NameValueCollection Headers
        {
            get { return this.context.Headers; }
        }

        public override bool IsAuthenticated
        {
            get { return this.user != null ? this.user.Identity != null && this.user.Identity.IsAuthenticated : this.context.IsAuthenticated; }
        }

        public override bool IsLocal
        {
            get { return this.context.IsLocal; }
        }

        public override bool IsSecureConnection
        {
            get { return this.context.IsSecureConnection; }
        }

        public override Uri RequestUri
        {
            get { return this.context.RequestUri; }
        }

        public override string SecWebSocketKey
        {
            get { return this.context.SecWebSocketKey; }
        }

        public override string Origin
        {
            get { return this.context.Origin; }
        }

        public override IEnumerable<string> SecWebSocketProtocols
        {
            get { return this.context.SecWebSocketProtocols; }
        }

        public override string SecWebSocketVersion
        {
            get { return this.context.SecWebSocketVersion; }
        }

        public override IPrincipal User
        {
            get { return this.user != null ? this.user : this.context.User; }
        }

        public override WebSocket WebSocket
        {
            get { throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.WebSocketContextWebSocketCannotBeAccessedError))); }
        }
    }
}
