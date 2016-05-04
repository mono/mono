//------------------------------------------------------------------------------
// <copyright file="AspNetWebSocketContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.WebSockets;
    using System.Security.Principal;
    using System.Web.Caching;
    using System.Web.Profile;

    // Mockable context object that's similar to HttpContextBase, but for WebSocket requests

    public abstract class AspNetWebSocketContext : WebSocketContext {

        //Maps to HttpRequest.AnonymousID
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = @"Inline with HttpRequest")]
        public virtual string AnonymousID {
            get { throw new NotImplementedException(); }
        }

        // Maps to HttpContext.Application
        public virtual HttpApplicationStateBase Application {
            get { throw new NotImplementedException(); }
        }

        //Maps to HttpRequest.ApplicationPath
        public virtual string ApplicationPath {
            get { throw new NotImplementedException(); }
        }

        //Access to the ASP.NET Cache object normally available off of
        //HttpContext.Current.Cache
        public virtual Cache Cache {
            get { throw new NotImplementedException(); }
        }

        //Access to the client certificate (if any)
        public virtual HttpClientCertificate ClientCertificate {
            get { throw new NotImplementedException(); }
        }

        // returns the number of active WebSockets connections (DevDiv #200247)
        public static int ConnectionCount {
            get { return AspNetWebSocketManager.Current.ActiveSocketCount; }
        }

        public override CookieCollection CookieCollection {
            get { throw new NotImplementedException(); }
        }

        //Access to cookies using ASP.NET cookie types
        public virtual HttpCookieCollection Cookies {
            get { throw new NotImplementedException(); }
        }

        //maps to HttpRequest.FilePath
        public virtual string FilePath {
            get { throw new NotImplementedException(); }
        }

        public override NameValueCollection Headers {
            get { throw new NotImplementedException(); }
        }

        public override bool IsAuthenticated {
            get { throw new NotImplementedException(); }
        }

        //Can be used by the websocket developer to detect if the underlying
        //TCP/IP connection is still alive.
        public virtual bool IsClientConnected {
            get { throw new NotImplementedException(); }
        }

        //maps to HttpContext.IsDebuggingEnabled
        public virtual bool IsDebuggingEnabled {
            get { throw new NotImplementedException(); }
        }

        public override bool IsLocal {
            get { throw new NotImplementedException(); }
        }

        public override bool IsSecureConnection {
            get { throw new NotImplementedException(); }
        }

        // maps to HttpContext.Items
        public virtual IDictionary Items {
            get { throw new NotImplementedException(); }
        }

        //Access to the underlying IIS security token for the current request.
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon", Justification = @"Inline with HttpRequest.LogonUserIdentity")]
        public virtual WindowsIdentity LogonUserIdentity {
            get { throw new NotImplementedException(); }
        }

        public override string Origin
        {
            get { throw new NotImplementedException(); }
        }

        //Maps to HttpRequest.Path
        public virtual string Path {
            get { throw new NotImplementedException(); }
        }

        //Maps to HttpRequest.PathInfo
        public virtual string PathInfo {
            get { throw new NotImplementedException(); }
        }

        //Maps to HttpContext.Profile
        public virtual ProfileBase Profile {
            get { throw new NotImplementedException(); }
        }

        //The query-string of the websocket request Url
        public virtual NameValueCollection QueryString {
            get { throw new NotImplementedException(); }
        }

        //The raw request Url exposes as a string
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = @"Inline with HttpRequest.RawUrl")]
        public virtual string RawUrl {
            get { throw new NotImplementedException(); }
        }

        public override Uri RequestUri {
            get { throw new NotImplementedException(); }
        }

        public override string SecWebSocketKey {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<string> SecWebSocketProtocols {
            get { throw new NotImplementedException(); }
        }

        public override string SecWebSocketVersion {
            get { throw new NotImplementedException(); }
        }

        // Maps to HttpContext.Server
        public virtual HttpServerUtilityBase Server {
            get { throw new NotImplementedException(); }
        }

        // Maps to HttpRequest.ServerVariables
        public virtual NameValueCollection ServerVariables {
            get { throw new NotImplementedException(); }
        }

        //Maps to HttpContext.Timestamp
        public virtual DateTime Timestamp {
            get { throw new NotImplementedException(); }
        }

        // Maps to HttpRequest.Unvalidated
        public virtual UnvalidatedRequestValuesBase Unvalidated {
            get { throw new NotImplementedException(); }
        }

        //Same as HttpRequest.UrlReferrer
        public virtual Uri UrlReferrer {
            get { throw new NotImplementedException(); }
        }

        public override IPrincipal User {
            get { throw new NotImplementedException(); }
        }

        //Same as HttpRequest.UserAgent
        public virtual string UserAgent {
            get { throw new NotImplementedException(); }
        }

        //Same as HttpRequest.UserHostAddress
        public virtual string UserHostAddress {
            get { throw new NotImplementedException(); }
        }

        //Same as HttpRequest.UserHostName
        public virtual string UserHostName {
            get { throw new NotImplementedException(); }
        }

        //Same as HttpRequest.UserLanguages
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = @"Inline with HttpRequest.UserLanguages")]
        public virtual string[] UserLanguages {
            get { throw new NotImplementedException(); }
        }

        public override WebSocket WebSocket {
            get { throw new NotImplementedException(); }
        }

    }
}
