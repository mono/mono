//------------------------------------------------------------------------------
// <copyright file="AspNetWebSocketContextImpl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.WebSockets;
    using System.Security.Principal;
    using System.Web.Caching;
    using System.Web.Profile;

    // Concrete implementation of AspNetWebSocketContext

    internal sealed class AspNetWebSocketContextImpl : AspNetWebSocketContext {

        // The HttpContextBase below isn't the entire HttpContext object graph,
        // but rather a very small subset that contains only properties relevant
        // to a WebSockets request. This should prevent the Gen 2 heap from being
        // spammed with data that's irrelevant to a WebSockets request.
        private readonly HttpContextBase _httpContext;
        private readonly HttpWorkerRequest _workerRequest;
        private readonly AspNetWebSocket _webSocket;

        private CookieCollection _cookieCollection;

        public AspNetWebSocketContextImpl(HttpContextBase httpContext = null, HttpWorkerRequest workerRequest = null, AspNetWebSocket webSocket = null) {
            _httpContext = httpContext;
            _workerRequest = workerRequest;
            _webSocket = webSocket;
        }

        public override string AnonymousID {
            get {
                return _httpContext.Request.AnonymousID;
            }
        }

        public override HttpApplicationStateBase Application {
            get {
                return _httpContext.Application;
            }
        }

        public override string ApplicationPath {
            get {
                return _httpContext.Request.ApplicationPath;
            }
        }

        public override Cache Cache {
            get {
                return _httpContext.Cache;
            }
        }

        public override HttpClientCertificate ClientCertificate {
            get {
                return _httpContext.Request.ClientCertificate;
            }
        }

        public override CookieCollection CookieCollection {
            get {
                if (_cookieCollection == null) {
                    // converts a System.Web.HttpCookieCollection to a System.Net.CookieCollection
                    CookieCollection cc = new CookieCollection();
                    HttpCookieCollection hcc = Cookies;
                    for (int i = 0; i < hcc.Count; i++) {
                        HttpCookie cookie = hcc.Get(i);
                        cc.Add(new Cookie {
                            Name = cookie.Name,
                            Value = cookie.Value,
                            HttpOnly = cookie.HttpOnly,
                            Path = cookie.Path,
                            Secure = cookie.Secure,
                            Domain = cookie.Domain,
                            Expires = cookie.Expires
                        });
                    }
                    _cookieCollection = cc;
                }

                return _cookieCollection;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _httpContext.Request.Cookies;
            }
        }

        public override string FilePath {
            get {
                return _httpContext.Request.FilePath;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpContext.Request.Headers;
            }
        }

        public override bool IsAuthenticated {
            get {
                return _httpContext.Request.IsAuthenticated;
            }
        }

        public override bool IsClientConnected {
            get {
                return _workerRequest.IsClientConnected();
            }
        }

        public override bool IsDebuggingEnabled {
            get {
                return _httpContext.IsDebuggingEnabled;
            }
        }

        public override bool IsLocal {
            get {
                return _httpContext.Request.IsLocal;
            }
        }

        public override bool IsSecureConnection {
            get {
                return _httpContext.Request.IsSecureConnection;
            }
        }

        public override IDictionary Items {
            get {
                return _httpContext.Items;
            }
        }

        public override WindowsIdentity LogonUserIdentity {
            get {
                return _httpContext.Request.LogonUserIdentity;
            }
        }

        public override string Origin
        {
            get
            {
                return Headers["Origin"];
            }
        }

        public override string Path {
            get {
                return _httpContext.Request.Path;
            }
        }

        public override string PathInfo {
            get {
                return _httpContext.Request.PathInfo;
            }
        }

        public override ProfileBase Profile {
            get {
                return _httpContext.Profile;
            }
        }

        public override NameValueCollection QueryString {
            get {
                return _httpContext.Request.QueryString;
            }
        }

        public override string RawUrl {
            get {
                return _httpContext.Request.RawUrl;
            }
        }

        public override Uri RequestUri {
            get {
                return _httpContext.Request.Url;
            }
        }

        public override string SecWebSocketKey {
            get {
                return Headers["Sec-WebSocket-Key"];
            }
        }

        public override IEnumerable<string> SecWebSocketProtocols {
            get {
                return _httpContext.WebSocketRequestedProtocols;
            }
        }

        public override string SecWebSocketVersion {
            get {
                return Headers["Sec-WebSocket-Version"];
            }
        }

        public override HttpServerUtilityBase Server {
            get {
                return _httpContext.Server;
            }
        }

        public override NameValueCollection ServerVariables {
            get {
                return _httpContext.Request.ServerVariables;
            }
        }

        public override DateTime Timestamp {
            get {
                return _httpContext.Timestamp;
            }
        }

        public override UnvalidatedRequestValuesBase Unvalidated {
            get {
                return _httpContext.Request.Unvalidated;
            }
        }

        public override Uri UrlReferrer {
            get {
                return _httpContext.Request.UrlReferrer;
            }
        }

        public override IPrincipal User {
            get {
                return _httpContext.User;
            }
        }

        public override string UserAgent {
            get {
                return _httpContext.Request.UserAgent;
            }
        }

        public override string UserHostAddress {
            get {
                return _httpContext.Request.UserHostAddress;
            }
        }

        public override string UserHostName {
            get {
                return _httpContext.Request.UserHostName;
            }
        }

        public override string[] UserLanguages {
            get {
                return _httpContext.Request.UserLanguages;
            }
        }

        public override WebSocket WebSocket {
            get {
                return _webSocket;
            }
        }

    }
}
