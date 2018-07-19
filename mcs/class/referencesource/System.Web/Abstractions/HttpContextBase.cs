//------------------------------------------------------------------------------
// <copyright file="HttpContextBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Instrumentation;
    using System.Web.Profile;
    using System.Web.SessionState;
    using System.Web.WebSockets;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    [SuppressMessage("Microsoft.Usage", "CA2302:FlagServiceProviders", Justification ="IServiceProvider implementation is not supported on this abstract class.")]
    public abstract class HttpContextBase : IServiceProvider
    {
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = @"The normal event pattern doesn't work between HttpContext and HttpContextBase since the signatures differ.")]
        public virtual ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Matches HttpContext class")]
        public virtual Exception[] AllErrors {
            get {
                throw new NotImplementedException();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual bool AllowAsyncDuringSyncStages {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual HttpApplicationStateBase Application {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpApplication ApplicationInstance {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual AsyncPreloadModeFlags AsyncPreloadMode {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual Cache Cache {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IHttpHandler CurrentHandler {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual RequestNotification CurrentNotification {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Error",
            Justification = "Matches HttpContext class")]
        public virtual Exception Error {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IHttpHandler Handler {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsCustomErrorEnabled {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsDebuggingEnabled {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsPostNotification {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsWebSocketRequest {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsWebSocketRequestUpgrading {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IDictionary Items {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual PageInstrumentationService PageInstrumentation {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IHttpHandler PreviousHandler {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual ProfileBase Profile {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpRequestBase Request {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpResponseBase Response {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpServerUtilityBase Server {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpSessionStateBase Session {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SkipAuthorization {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual DateTime Timestamp {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool ThreadAbortOnTimeout {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual TraceContext Trace {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IPrincipal User {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual string WebSocketNegotiatedProtocol {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IList<string> WebSocketRequestedProtocols {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc) {
            throw new NotImplementedException();
        }

        public virtual void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options) {
            throw new NotImplementedException();
        }       

        public virtual void AddError(Exception errorInfo) {
            throw new NotImplementedException();
        }

        public virtual void ClearError() {
            throw new NotImplementedException();
        }

        public virtual ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target) {
            throw new NotImplementedException();
        }

        public virtual object GetGlobalResourceObject(string classKey, string resourceKey) {
            throw new NotImplementedException();
        }

        public virtual object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public virtual object GetLocalResourceObject(string virtualPath, string resourceKey) {
            throw new NotImplementedException();
        }

        public virtual object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public virtual object GetSection(string sectionName) {
            throw new NotImplementedException();
        }

        public virtual void RemapHandler(IHttpHandler handler) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Matches HttpContext class")]
        public virtual void RewritePath(string path) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Matches HttpContext class")]
        public virtual void RewritePath(string path, bool rebaseClientPath) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Matches HttpContext class")]
        public virtual void RewritePath(string filePath, string pathInfo, string queryString) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters",
            Justification = "Matches HttpContext class")]
        public virtual void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) {
            throw new NotImplementedException();
        }

        public virtual void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior) {
            throw new NotImplementedException();
        }

        #region IServiceProvider Members
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public virtual object GetService(Type serviceType) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
