//------------------------------------------------------------------------------
// <copyright file="ClientWebSocket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class InternalClientWebSocket : WebSocketBase
    {
        private readonly SafeHandle m_SessionHandle;
        private readonly WebSocketProtocolComponent.Property[] m_Properties;

        public InternalClientWebSocket(Stream innerStream, string subProtocol, int receiveBufferSize, int sendBufferSize,
            TimeSpan keepAliveInterval, bool useZeroMaskingKey, ArraySegment<byte> internalBuffer)
            : base(innerStream, subProtocol, keepAliveInterval, 
                WebSocketBuffer.CreateClientBuffer(internalBuffer, receiveBufferSize, sendBufferSize))
        {
            m_Properties = this.InternalBuffer.CreateProperties(useZeroMaskingKey);
            m_SessionHandle = this.CreateWebSocketHandle();

            if (m_SessionHandle == null || m_SessionHandle.IsInvalid)
            {
                WebSocketHelpers.ThrowPlatformNotSupportedException_WSPC();
            }

            StartKeepAliveTimer();
        }

        internal override SafeHandle SessionHandle
        {
            get
            {
                Contract.Assert(m_SessionHandle != null, "'m_SessionHandle MUST NOT be NULL.");
                return m_SessionHandle;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
            Justification = "Only harmless information (useZeroMaskingKey) is passed from PT code into native code.")]
        private SafeHandle CreateWebSocketHandle()
        {
            Contract.Assert(m_Properties != null, "'m_Properties' MUST NOT be NULL.");
            SafeWebSocketHandle sessionHandle;
            WebSocketProtocolComponent.WebSocketCreateClientHandle(m_Properties, out sessionHandle);
            Contract.Assert(sessionHandle != null, "'sessionHandle MUST NOT be NULL.");

            return sessionHandle;
        }
    }
}
