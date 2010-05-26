/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Web;
    using System.Web.Mvc.Async;
    using System.Web.Routing;
    using System.Web.SessionState;

    public class MvcHttpHandler : UrlRoutingHandler, IHttpAsyncHandler, IRequiresSessionState {

        private static readonly object _processRequestTag = new object();

        protected virtual IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state) {
            HttpContextBase iHttpContext = new HttpContextWrapper(httpContext);
            return BeginProcessRequest(iHttpContext, callback, state);
        }

        protected internal virtual IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object state) {
            IHttpHandler httpHandler = GetHttpHandler(httpContext);
            IHttpAsyncHandler httpAsyncHandler = httpHandler as IHttpAsyncHandler;

            if (httpAsyncHandler != null) {
                // asynchronous handler
                BeginInvokeDelegate beginDelegate = delegate(AsyncCallback asyncCallback, object asyncState) {
                    return httpAsyncHandler.BeginProcessRequest(HttpContext.Current, asyncCallback, asyncState);
                };
                EndInvokeDelegate endDelegate = delegate(IAsyncResult asyncResult) {
                    httpAsyncHandler.EndProcessRequest(asyncResult);
                };
                return AsyncResultWrapper.Begin(callback, state, beginDelegate, endDelegate, _processRequestTag);
            }
            else {
                // synchronous handler
                Action action = delegate {
                    httpHandler.ProcessRequest(HttpContext.Current);
                };
                return AsyncResultWrapper.BeginSynchronous(callback, state, action, _processRequestTag);
            }
        }

        protected internal virtual void EndProcessRequest(IAsyncResult asyncResult) {
            AsyncResultWrapper.End(asyncResult, _processRequestTag);
        }

        private static IHttpHandler GetHttpHandler(HttpContextBase httpContext) {
            DummyHttpHandler dummyHandler = new DummyHttpHandler();
            dummyHandler.PublicProcessRequest(httpContext);
            return dummyHandler.HttpHandler;
        }

        // synchronous code
        protected override void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext) {
            if (httpHandler == null) {
                throw new ArgumentNullException("httpHandler");
            }

            httpHandler.ProcessRequest(HttpContext.Current);
        }

        #region IHttpAsyncHandler Members
        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
            return BeginProcessRequest(context, cb, extraData);
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) {
            EndProcessRequest(result);
        }
        #endregion

        // Since UrlRoutingHandler.ProcessRequest() does the heavy lifting of looking at the RouteCollection for
        // a matching route, we need to call into it. However, that method is also responsible for kicking off
        // the synchronous request, and we can't allow it to do that. The purpose of this dummy class is to run
        // only the lookup portion of UrlRoutingHandler.ProcessRequest(), then intercept the handler it returns
        // and execute it asynchronously.

        private sealed class DummyHttpHandler : UrlRoutingHandler {
            public IHttpHandler HttpHandler;

            public void PublicProcessRequest(HttpContextBase httpContext) {
                ProcessRequest(httpContext);
            }

            protected override void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext) {
                // don't process the request, just store a reference to it
                HttpHandler = httpHandler;
            }
        }

    }
}
