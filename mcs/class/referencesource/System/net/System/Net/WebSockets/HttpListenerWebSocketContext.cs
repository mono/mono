//------------------------------------------------------------------------------
// <copyright file="HttpListenerWebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Security.Principal;

    public class HttpListenerWebSocketContext : WebSocketContext
    {
        private Uri m_RequestUri;
        private NameValueCollection m_Headers;
        private CookieCollection m_CookieCollection;
        private IPrincipal m_User;
        private bool m_IsAuthenticated;
        private bool m_IsLocal;
        private bool m_IsSecureConnection;

        private string m_Origin;
        private IEnumerable<string> m_SecWebSocketProtocols;
        private string m_SecWebSocketVersion;
        private string m_SecWebSocketKey;

        private WebSocket m_WebSocket;

        internal HttpListenerWebSocketContext(
            Uri requestUri,
            NameValueCollection headers,
            CookieCollection cookieCollection,
            IPrincipal user,
            bool isAuthenticated,
            bool isLocal,
            bool isSecureConnection,
            string origin,
            IEnumerable<string> secWebSocketProtocols,
            string secWebSocketVersion,
            string secWebSocketKey,
            WebSocket webSocket)
        {
            Contract.Assert(requestUri != null, "requestUri shouldn't be null");
            Contract.Assert(headers != null, "headers shouldn't be null");
            Contract.Assert(cookieCollection != null, "cookieCollection shouldn't be null");
            Contract.Assert(secWebSocketProtocols != null, "secWebSocketProtocols shouldn't be null");
            Contract.Assert(webSocket != null, "webSocket shouldn't be null"); 

            m_CookieCollection = new CookieCollection();
            m_CookieCollection.Add(cookieCollection);

            m_Headers = new NameValueCollection(headers);
            m_User = CopyPrincipal(user); 

            m_RequestUri = requestUri;
            m_IsAuthenticated = isAuthenticated; 
            m_IsLocal = isLocal; 
            m_IsSecureConnection = isSecureConnection; 
            m_Origin = origin; 
            m_SecWebSocketProtocols = secWebSocketProtocols;
            m_SecWebSocketVersion = secWebSocketVersion; 
            m_SecWebSocketKey = secWebSocketKey;
            m_WebSocket = webSocket; 
        }

        public override Uri RequestUri
        {
            get { return this.m_RequestUri; }
        }

        public override NameValueCollection Headers
        {
            get { return this.m_Headers; }
        }

        public override string Origin
        {
            get { return this.m_Origin; }
        }

        public override IEnumerable<string> SecWebSocketProtocols
        {
            get { return this.m_SecWebSocketProtocols; }
        }

        public override string SecWebSocketVersion
        {
            get { return this.m_SecWebSocketVersion; }
        }

        public override string SecWebSocketKey
        {
            get { return this.m_SecWebSocketKey; }
        }

        public override CookieCollection CookieCollection
        {
            get { return this.m_CookieCollection; }
        }

        public override IPrincipal User
        {
            get { return this.m_User; }
        }

        public override bool IsAuthenticated
        {
            get { return this.m_IsAuthenticated; }
        }

        public override bool IsLocal
        {
            get { return this.m_IsLocal; }
        }

        public override bool IsSecureConnection
        {
            get { return this.m_IsSecureConnection; }
        }

        public override WebSocket WebSocket
        {
            get { return this.m_WebSocket; }
        }

        private static IPrincipal CopyPrincipal(IPrincipal user)
        {
            // Copy IPrincipal to the new WebSocket object. m_User gets disposed when
            // HttpListenerContext is closed. 
            IPrincipal retVal = null;
            if (user != null)
            {
                if (user as WindowsPrincipal == null)
                {
                    // authtype Basic
                    HttpListenerBasicIdentity basicIdentity = user.Identity as HttpListenerBasicIdentity;
                    if (basicIdentity != null)
                    {
                        retVal = new GenericPrincipal((new HttpListenerBasicIdentity(basicIdentity.Name, 
                            basicIdentity.Password)), null);
                    }
                }
                else
                {
                    // Digest, Negotiate, NTLM 
                    WindowsIdentity windowsIdentity = (WindowsIdentity)user.Identity;
                    retVal = new WindowsPrincipal(HttpListener.CreateWindowsIdentity(windowsIdentity.Token, 
                        windowsIdentity.AuthenticationType, WindowsAccountType.Normal, true));
                }
            }

            return retVal; 
        }
    }
}
