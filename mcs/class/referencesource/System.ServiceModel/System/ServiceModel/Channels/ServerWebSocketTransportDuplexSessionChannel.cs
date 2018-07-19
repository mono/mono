// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;

    class ServerWebSocketTransportDuplexSessionChannel : WebSocketTransportDuplexSessionChannel
    {
        WebSocketContext webSocketContext;
        HttpRequestContext httpRequestContext;
        HttpPipeline httpPipeline;
        HttpResponseMessage httpResponseMessage;
        string subProtocol;
        WebSocketMessageSource webSocketMessageSource;
        SessionOpenNotification sessionOpenNotification;

        public ServerWebSocketTransportDuplexSessionChannel(
                        HttpChannelListener channelListener, 
                        EndpointAddress localAddress, 
                        Uri localVia, 
                        ConnectionBufferPool bufferPool, 
                        HttpRequestContext httpRequestContext, 
                        HttpPipeline httpPipeline, 
                        HttpResponseMessage httpResponseMessage, 
                        string subProtocol)
            : base(channelListener, localAddress, localVia, bufferPool)
        {
            this.httpRequestContext = httpRequestContext;
            this.httpPipeline = httpPipeline;
            this.httpResponseMessage = httpResponseMessage;
            this.subProtocol = subProtocol;
        }

        protected override bool IsStreamedOutput
        {
            get { return TransferModeHelper.IsResponseStreamed(this.TransferMode); }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(SessionOpenNotification))
            {
                if (this.sessionOpenNotification == null)
                {
                    this.sessionOpenNotification = new SessionOpenNotificationHelper(this);
                }

                return (T)(object)this.sessionOpenNotification;
            }

            return base.GetProperty<T>();
        }

        internal void SetWebSocketInfo(WebSocketContext webSocketContext, RemoteEndpointMessageProperty remoteEndpointMessageProperty, SecurityMessageProperty handshakeSecurityMessageProperty, byte[] innerBuffer, bool shouldDisposeWebSocketAfterClosed, HttpRequestMessage requestMessage)
        {
            Fx.Assert(webSocketContext != null, "webSocketContext should not be null.");
            this.ShouldDisposeWebSocketAfterClosed = shouldDisposeWebSocketAfterClosed;
            this.webSocketContext = webSocketContext;
            this.WebSocket = webSocketContext.WebSocket;
            this.InternalBuffer = innerBuffer;

            if (handshakeSecurityMessageProperty != null)
            {
                this.RemoteSecurity = handshakeSecurityMessageProperty;
            }

            bool inputUseStreaming = TransferModeHelper.IsRequestStreamed(this.TransferMode);
            this.webSocketMessageSource = new WebSocketMessageSource(
                            this,
                            this.webSocketContext,
                            inputUseStreaming,
                            remoteEndpointMessageProperty,
                            this,
                            requestMessage);

            this.SetMessageSource(this.webSocketMessageSource);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            ((IDisposable)this.httpRequestContext).Dispose();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (TD.WebSocketConnectionAcceptStartIsEnabled())
            {
                TD.WebSocketConnectionAcceptStart(this.httpRequestContext.EventTraceActivity);
            }

            this.httpRequestContext.AcceptWebSocket(this.httpResponseMessage, this.subProtocol, timeout);

            if (TD.WebSocketConnectionAcceptedIsEnabled())
            {
                TD.WebSocketConnectionAccepted(
                    this.httpRequestContext.EventTraceActivity,
                    this.WebSocket != null ? this.WebSocket.GetHashCode() : -1);
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (TD.WebSocketConnectionAcceptStartIsEnabled())
            {
                TD.WebSocketConnectionAcceptStart(this.httpRequestContext.EventTraceActivity);
            }

            return this.httpRequestContext.BeginAcceptWebSocket(this.httpResponseMessage, this.subProtocol, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.httpRequestContext.EndAcceptWebSocket(result);

            if (TD.WebSocketConnectionAcceptedIsEnabled())
            {
                TD.WebSocketConnectionAccepted(
                    this.httpRequestContext.EventTraceActivity,
                    this.WebSocket != null ? this.WebSocket.GetHashCode() : -1);
            }
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            // We don't need the HttpPipeline any more once the HTTP handshake is finished. 
            // Close it to release the CancellationTokenSource and other possible resources.
            this.httpPipeline.Close();
        }

        class SessionOpenNotificationHelper : SessionOpenNotification
        {
            readonly ServerWebSocketTransportDuplexSessionChannel channel;

            public SessionOpenNotificationHelper(ServerWebSocketTransportDuplexSessionChannel channel)
            {
                this.channel = channel;
            }

            public override bool IsEnabled
            {
                get
                {
                    return this.channel.WebSocketSettings.CreateNotificationOnConnection;
                }
            }

            public override void UpdateMessageProperties(MessageProperties inboundMessageProperties)
            {
                this.channel.webSocketMessageSource.UpdateOpenNotificationMessageProperties(inboundMessageProperties);
            }
        }
    }
}
