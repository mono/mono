// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IO;
    using System.Net;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    class ClientWebSocketTransportDuplexSessionChannel : WebSocketTransportDuplexSessionChannel
    {
        readonly ClientWebSocketFactory connectionFactory;
        HttpChannelFactory<IDuplexSessionChannel> channelFactory;
        Stream connection;
        SecurityTokenProviderContainer webRequestTokenProvider;
        SecurityTokenProviderContainer webRequestProxyTokenProvider;
        HttpWebRequest httpWebRequest;
        string webSocketKey;
        volatile bool cleanupStarted;
        volatile bool cleanupIdentity;

        static ClientWebSocketTransportDuplexSessionChannel()
        {
            WebSocket.RegisterPrefixes();
        }

        public ClientWebSocketTransportDuplexSessionChannel(HttpChannelFactory<IDuplexSessionChannel> channelFactory, ClientWebSocketFactory connectionFactory, EndpointAddress remoteAddresss, Uri via, ConnectionBufferPool bufferPool)
            : base(channelFactory, remoteAddresss, via, bufferPool)
        {
            this.channelFactory = channelFactory;
            this.connectionFactory = connectionFactory;
        }

        protected override bool IsStreamedOutput
        {
            get { return TransferModeHelper.IsRequestStreamed(this.TransferMode); }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool success = false;
            try
            {
                if (TD.WebSocketConnectionRequestSendStartIsEnabled())
                {
                    TD.WebSocketConnectionRequestSendStart(
                        this.EventTraceActivity,
                        this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
                }

                this.httpWebRequest = this.CreateHttpWebRequest(timeout);
                IAsyncResult result = this.httpWebRequest.BeginGetResponse(callback, state);
                success = true;
                return result;
            }
            catch (WebException ex)
            {
                if (TD.WebSocketConnectionFailedIsEnabled())
                {
                    TD.WebSocketConnectionFailed(this.EventTraceActivity, ex.Message);
                }

                TryConvertAndThrow(ex);
                throw FxTrace.Exception.AsError(HttpChannelUtilities.CreateRequestWebException(ex, this.httpWebRequest, HttpAbortReason.None));
            }
            finally
            {
                if (!success)
                {
                    this.CleanupTokenProviders();
                    this.CleanupOnError(this.httpWebRequest, null);
                }
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            bool success = false;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)this.httpWebRequest.EndGetResponse(result);
                this.HandleHttpWebResponse(this.httpWebRequest, response);
                this.RemoveIdentityMapping(false);
                success = true;

                if (TD.WebSocketConnectionRequestSendStopIsEnabled())
                {
                    TD.WebSocketConnectionRequestSendStop(
                        this.EventTraceActivity,
                        this.WebSocket != null ? this.WebSocket.GetHashCode() : -1);
                }
            }
            catch (WebException ex)
            {
                if (TD.WebSocketConnectionFailedIsEnabled())
                {
                    TD.WebSocketConnectionFailed(this.EventTraceActivity, ex.Message);
                }

                TryConvertAndThrow(ex);
                throw FxTrace.Exception.AsError(HttpChannelUtilities.CreateRequestWebException(ex, this.httpWebRequest, HttpAbortReason.None));
            }
            finally
            {
                this.CleanupTokenProviders();

                if (!success)
                {
                    this.CleanupOnError(this.httpWebRequest, response);
                }
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            bool success = false;
            try
            {
                if (TD.WebSocketConnectionRequestSendStartIsEnabled())
                {
                    TD.WebSocketConnectionRequestSendStart(
                        this.EventTraceActivity,
                        this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
                }

                request = this.CreateHttpWebRequest(helper.RemainingTime());
                response = (HttpWebResponse)request.GetResponse();
                this.HandleHttpWebResponse(request, response);
                this.RemoveIdentityMapping(false);
                success = true;

                if (TD.WebSocketConnectionRequestSendStopIsEnabled())
                {
                    TD.WebSocketConnectionRequestSendStop(
                        this.EventTraceActivity,
                        this.WebSocket != null ? this.WebSocket.GetHashCode() : -1);
                }
            }
            catch (WebException ex)
            {
                if (TD.WebSocketConnectionFailedIsEnabled())
                {
                    TD.WebSocketConnectionFailed(this.EventTraceActivity, ex.Message);
                }

                TryConvertAndThrow(ex);
                throw FxTrace.Exception.AsError(HttpChannelUtilities.CreateRequestWebException(ex, request, HttpAbortReason.None));
            }
            finally
            {
                this.CleanupTokenProviders();
                if (!success)
                {
                    this.CleanupOnError(request, response);
                }
            }
        }

        protected override void OnCleanup()
        {
            this.cleanupStarted = true;
            base.OnCleanup();

            if (this.connection != null)
            {
                this.connection.Close();
            }
        }

        static void CheckResponseHeader(HttpWebResponse response, string headerKey, string expectedValue, bool ignoreCase)
        {
            string actualValue = response.Headers[headerKey];
            if (actualValue == null)
            {
                throw FxTrace.Exception.AsError(new CommunicationException(
                        SR.GetString(SR.WebSocketTransportError),
                        new WebSocketException(SR.GetString(
                        SR.WebSocketUpgradeFailedHeaderMissingError, headerKey))));
            }

            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!actualValue.Equals(expectedValue, comparisonType))
            {
                throw FxTrace.Exception.AsError(new CommunicationException(
                        SR.GetString(SR.WebSocketTransportError),
                        new WebSocketException(SR.GetString(
                        SR.WebSocketUpgradeFailedWrongHeaderError, headerKey, actualValue, expectedValue))));
            }
        }

        static void TryConvertAndThrow(WebException ex)
        {
            if (ex.Response != null)
            {
                HttpWebResponse webResponse = (HttpWebResponse)ex.Response;
                if (webResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    string serverContentType = webResponse.Headers[WebSocketTransportSettings.SoapContentTypeHeader];
                    if (!string.IsNullOrWhiteSpace(serverContentType))
                    {
                        string serverTransferMode = webResponse.Headers[WebSocketTransportSettings.BinaryEncoderTransferModeHeader];
                        if (!string.IsNullOrWhiteSpace(serverTransferMode))
                        {
                            throw FxTrace.Exception.AsError(new CommunicationException(SR.GetString(SR.WebSocketContentTypeAndTransferModeMismatchFromServer), ex));
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new CommunicationException(SR.GetString(SR.WebSocketContentTypeMismatchFromServer), ex));
                        }
                    }
                }
                else if (webResponse.StatusCode == HttpStatusCode.UpgradeRequired)
                {
                    string serverVersion = webResponse.Headers[WebSocketHelper.SecWebSocketVersion];
                    if (!string.IsNullOrWhiteSpace(serverVersion))
                    {
                        throw FxTrace.Exception.AsError(new CommunicationException(SR.GetString(SR.WebSocketVersionMismatchFromServer, serverVersion), ex));
                    }

                    string serverSubProtocol = webResponse.Headers[WebSocketHelper.SecWebSocketProtocol];
                    if (!string.IsNullOrWhiteSpace(serverSubProtocol))
                    {
                        throw FxTrace.Exception.AsError(new CommunicationException(SR.GetString(SR.WebSocketSubProtocolMismatchFromServer, serverSubProtocol), ex));
                    }
                }
            }
        }

        void ConfigureHttpWebRequestHeader(HttpWebRequest request)
        {
            if (this.WebSocketSettings.SubProtocol != null)
            {
                request.Headers[WebSocketHelper.SecWebSocketProtocol] = this.WebSocketSettings.SubProtocol;
            }

            // These headers were added for WCF specific handshake to avoid encoder or transfermode mismatch between client and server.
            // For BinaryMessageEncoder, since we are using a sessionful channel for websocket, the encoder is actually different when
            // we are using Buffered or Stramed transfermode. So we need an extra header to identify the transfermode we are using, just
            // to make people a little bit easier to diagnose these mismatch issues.
            if (this.channelFactory.MessageVersion != MessageVersion.None)
            {
                request.Headers[WebSocketTransportSettings.SoapContentTypeHeader] = this.channelFactory.WebSocketSoapContentType;

                if (this.channelFactory.MessageEncoderFactory is BinaryMessageEncoderFactory)
                {
                    request.Headers[WebSocketTransportSettings.BinaryEncoderTransferModeHeader] = this.channelFactory.TransferMode.ToString();
                }
            }
        }

        void CleanupOnError(HttpWebRequest request, HttpWebResponse response)
        {
            if (response != null)
            {
                response.Close();
            }

            if (request != null)
            {
                request.Abort();
            }

            this.Cleanup();
            this.RemoveIdentityMapping(true);
        }

        void RemoveIdentityMapping(bool aborting)
        {
            if (this.cleanupIdentity)
            {
                lock (this.ThisLock)
                {
                    if (this.cleanupIdentity)
                    {
                        this.cleanupIdentity = false;
                        HttpTransportSecurityHelpers.RemoveIdentityMapping(Via, RemoteAddress, !aborting);
                    }
                }
            }
        }

        HttpWebRequest CreateHttpWebRequest(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            ChannelParameterCollection channelParameterCollection = new ChannelParameterCollection();

            HttpWebRequest request;

            if (HttpChannelFactory<IDuplexSessionChannel>.MapIdentity(this.RemoteAddress, this.channelFactory.AuthenticationScheme))
            {
                lock (ThisLock)
                {
                    this.cleanupIdentity = HttpTransportSecurityHelpers.AddIdentityMapping(Via, RemoteAddress);
                }
            }

            this.channelFactory.CreateAndOpenTokenProviders(
                            this.RemoteAddress,
                            this.Via,
                            channelParameterCollection,
                            helper.RemainingTime(),
                            out this.webRequestTokenProvider,
                            out this.webRequestProxyTokenProvider);

            SecurityTokenContainer clientCertificateToken = null;
            HttpsChannelFactory<IDuplexSessionChannel> httpsChannelFactory = this.channelFactory as HttpsChannelFactory<IDuplexSessionChannel>;
            if (httpsChannelFactory != null && httpsChannelFactory.RequireClientCertificate)
            {
                SecurityTokenProvider certificateProvider = httpsChannelFactory.CreateAndOpenCertificateTokenProvider(this.RemoteAddress, this.Via, channelParameterCollection, helper.RemainingTime());
                clientCertificateToken = httpsChannelFactory.GetCertificateSecurityToken(certificateProvider, this.RemoteAddress, this.Via, channelParameterCollection, ref helper);
            }

            request = this.channelFactory.GetWebRequest(this.RemoteAddress, this.Via, this.webRequestTokenProvider, this.webRequestProxyTokenProvider, clientCertificateToken, helper.RemainingTime(), true);

            // If a web socket connection factory is specified (for example, when using web sockets on pre-Win8 OS), 
            // we're going to use the protocol version from it. At the moment, on pre-Win8 OS, the HttpWebRequest 
            // created above doesn't have the version header specified.
            if (this.connectionFactory != null)
            {
                this.UseWebSocketVersionFromFactory(request);
            }

            this.webSocketKey = request.Headers[WebSocketHelper.SecWebSocketKey];
            this.ConfigureHttpWebRequestHeader(request);
            request.Timeout = (int)helper.RemainingTime().TotalMilliseconds;
            return request;
        }

        void CleanupTokenProviders()
        {
            if (this.webRequestTokenProvider != null)
            {
                this.webRequestTokenProvider.Abort();
                this.webRequestTokenProvider = null;
            }

            if (this.webRequestProxyTokenProvider != null)
            {
                this.webRequestProxyTokenProvider.Abort();
                this.webRequestProxyTokenProvider = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule, Justification = "The exception thrown here is already wrapped.")]
        void HandleHttpWebResponse(HttpWebRequest request, HttpWebResponse response)
        {
            this.ValidateHttpWebResponse(response);
            this.connection = response.GetResponseStream();
            WebSocket clientWebSocket = null;

            try
            {
                if (this.connectionFactory != null)
                {
                    this.WebSocket = clientWebSocket = this.CreateWebSocketWithFactory();
                }
                else
                {
                    byte[] internalBuffer = this.TakeBuffer();
                    try
                    {
                        this.WebSocket = clientWebSocket = WebSocket.CreateClientWebSocket(
                                            this.connection,
                                            this.WebSocketSettings.SubProtocol,
                                            WebSocketHelper.GetReceiveBufferSize(this.channelFactory.MaxReceivedMessageSize),
                                            WebSocketDefaults.BufferSize,
                                            this.WebSocketSettings.GetEffectiveKeepAliveInterval(),
                                            this.WebSocketSettings.DisablePayloadMasking,
                                            new ArraySegment<byte>(internalBuffer));
                    }
                    finally
                    {
                        // even when setting this.InternalBuffer in the finally block
                        // there is still a potential race condition, which could result
                        // in not returning 'internalBuffer' to the pool.
                        // This is acceptable since it is extremely unlikely, only for 
                        // the error case and there is no big harm if the buffers are
                        // occasionally not returned to the pool. WebSocketBufferPool.Take()
                        // will just allocate new buffers;
                        this.InternalBuffer = internalBuffer;
                    }
                }
            }
            finally
            {
                // There is a race condition betwene OnCleanup and OnOpen that
                // can result in cleaning up while the clientWebSocket instance is 
                // created. In this case OnCleanup won't be called anymore and would
                // not clean up the WebSocket instance immediately - only GC would
                // cleanup during finalization.
                // To avoid this we abort the WebSocket (and implicitly this.connection)
                if (clientWebSocket != null && this.cleanupStarted)
                {
                    clientWebSocket.Abort();
                    CommunicationObjectAbortedException communicationObjectAbortedException = new CommunicationObjectAbortedException(
                        new WebSocketException(WebSocketError.ConnectionClosedPrematurely).Message);
                    FxTrace.Exception.AsWarning(communicationObjectAbortedException);
                    throw communicationObjectAbortedException;
                }
            }

            bool inputUseStreaming = TransferModeHelper.IsResponseStreamed(this.TransferMode);

            SecurityMessageProperty handshakeReplySecurityMessageProperty = this.channelFactory.CreateReplySecurityProperty(request, response);

            if (handshakeReplySecurityMessageProperty != null)
            {
                this.RemoteSecurity = handshakeReplySecurityMessageProperty;
            }

            this.SetMessageSource(new WebSocketMessageSource(
                    this,
                    this.WebSocket,
                    inputUseStreaming,
                    this));
        }

        void ValidateHttpWebResponse(HttpWebResponse response)
        {
            if (response.StatusCode != HttpStatusCode.SwitchingProtocols)
            {
                throw FxTrace.Exception.AsError(new CommunicationException(
                        SR.GetString(SR.WebSocketTransportError),
                        new WebSocketException(SR.GetString(
                        SR.WebSocketUpgradeFailedError, (int)response.StatusCode, response.StatusDescription, (int)HttpStatusCode.SwitchingProtocols, HttpStatusCode.SwitchingProtocols))));
            }

            CheckResponseHeader(response, HttpTransportDefaults.ConnectionHeader, WebSocketDefaults.WebSocketConnectionHeaderValue, true);
            CheckResponseHeader(response, HttpTransportDefaults.UpgradeHeader, WebSocketDefaults.WebSocketUpgradeHeaderValue, true);
            string expectedAcceptHeader = WebSocketHelper.ComputeAcceptHeader(this.webSocketKey);
            CheckResponseHeader(response, WebSocketHelper.SecWebSocketAccept, expectedAcceptHeader, false);

            if (this.WebSocketSettings.SubProtocol != null)
            {
                CheckResponseHeader(response, WebSocketHelper.SecWebSocketProtocol, this.WebSocketSettings.SubProtocol, true);
            }
            else
            {
                string headerValue = response.Headers[WebSocketHelper.SecWebSocketProtocol];
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    throw FxTrace.Exception.AsError(new CommunicationException(
                        SR.GetString(SR.WebSocketTransportError),
                        new WebSocketException(SR.GetString(
                        SR.WebSocketUpgradeFailedInvalidProtocolError, headerValue))));
                }
            }
        }

        void UseWebSocketVersionFromFactory(HttpWebRequest request)
        {
            Fx.Assert(this.connectionFactory != null, "Invalid call: UseWebSocketVersionFromFactory.");

            if (TD.WebSocketUseVersionFromClientWebSocketFactoryIsEnabled())
            {
                TD.WebSocketUseVersionFromClientWebSocketFactory(this.EventTraceActivity, this.connectionFactory.GetType().FullName);
            }

            // Obtain the WebSocketVersion from the factory.
            string webSocketVersion;
            try
            {
                webSocketVersion = this.connectionFactory.WebSocketVersion;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_GetWebSocketVersionFailed, this.connectionFactory.GetType().Name), e));
            }

            // The WebSocketVersion is a required http header, to initiate a web-socket connection.
            if (string.IsNullOrWhiteSpace(webSocketVersion))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_InvalidWebSocketVersion, this.connectionFactory.GetType().Name)));
            }

            try
            {
                request.Headers[WebSocketHelper.SecWebSocketVersion] = webSocketVersion;
            }
            catch (ArgumentException e)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_InvalidWebSocketVersion, this.connectionFactory.GetType().Name), e));
            }
        }

        WebSocket CreateWebSocketWithFactory()
        {
            Fx.Assert(this.connectionFactory != null, "Invalid call: CreateWebSocketWithFactory.");

            if (TD.WebSocketCreateClientWebSocketWithFactoryIsEnabled())
            {
                TD.WebSocketCreateClientWebSocketWithFactory(this.EventTraceActivity, this.connectionFactory.GetType().FullName);
            }

            // Create the client WebSocket with the factory.
            WebSocket ws;
            try
            {
                ws = this.connectionFactory.CreateWebSocket(this.connection, this.WebSocketSettings.Clone());
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_CreateWebSocketFailed, this.connectionFactory.GetType().Name), e));
            }

            // The returned WebSocket should be valid (non-null), in an opened state and with the same SubProtocol that we requested.
            if (ws == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_InvalidWebSocket, this.connectionFactory.GetType().Name)));
            }
            else if (ws.State != WebSocketState.Open)
            {
                ws.Dispose();
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_InvalidWebSocket, this.connectionFactory.GetType().Name)));
            }
            else
            {
                string requested = this.WebSocketSettings.SubProtocol;
                string obtained = ws.SubProtocol;
                if (!(requested == null ? string.IsNullOrWhiteSpace(obtained) : requested.Equals(obtained, StringComparison.OrdinalIgnoreCase)))
                {
                    ws.Dispose();
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.ClientWebSocketFactory_InvalidSubProtocol, this.connectionFactory.GetType().Name, obtained, requested)));
                }
            }

            return ws;
        }
    }
}
