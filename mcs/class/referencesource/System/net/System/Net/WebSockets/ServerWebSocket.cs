//------------------------------------------------------------------------------
// <copyright file="ServerWebSocket.cs" company="Microsoft">
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

    internal sealed class ServerWebSocket : WebSocketBase
    {
        private readonly SafeHandle m_SessionHandle;
        private readonly WebSocketProtocolComponent.Property[] m_Properties;
        
        public ServerWebSocket(Stream innerStream,
            string subProtocol,
            int receiveBufferSize,
            TimeSpan keepAliveInterval,
            ArraySegment<byte> internalBuffer)
            : base(innerStream, subProtocol, keepAliveInterval, 
                WebSocketBuffer.CreateServerBuffer(internalBuffer, receiveBufferSize))
        {
            m_Properties = this.InternalBuffer.CreateProperties(false);
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
            Justification = "No arbitrary data controlled by PT code is leaking into native code.")]
        private SafeHandle CreateWebSocketHandle()
        {
            Contract.Assert(m_Properties != null, "'m_Properties' MUST NOT be NULL.");
            SafeWebSocketHandle sessionHandle;
            WebSocketProtocolComponent.WebSocketCreateServerHandle(m_Properties,
                m_Properties.Length, 
                out sessionHandle);
            Contract.Assert(sessionHandle != null, "'sessionHandle MUST NOT be NULL.");

            return sessionHandle;
        }
    }
}
