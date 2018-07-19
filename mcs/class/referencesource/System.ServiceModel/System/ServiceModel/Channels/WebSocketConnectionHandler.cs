// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;

    abstract class WebSocketConnectionHandler : HttpMessageHandler
    {
        protected internal virtual HttpResponseMessage AcceptWebSocket(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.AcceptWebSocket(request))
            {
                return GetWebSocketAcceptedResponseMessage(request);
            }
            else
            {
                return GetUpgradeRequiredResponseMessage(request);
            }
        }

        protected internal virtual bool AcceptWebSocket(HttpRequestMessage request)
        {
            return true;
        }

        protected static HttpResponseMessage GetUpgradeRequiredResponseMessage(HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.UpgradeRequired);
            response.RequestMessage = request;
            return response;
        }

        protected static HttpResponseMessage GetBadRequestResponseMessage(HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.RequestMessage = request;
            return response;
        }

        protected static HttpResponseMessage GetWebSocketAcceptedResponseMessage(HttpRequestMessage request)
        {
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
            message.RequestMessage = request;
            return message;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw FxTrace.Exception.ArgumentNull("request");
            }

            return Task.Factory.StartNew(
                                () => { return this.AcceptWebSocket(request, cancellationToken); }, 
                                cancellationToken);
        }
    }
}
