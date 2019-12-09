// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
    internal sealed class WebSocketHandle
    {
        private WebSocketState _state = WebSocketState.Connecting;
        private WebAssembly.Net.WebSockets.ClientWebSocket _webSocket;

        public static WebSocketHandle Create() => new WebSocketHandle();

        public static bool IsValid(WebSocketHandle handle) => handle != null;

        public WebSocketCloseStatus? CloseStatus => _webSocket?.CloseStatus;

        public string CloseStatusDescription => _webSocket?.CloseStatusDescription;

        public WebSocketState State => _webSocket?.State ?? _state;

        public string SubProtocol => _webSocket?.SubProtocol;

        public static void CheckPlatformSupport() { /* nop */ }

        public void Dispose()
        {
            _state = WebSocketState.Closed;
            _webSocket?.Dispose();
        }

        public void Abort()
        {
            _webSocket?.Abort();
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) =>
            _webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

        public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) =>
            _webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) =>
            _webSocket.ReceiveAsync(buffer, cancellationToken);

        public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken) =>
            _webSocket.ReceiveAsync(buffer, cancellationToken);

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
            _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) =>
            _webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);

        public async Task ConnectAsyncCore(Uri uri, CancellationToken cancellationToken, ClientWebSocketOptions options)
        {
            // TODO #14480 : Not currently implemented, or explicitly ignored:
            // - ClientWebSocketOptions.UseDefaultCredentials
            // - ClientWebSocketOptions.Credentials
            // - ClientWebSocketOptions.Proxy
            // - ClientWebSocketOptions._sendBufferSize
            // throw new PlatformNotSupportedException ();
            // Establish connection to the server
            CancellationTokenRegistration registration = cancellationToken.Register(s => ((WebSocketHandle)s).Abort(), this);
            try
            {
                _webSocket = new WebAssembly.Net.WebSockets.ClientWebSocket ();//(options);
                foreach (var t in options.RequestedSubProtocols) {
                    _webSocket.Options.AddSubProtocol (t);
                }

                await _webSocket.ConnectAsync (uri, cancellationToken);

            }
            catch (Exception exc)
            {
                if (_state < WebSocketState.Closed)
                {
                    _state = WebSocketState.Closed;
                }

                Abort();

                if (exc is WebSocketException)
                {
                    throw;
                }
                throw new WebSocketException(SR.net_webstatus_ConnectFailure, exc);
            }
            finally
            {
                registration.Dispose();
            }
        }
    }
}