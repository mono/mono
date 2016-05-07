//------------------------------------------------------------------------------
// <copyright file="HttpListenerContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Security.Authentication.ExtendedProtection;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Microsoft.Win32;
    using System.Diagnostics;

    public sealed unsafe class HttpListenerContext /* BaseHttpContext, */   {
        private HttpListener m_Listener;
        private HttpListenerRequest m_Request;
        private HttpListenerResponse m_Response;
        private IPrincipal m_User;
        private string m_MutualAuthentication;
        private AuthenticationSchemes m_AuthenticationSchemes;
        private ExtendedProtectionPolicy m_ExtendedProtectionPolicy;
        private bool m_PromoteCookiesToRfc2965;

        internal const string NTLM = "NTLM";

        internal HttpListenerContext(HttpListener httpListener, RequestContextBase memoryBlob)
        {
            if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpListener#" + ValidationHelper.HashString(httpListener) + " requestBlob=" + ValidationHelper.HashString((IntPtr) memoryBlob.RequestBlob));
            m_Listener = httpListener;
            m_Request = new HttpListenerRequest(this, memoryBlob);
            m_AuthenticationSchemes = httpListener.AuthenticationSchemes;
            m_ExtendedProtectionPolicy = httpListener.ExtendedProtectionPolicy;
            GlobalLog.Print("HttpListenerContext#" + ValidationHelper.HashString(this) + "::.ctor() HttpListener#" + ValidationHelper.HashString(m_Listener) + " HttpListenerRequest#" + ValidationHelper.HashString(m_Request));
        }

        // Call this right after construction, and only once!  Not after it's been handed to a user.
        internal void SetIdentity(IPrincipal principal, string mutualAuthentication)
        {
            m_MutualAuthentication = mutualAuthentication;
            m_User = principal;
            GlobalLog.Print("HttpListenerContext#" + ValidationHelper.HashString(this) + "::SetIdentity() mutual:" + (mutualAuthentication == null ? "<null>" : mutualAuthentication) + " Principal#" + ValidationHelper.HashString(principal));
        }

        public /* new */ HttpListenerRequest Request {
            get {
                return m_Request;
            }
        }

        public /* new */ HttpListenerResponse Response {
            get {
                if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Response", "");
                if (m_Response==null) {
                    m_Response = new HttpListenerResponse(this);
                    GlobalLog.Print("HttpListenerContext#" + ValidationHelper.HashString(this) + "::.Response_get() HttpListener#" + ValidationHelper.HashString(m_Listener) + " HttpListenerRequest#" + ValidationHelper.HashString(m_Request) + " HttpListenerResponse#" + ValidationHelper.HashString(m_Response));
                }
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Response", "");
                return m_Response;
            }
        }

        // Requires ControlPrincipal permission if the request was authenticated with Negotiate, NTLM, or Digest.
        // IsAuthenticated depends on the demand here, so if it is changed (like to a LinkDemand) make sure IsAuthenticated is ok.
        public /* override */ IPrincipal User {
            get {
                if (m_User as WindowsPrincipal == null)
                {
                    return m_User;
                }

                new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Demand();
                return m_User;
            }
        }

        // What auth schemes were expected of this request?
        // This can be used to cache the results of HttpListener.AuthenticationSchemeSelectorDelegate.
        internal AuthenticationSchemes AuthenticationSchemes {
            get { 
                return m_AuthenticationSchemes; 
            }
            set { 
                m_AuthenticationSchemes = value; 
            }
        }

        // This can be used to cache the results of HttpListener.ExtendedProtectionSelectorDelegate.
        internal ExtendedProtectionPolicy ExtendedProtectionPolicy  {
            get { 
                return m_ExtendedProtectionPolicy; 
            }
            set { 
                m_ExtendedProtectionPolicy = value; 
            } 
        }

        // <













        internal bool PromoteCookiesToRfc2965 {
            get {
                return m_PromoteCookiesToRfc2965;
            }
        }

        internal string MutualAuthentication {
            get {
                return m_MutualAuthentication;
            }
        }

        internal HttpListener Listener {
            get {
                return m_Listener;
            }
        }

        internal CriticalHandle RequestQueueHandle {
            get {
                return m_Listener.RequestQueueHandle;
            }
        }

        internal void EnsureBoundHandle()
        {
            m_Listener.EnsureBoundHandle();
        }

        internal ulong RequestId {
            get {
                return Request.RequestId;
            }
        }

        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol)
        {
            return this.AcceptWebSocketAsync(subProtocol, 
                WebSocketHelpers.DefaultReceiveBufferSize,
                WebSocket.DefaultKeepAliveInterval);
        }

        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval)
        {
            return this.AcceptWebSocketAsync(subProtocol,
                WebSocketHelpers.DefaultReceiveBufferSize,
                keepAliveInterval);
        }

        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, 
            int receiveBufferSize, 
            TimeSpan keepAliveInterval)
        {
            WebSocketHelpers.ValidateOptions(subProtocol, receiveBufferSize, WebSocketBuffer.MinSendBufferSize, keepAliveInterval);

            ArraySegment<byte> internalBuffer = WebSocketBuffer.CreateInternalBufferArraySegment(receiveBufferSize, WebSocketBuffer.MinSendBufferSize, true);
            return this.AcceptWebSocketAsync(subProtocol, 
                receiveBufferSize, 
                keepAliveInterval, 
                internalBuffer);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, 
            int receiveBufferSize, 
            TimeSpan keepAliveInterval, 
            ArraySegment<byte> internalBuffer)
        {
            return WebSocketHelpers.AcceptWebSocketAsync(this, 
                subProtocol, 
                receiveBufferSize, 
                keepAliveInterval, 
                internalBuffer);
        }
        
        internal void Close() {
            if(Logging.On) Logging.Enter(Logging.HttpListener, this, "Close()", "");

            try {
                if (m_Response!=null) {
                    m_Response.Close();
                }
            }
            finally {
                try {
                    m_Request.Close();
                }
                finally {
                    IDisposable user = m_User == null ? null : m_User.Identity as IDisposable;

                    // For unsafe connection ntlm auth we dont dispose this identity as yet since its cached
                    if ((user != null) &&
                        (m_User.Identity.AuthenticationType != NTLM) && 
                        (!m_Listener.UnsafeConnectionNtlmAuthentication)) 
                    {
                            user.Dispose();
                    }
                }
            }
            if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Close", "");
        }

        internal void Abort() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Abort", "");
            ForceCancelRequest(RequestQueueHandle, m_Request.RequestId);
            try {
                m_Request.Close();
            }
            finally {
                IDisposable user = m_User == null ? null : m_User.Identity as IDisposable;
                if (user != null) {
                    user.Dispose();
                }
            }
            if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Abort", "");
        }


        internal UnsafeNclNativeMethods.HttpApi.HTTP_VERB GetKnownMethod() {
            GlobalLog.Print("HttpListenerContext::GetKnownMethod()");
            return UnsafeNclNativeMethods.HttpApi.GetKnownVerb(Request.RequestBuffer, Request.OriginalBlobAddress);
        }

        // This is only called while processing incoming requests.  We don't have to worry about cancelling 
        // any response writes.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification =
            "It is safe to ignore the return value on a cancel operation because the connection is being closed")]
        internal static void CancelRequest(CriticalHandle requestQueueHandle, ulong requestId) {
            uint statusCode = UnsafeNclNativeMethods.HttpApi.HttpCancelHttpRequest(requestQueueHandle, requestId,
                IntPtr.Zero);
        }

        // The request is being aborted, but large writes may be in progress. Cancel them.
        internal void ForceCancelRequest(CriticalHandle requestQueueHandle, ulong requestId) {

            uint statusCode = UnsafeNclNativeMethods.HttpApi.HttpCancelHttpRequest(requestQueueHandle, requestId, 
                IntPtr.Zero);

            // Either the connection has already dropped, or the last write is in progress.
            // The requestId becomes invalid as soon as the last Content-Length write starts.
            // The only way to cancel now is with CancelIoEx.
            if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_CONNECTION_INVALID)
            {
                m_Response.CancelLastWrite(requestQueueHandle);
            }
        }

        internal void SetAuthenticationHeaders() {
            Listener.SetAuthenticationHeaders(this);
        }
    }
}
