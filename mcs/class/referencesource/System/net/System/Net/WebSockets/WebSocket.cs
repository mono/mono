//------------------------------------------------------------------------------
// <copyright file="WebSocket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class WebSocket : IDisposable
    {
        public abstract Nullable<WebSocketCloseStatus> CloseStatus { get; }
        public abstract string CloseStatusDescription { get; }
        public abstract string SubProtocol { get; }
        public abstract WebSocketState State { get; }

        private static TimeSpan? defaultKeepAliveInterval;

        public static TimeSpan DefaultKeepAliveInterval
        {
            [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
                Justification = "This is a harmless read-only operation")]
            get
            {
                if (defaultKeepAliveInterval == null)
                {
                    if (WebSocketProtocolComponent.IsSupported)
                    {
                        defaultKeepAliveInterval = WebSocketProtocolComponent.WebSocketGetDefaultKeepAliveInterval();
                    }
                    else
                    {
                        defaultKeepAliveInterval = Timeout.InfiniteTimeSpan;
                    }
                }
                return defaultKeepAliveInterval.Value;
            }
        }

        public static ArraySegment<byte> CreateClientBuffer(int receiveBufferSize, int sendBufferSize)
        {
            WebSocketHelpers.ValidateBufferSizes(receiveBufferSize, sendBufferSize);

            return WebSocketBuffer.CreateInternalBufferArraySegment(receiveBufferSize, sendBufferSize, false);
        }

        public static ArraySegment<byte> CreateServerBuffer(int receiveBufferSize)
        {
            WebSocketHelpers.ValidateBufferSizes(receiveBufferSize, WebSocketBuffer.MinSendBufferSize);

            return WebSocketBuffer.CreateInternalBufferArraySegment(receiveBufferSize, WebSocketBuffer.MinSendBufferSize, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static WebSocket CreateClientWebSocket(Stream innerStream, 
            string subProtocol,
            int receiveBufferSize,
            int sendBufferSize,
            TimeSpan keepAliveInterval, 
            bool useZeroMaskingKey, 
            ArraySegment<byte> internalBuffer)
        {
            if (!WebSocketProtocolComponent.IsSupported)
            {
                WebSocketHelpers.ThrowPlatformNotSupportedException_WSPC();
            }

            WebSocketHelpers.ValidateInnerStream(innerStream);
            WebSocketHelpers.ValidateOptions(subProtocol, receiveBufferSize, sendBufferSize, keepAliveInterval);
            WebSocketHelpers.ValidateArraySegment<byte>(internalBuffer, "internalBuffer");
            WebSocketBuffer.Validate(internalBuffer.Count, receiveBufferSize, sendBufferSize, false);

            return new InternalClientWebSocket(innerStream, 
                subProtocol, 
                receiveBufferSize, 
                sendBufferSize, 
                keepAliveInterval, 
                useZeroMaskingKey, 
                internalBuffer);
        }

        internal static WebSocket CreateServerWebSocket(Stream innerStream,
            string subProtocol,
            int receiveBufferSize,
            TimeSpan keepAliveInterval,
            ArraySegment<byte> internalBuffer)
        {
            if (!WebSocketProtocolComponent.IsSupported)
            {
                WebSocketHelpers.ThrowPlatformNotSupportedException_WSPC();
            }

            WebSocketHelpers.ValidateInnerStream(innerStream);
            WebSocketHelpers.ValidateOptions(subProtocol, receiveBufferSize, WebSocketBuffer.MinSendBufferSize, keepAliveInterval);
            WebSocketHelpers.ValidateArraySegment<byte>(internalBuffer, "internalBuffer");
            WebSocketBuffer.Validate(internalBuffer.Count, receiveBufferSize, WebSocketBuffer.MinSendBufferSize, true);

            return new ServerWebSocket(innerStream, 
                subProtocol, 
                receiveBufferSize,
                keepAliveInterval, 
                internalBuffer);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterPrefixes()
        {
            WebRequest.RegisterPrefix(Uri.UriSchemeWs + ":", new WebSocketHttpRequestCreator(false));
            WebRequest.RegisterPrefix(Uri.UriSchemeWss + ":", new WebSocketHttpRequestCreator(true));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.")]
        public static bool IsApplicationTargeting45()
        {
            return BinaryCompatibility.TargetsAtLeast_Desktop_V4_5;
        }

        public abstract void Abort();
        public abstract Task CloseAsync(WebSocketCloseStatus closeStatus, 
            string statusDescription, 
            CancellationToken cancellationToken);
        public abstract Task CloseOutputAsync(WebSocketCloseStatus closeStatus, 
            string statusDescription, 
            CancellationToken cancellationToken);
        public abstract void Dispose();
        public abstract Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, 
            CancellationToken cancellationToken);
        public abstract Task SendAsync(ArraySegment<byte> buffer, 
            WebSocketMessageType messageType, 
            bool endOfMessage, 
            CancellationToken cancellationToken);

        protected static void ThrowOnInvalidState(WebSocketState state, params WebSocketState[] validStates)
        {
            string validStatesText = string.Empty;

            if (validStates != null && validStates.Length > 0)
            {
                foreach (WebSocketState currentState in validStates)
                {
                    if (state == currentState)
                    {
                        return;
                    }
                }

                validStatesText = string.Join(", ", validStates);
            }

            throw new WebSocketException(SR.GetString(SR.net_WebSockets_InvalidState, state, validStatesText));
        }

        protected static bool IsStateTerminal(WebSocketState state)
        {
            return state == WebSocketState.Closed ||
                   state == WebSocketState.Aborted;
        }
    }
}
