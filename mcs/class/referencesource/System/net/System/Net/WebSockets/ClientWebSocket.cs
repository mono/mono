//------------------------------------------------------------------------------
// <copyright file="ClientWebSocket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ClientWebSocket : WebSocket
    {
        private readonly ClientWebSocketOptions options;
        private WebSocket innerWebSocket;
        private readonly CancellationTokenSource cts;

        // Stages of this class. Interlocked doesn't support enums.
        private int state;
        private const int created = 0;
        private const int connecting = 1;
        private const int connected = 2;
        private const int disposed = 3;

        static ClientWebSocket() 
        {
            // Register ws: and wss: with WebRequest.Register so that WebRequest.Create returns a 
            // WebSocket capable HttpWebRequest instance.
            WebSocket.RegisterPrefixes();
        }
        
        public ClientWebSocket() 
        {
            if (Logging.On) Logging.Enter(Logging.WebSockets, this, ".ctor", null);

            if (!WebSocketProtocolComponent.IsSupported)
            {
                WebSocketHelpers.ThrowPlatformNotSupportedException_WSPC();
            }

            state = created;
            options = new ClientWebSocketOptions();
            cts = new CancellationTokenSource();

            if (Logging.On) Logging.Exit(Logging.WebSockets, this, ".ctor", null);
        }

        #region Properties

        public ClientWebSocketOptions Options { get { return options; } }

        public override WebSocketCloseStatus? CloseStatus
        {
            get 
            {
                if (innerWebSocket != null)
                {
                    return innerWebSocket.CloseStatus;
                }
                return null; 
            }
        }

        public override string CloseStatusDescription
        {
            get
            {
                if (innerWebSocket != null)
                {
                    return innerWebSocket.CloseStatusDescription;
                }
                return null;
            }
        }

        public override string SubProtocol
        {
            get
            {
                if (innerWebSocket != null)
                {
                    return innerWebSocket.SubProtocol;
                }
                return null;
            }
        }

        public override WebSocketState State
        {
            get 
            {
                // state == Connected or Disposed
                if (innerWebSocket != null)
                {
                    return innerWebSocket.State;
                }
                switch (state)
                {
                    case created:
                        return WebSocketState.None;
                    case connecting:
                        return WebSocketState.Connecting;
                    case disposed: // We only get here if disposed before connecting
                        return WebSocketState.Closed;
                    default:
                        Contract.Assert(false, "NotImplemented: " + state);
                        return WebSocketState.Closed;
                }
            }
        }

        #endregion Properties

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException(SR.GetString(SR.net_uri_NotAbsolute), "uri");
            }
            if (uri.Scheme != Uri.UriSchemeWs && uri.Scheme != Uri.UriSchemeWss)
            {
                throw new ArgumentException(SR.GetString(SR.net_WebSockets_Scheme), "uri");
            }

            // Check that we have not started already
            int priorState = Interlocked.CompareExchange(ref state, connecting, created);
            if (priorState == disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            else if (priorState != created)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_WebSockets_AlreadyStarted));
            }
            options.SetToReadOnly();

            return ConnectAsyncCore(uri, cancellationToken);
        }

        private async Task ConnectAsyncCore(Uri uri, CancellationToken cancellationToken)
        {
            HttpWebResponse response = null;
            CancellationTokenRegistration connectCancellation = new CancellationTokenRegistration();
            // Any errors from here on out are fatal and this instance will be disposed.
            try
            {
                HttpWebRequest request = CreateAndConfigureRequest(uri);
                if (Logging.On) Logging.Associate(Logging.WebSockets, this, request);

                connectCancellation = cancellationToken.Register(AbortRequest, request, false);

                response = await request.GetResponseAsync().SuppressContextFlow() as HttpWebResponse;
                Contract.Assert(response != null, "Not an HttpWebResponse");

                if (Logging.On) Logging.Associate(Logging.WebSockets, this, response);

                string subprotocol = ValidateResponse(request, response);

                innerWebSocket = WebSocket.CreateClientWebSocket(response.GetResponseStream(), subprotocol,
                    options.ReceiveBufferSize, options.SendBufferSize, options.KeepAliveInterval, false,
                    options.GetOrCreateBuffer());

                if (Logging.On) Logging.Associate(Logging.WebSockets, this, innerWebSocket);

                // Change internal state to 'connected' to enable the other methods
                if (Interlocked.CompareExchange(ref state, connected, connecting) != connecting)
                {
                    // Aborted/Disposed during connect.
                    throw new ObjectDisposedException(GetType().FullName);
                }
            }
            catch (WebException ex)
            {
                ConnectExceptionCleanup(response);
                WebSocketException wex = new WebSocketException(SR.GetString(SR.net_webstatus_ConnectFailure), ex);
                if (Logging.On) Logging.Exception(Logging.WebSockets, this, "ConnectAsync", wex);
                throw wex;
            }
            catch (Exception ex)
            {
                ConnectExceptionCleanup(response);
                if (Logging.On) Logging.Exception(Logging.WebSockets, this, "ConnectAsync", ex);
                throw;
            }
            finally
            {
                // We successfully connected (or failed trying), disengage from this token.  
                // Otherwise any timeout/cancellation would apply to the full session.
                // In the failure case we need to release the reference to HWR.
                connectCancellation.Dispose();
            }
        }

        private void ConnectExceptionCleanup(HttpWebResponse response)
        {
            Dispose();
            if (response != null)
            {
                response.Dispose();
            }
        }

        private HttpWebRequest CreateAndConfigureRequest(Uri uri)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            if (request == null)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_WebSockets_InvalidRegistration));
            }

            // Request Headers
            foreach (string key in options.RequestHeaders.Keys)
            {
                request.Headers.Add(key, options.RequestHeaders[key]);
            }

            // SubProtocols
            if (options.RequestedSubProtocols.Count > 0)
            {
                request.Headers.Add(HttpKnownHeaderNames.SecWebSocketProtocol,
                    string.Join(", ", options.RequestedSubProtocols));
            }

            // Creds
            if (options.UseDefaultCredentials)
            {
                request.UseDefaultCredentials = true;
            }
            else if (options.Credentials != null)
            {
                request.Credentials = options.Credentials;
            }

            // Certs
            if (options.InternalClientCertificates != null)
            {
                request.ClientCertificates = options.InternalClientCertificates;
            }

            request.Proxy = options.Proxy;
            request.CookieContainer = options.Cookies;

            // For Abort/Dispose.  Calling Abort on the request at any point will close the connection.
            cts.Token.Register(AbortRequest, request, false);

            return request;
        }
        
        // Validate the response headers and return the sub-protocol.
        private string ValidateResponse(HttpWebRequest request, HttpWebResponse response)
        {
            // 101
            if (response.StatusCode != HttpStatusCode.SwitchingProtocols)
            {
                throw new WebSocketException(SR.GetString(SR.net_WebSockets_Connect101Expected, 
                    (int)response.StatusCode));
            }

            // Upgrade: websocket
            string upgradeHeader = response.Headers[HttpKnownHeaderNames.Upgrade];
            if (!string.Equals(upgradeHeader, WebSocketHelpers.WebSocketUpgradeToken, 
                StringComparison.OrdinalIgnoreCase))
            {
                throw new WebSocketException(SR.GetString(SR.net_WebSockets_InvalidResponseHeader, 
                    HttpKnownHeaderNames.Upgrade, upgradeHeader));
            }

            // Connection: Upgrade
            string connectionHeader = response.Headers[HttpKnownHeaderNames.Connection];
            if (!string.Equals(connectionHeader, HttpKnownHeaderNames.Upgrade,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new WebSocketException(SR.GetString(SR.net_WebSockets_InvalidResponseHeader,
                    HttpKnownHeaderNames.Connection, connectionHeader));
            }

            // Sec-WebSocket-Accept derived from request Sec-WebSocket-Key
            string websocketAcceptHeader = response.Headers[HttpKnownHeaderNames.SecWebSocketAccept];
            string expectedAcceptHeader = WebSocketHelpers.GetSecWebSocketAcceptString(
                request.Headers[HttpKnownHeaderNames.SecWebSocketKey]);
            if (!string.Equals(websocketAcceptHeader, expectedAcceptHeader, StringComparison.OrdinalIgnoreCase))
            {
                throw new WebSocketException(SR.GetString(SR.net_WebSockets_InvalidResponseHeader,
                    HttpKnownHeaderNames.SecWebSocketAccept, websocketAcceptHeader));
            }

            // Sec-WebSocket-Protocol matches one from request
            // A missing header is ok.  It's also ok if the client didn't specify any.
            string subProtocol = response.Headers[HttpKnownHeaderNames.SecWebSocketProtocol];
            if (!string.IsNullOrWhiteSpace(subProtocol) && options.RequestedSubProtocols.Count > 0)
            {
                bool foundMatch = false;
                foreach (string requestedSubProtocol in options.RequestedSubProtocols)
                {
                    if (string.Equals(requestedSubProtocol, subProtocol, StringComparison.OrdinalIgnoreCase))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    throw new WebSocketException(SR.GetString(SR.net_WebSockets_AcceptUnsupportedProtocol,
                        string.Join(", ", options.RequestedSubProtocols), subProtocol));
                }
            }

            return string.IsNullOrWhiteSpace(subProtocol) ? null : subProtocol; // May be null or valid.
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, 
            CancellationToken cancellationToken)
        {
            ThrowIfNotConnected();
            return innerWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }

        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, 
            CancellationToken cancellationToken)
        {
            ThrowIfNotConnected();
            return innerWebSocket.ReceiveAsync(buffer, cancellationToken);
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            ThrowIfNotConnected();
            return innerWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            ThrowIfNotConnected();
            return innerWebSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
        }

        public override void Abort()
        {
            if (state == disposed)
            {
                return;
            }
            if (innerWebSocket != null)
            {
                innerWebSocket.Abort();
            }
            Dispose();
        }

        private void AbortRequest(object obj)
        {
            HttpWebRequest request = (HttpWebRequest)obj;
            request.Abort();
        }

        public override void Dispose()
        {
            int priorState = Interlocked.Exchange(ref state, disposed);
            if (priorState == disposed)
            {
                // No cleanup required.
                return;
            }
            cts.Cancel(false);
            cts.Dispose();
            if (innerWebSocket != null)
            {
                innerWebSocket.Dispose();
            }
        }

        private void ThrowIfNotConnected()
        {
            if (state == disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            else if (state != connected)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_WebSockets_NotConnected));
            }
        }
    }

    public sealed class ClientWebSocketOptions
    {
        private bool isReadOnly; // After ConnectAsync is called the options cannot be modified.
        private readonly IList<string> requestedSubProtocols;
        private readonly WebHeaderCollection requestHeaders;
        private TimeSpan keepAliveInterval;
        private int receiveBufferSize;
        private int sendBufferSize;
        private ArraySegment<byte>? buffer;
        private bool useDefaultCredentials;
        private ICredentials credentials;
        private IWebProxy proxy;
        private X509CertificateCollection clientCertificates;
        private CookieContainer cookies;

        internal ClientWebSocketOptions()
        {
            requestedSubProtocols = new List<string>();
            requestHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
            Proxy = WebRequest.DefaultWebProxy;
            receiveBufferSize = WebSocketHelpers.DefaultReceiveBufferSize;
            sendBufferSize = WebSocketHelpers.DefaultClientSendBufferSize;
            keepAliveInterval = WebSocket.DefaultKeepAliveInterval;
        }

        #region HTTP Settings

        // Note that some headers are restricted like Host.
        public void SetRequestHeader(string headerName, string headerValue)
        {
            ThrowIfReadOnly();
            // WebHeadersColection performs the validation
            requestHeaders.Set(headerName, headerValue);
        }

        internal WebHeaderCollection RequestHeaders { get { return requestHeaders; } }

        public bool UseDefaultCredentials
        {
            get
            {
                return useDefaultCredentials;
            }
            set
            {
                ThrowIfReadOnly();
                useDefaultCredentials = value;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return credentials;
            }
            set
            {
                ThrowIfReadOnly();
                credentials = value;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return proxy;
            }
            set
            {
                ThrowIfReadOnly();
                proxy = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", 
            Justification = "This collectin will be handed off directly to HttpWebRequest.")]
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                if (clientCertificates == null)
                {
                    clientCertificates = new X509CertificateCollection();
                }
                return clientCertificates;
            }
            set
            {
                ThrowIfReadOnly();
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                clientCertificates = value;
            }
        }

        internal X509CertificateCollection InternalClientCertificates { get { return clientCertificates; } }

        public CookieContainer Cookies
        {
            get
            {
                return cookies;
            }
            set
            {
                ThrowIfReadOnly();
                cookies = value;
            }
        }

        #endregion HTTP Settings

        #region WebSocket Settings

        public void SetBuffer(int receiveBufferSize, int sendBufferSize)
        {
            ThrowIfReadOnly();
            WebSocketHelpers.ValidateBufferSizes(receiveBufferSize, sendBufferSize);

            this.buffer = null;
            this.receiveBufferSize = receiveBufferSize;
            this.sendBufferSize = sendBufferSize;
        }

        public void SetBuffer(int receiveBufferSize, int sendBufferSize, ArraySegment<byte> buffer)
        {
            ThrowIfReadOnly();
            WebSocketHelpers.ValidateBufferSizes(receiveBufferSize, sendBufferSize);
            WebSocketHelpers.ValidateArraySegment(buffer, "buffer");
            WebSocketBuffer.Validate(buffer.Count, receiveBufferSize, sendBufferSize, false);

            this.receiveBufferSize = receiveBufferSize;
            this.sendBufferSize = sendBufferSize;
            
            // Only full-trust applications can specify their own buffer to be used as the
            // internal buffer for the WebSocket object.  This is because the contents of the
            // buffer are used internally by the WebSocket as it marshals data with embedded
            // pointers to native code.  A malicious application could use this to corrupt
            // native memory.
            if (AppDomain.CurrentDomain.IsFullyTrusted)
            {
                this.buffer = buffer;
            }
            else
            {
                // We silently ignore the passed in buffer and will create an internal
                // buffer later.
                this.buffer = null;
            }
        }

        internal int ReceiveBufferSize { get { return receiveBufferSize; } }

        internal int SendBufferSize { get { return sendBufferSize; } }

        internal ArraySegment<byte> GetOrCreateBuffer()
        {
            if (!buffer.HasValue)
            {
                buffer = WebSocket.CreateClientBuffer(receiveBufferSize, sendBufferSize);
            }
            return buffer.Value;
        }

        public void AddSubProtocol(string subProtocol) 
        {
            ThrowIfReadOnly();
            WebSocketHelpers.ValidateSubprotocol(subProtocol);
            // Duplicates not allowed.
            foreach (string item in requestedSubProtocols)
            {
                if (string.Equals(item, subProtocol, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(SR.GetString(SR.net_WebSockets_NoDuplicateProtocol, subProtocol), 
                        "subProtocol");
                }
            }
            requestedSubProtocols.Add(subProtocol);
        }

        internal IList<string> RequestedSubProtocols { get { return requestedSubProtocols; } }

        public TimeSpan KeepAliveInterval 
        {
            get
            {
                return keepAliveInterval;
            }
            set
            {
                ThrowIfReadOnly();
                if (value < Timeout.InfiniteTimeSpan)
                {
                    throw new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.net_WebSockets_ArgumentOutOfRange_TooSmall,
                        Timeout.InfiniteTimeSpan.ToString()));
                }
                keepAliveInterval = value;
            }
        }

        #endregion WebSocket settings

        #region Helpers

        internal void SetToReadOnly()
        {
            Contract.Assert(!isReadOnly, "Already set");
            isReadOnly = true;
        }

        private void ThrowIfReadOnly()
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(SR.GetString(SR.net_WebSockets_AlreadyStarted));
            }
        }

        #endregion Helpers
    }
}
