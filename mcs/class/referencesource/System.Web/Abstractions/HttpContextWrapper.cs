//------------------------------------------------------------------------------
// <copyright file="HttpContextWrapper2.cs" company="Microsoft">
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
    public class HttpContextWrapper : HttpContextBase {
        private readonly HttpContext _context;

        public HttpContextWrapper(HttpContext httpContext) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }
            _context = httpContext;
        }

        public override ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback) {
            return _context.AddOnRequestCompleted(WrapCallback(callback));
        }

        public override Exception[] AllErrors {
            get {
                return _context.AllErrors;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override bool AllowAsyncDuringSyncStages {
            get {
                return _context.AllowAsyncDuringSyncStages;
            }
            set {
                _context.AllowAsyncDuringSyncStages = value;
            }
        }

        public override HttpApplicationStateBase Application {
            get {
                return new HttpApplicationStateWrapper(_context.Application);
            }
        }

        //
        public override HttpApplication ApplicationInstance {
            get {
                return _context.ApplicationInstance;
            }
            set {
                _context.ApplicationInstance = value;
            }
        }

        public override AsyncPreloadModeFlags AsyncPreloadMode {
            get {
                return _context.AsyncPreloadMode;
            }
            set {
                _context.AsyncPreloadMode = value;
            }
        }

        //
        public override Cache Cache {
            get {
                return _context.Cache;
            }
        }

        public override IHttpHandler CurrentHandler {
            get {
                return _context.CurrentHandler;
            }
        }

        public override RequestNotification CurrentNotification {
            get {
                return _context.CurrentNotification;
            }
        }

        public override Exception Error {
            get {
                return _context.Error;
            }
        }

        public override IHttpHandler Handler {
            get {
                return _context.Handler;
            }
            set {
                _context.Handler = value;
            }
        }

        public override bool IsCustomErrorEnabled {
            get {
                return _context.IsCustomErrorEnabled;
            }
        }

        public override bool IsDebuggingEnabled {
            get {
                return _context.IsDebuggingEnabled;
            }
        }

        public override bool IsPostNotification {
            get {
                return _context.IsPostNotification;
            }
        }

        public override bool IsWebSocketRequest {
            get {
                return _context.IsWebSocketRequest;
            }
        }

        public override bool IsWebSocketRequestUpgrading {
            get {
                return _context.IsWebSocketRequestUpgrading;
            }
        }

        public override IDictionary Items {
            get {
                return _context.Items;
            }
        }

        public override PageInstrumentationService PageInstrumentation {
            get {
                return _context.PageInstrumentation;
            }
        }

        public override IHttpHandler PreviousHandler {
            get {
                return _context.PreviousHandler;
            }
        }

        //
        public override ProfileBase Profile {
            get {
                return _context.Profile;
            }
        }

        public override HttpRequestBase Request {
            get {
                return new HttpRequestWrapper(_context.Request);
            }
        }

        public override HttpResponseBase Response {
            get {
                return new HttpResponseWrapper(_context.Response);
            }
        }

        public override HttpServerUtilityBase Server {
            get {
                return new HttpServerUtilityWrapper(_context.Server);
            }
        }

        public override HttpSessionStateBase Session {
            get {
                HttpSessionState session = _context.Session;
                return (session != null) ? new HttpSessionStateWrapper(session) : null;
            }
        }

        public override bool SkipAuthorization {
            get {
                return _context.SkipAuthorization;
            }
            set {
                _context.SkipAuthorization = value;
            }
        }

        public override DateTime Timestamp {
            get {
                return _context.Timestamp;
            }
        }

        public override bool ThreadAbortOnTimeout {
            get {
                return _context.ThreadAbortOnTimeout;
            }
            set {
                _context.ThreadAbortOnTimeout = value;
            }
        }

        //
        public override TraceContext Trace {
            get {
                return _context.Trace;
            }
        }

        public override IPrincipal User {
            get {
                return _context.User;
            }
            set {
                _context.User = value;
            }
        }

        public override string WebSocketNegotiatedProtocol {
            get {
                return _context.WebSocketNegotiatedProtocol;
            }
        }

        public override IList<string> WebSocketRequestedProtocols {
            get {
                return _context.WebSocketRequestedProtocols;
            }
        }

        public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc) {
            _context.AcceptWebSocketRequest(userFunc);
        }

        public override void AcceptWebSocketRequest(Func<AspNetWebSocketContext, Task> userFunc, AspNetWebSocketOptions options) {
            _context.AcceptWebSocketRequest(userFunc, options);
        }    

        public override void AddError(Exception errorInfo) {
            _context.AddError(errorInfo);
        }

        public override void ClearError() {
            _context.ClearError();
        }

        public override ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target) {
            return _context.DisposeOnPipelineCompleted(target);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo",
            Justification = "Matches HttpContext class")]
        public override object GetGlobalResourceObject(string classKey, string resourceKey) {
            return HttpContext.GetGlobalResourceObject(classKey, resourceKey);
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture) {
            return HttpContext.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo",
            Justification = "Matches HttpContext class")]
        public override object GetLocalResourceObject(string virtualPath, string resourceKey) {
            return HttpContext.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture) {
            return HttpContext.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName) {
            return _context.GetSection(sectionName);
        }

        public override void RemapHandler(IHttpHandler handler) {
            _context.RemapHandler(handler);
        }

        public override void RewritePath(string path) {
            _context.RewritePath(path);
        }

        public override void RewritePath(string path, bool rebaseClientPath) {
            _context.RewritePath(path, rebaseClientPath);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString) {
            _context.RewritePath(filePath, pathInfo, queryString);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) {
            _context.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        public override void SetSessionStateBehavior(SessionStateBehavior sessionStateBehavior) {
            _context.SetSessionStateBehavior(sessionStateBehavior);
        }

        public override object GetService(Type serviceType) {
            return ((IServiceProvider)_context).GetService(serviceType);
        }

        internal static Action<HttpContext> WrapCallback(Action<HttpContextBase> callback) {
            if (callback != null) {
                return context => callback(new HttpContextWrapper(context));
            }
            else {
                return null;
            }
        }
    }
}
