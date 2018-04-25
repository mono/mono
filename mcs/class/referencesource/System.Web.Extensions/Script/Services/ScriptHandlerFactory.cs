//------------------------------------------------------------------------------
// <copyright file="ScriptHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.SessionState;
using System.Security.Permissions;
using System.Security;

namespace System.Web.Script.Services {
    internal class ScriptHandlerFactory : IHttpHandlerFactory {
        IHttpHandlerFactory _restHandlerFactory;
        IHttpHandlerFactory _webServiceHandlerFactory;

        internal class HandlerWrapper : IHttpHandler {
            protected IHttpHandler _originalHandler;
            private IHttpHandlerFactory _originalFactory;

            internal HandlerWrapper(IHttpHandler originalHandler, IHttpHandlerFactory originalFactory) {
                _originalFactory = originalFactory;
                _originalHandler = originalHandler;
            }

            internal void ReleaseHandler() {
                _originalFactory.ReleaseHandler(_originalHandler);
            }

            public bool IsReusable {
                get {
                    return _originalHandler.IsReusable;
                }
            }

            public void ProcessRequest(HttpContext context) {
                _originalHandler.ProcessRequest(context);
            }
        }

        internal class HandlerWrapperWithSession : HandlerWrapper, IRequiresSessionState {
            internal HandlerWrapperWithSession(IHttpHandler originalHandler, IHttpHandlerFactory originalFactory)
                : base(originalHandler, originalFactory) {}
        }

        private class AsyncHandlerWrapper : HandlerWrapper, IHttpAsyncHandler {
            internal AsyncHandlerWrapper(IHttpHandler originalHandler, IHttpHandlerFactory originalFactory)
                :
                base(originalHandler, originalFactory) { }

            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
                return ((IHttpAsyncHandler)_originalHandler).BeginProcessRequest(context, cb, extraData);
            }

            public void EndProcessRequest(IAsyncResult result) {
                ((IHttpAsyncHandler)_originalHandler).EndProcessRequest(result);
            }
        }

        private class AsyncHandlerWrapperWithSession : AsyncHandlerWrapper, IRequiresSessionState {
            internal AsyncHandlerWrapperWithSession(IHttpHandler originalHandler, IHttpHandlerFactory originalFactory)
                : base(originalHandler, originalFactory) { }
        }

        public ScriptHandlerFactory() {
            _restHandlerFactory = new RestHandlerFactory();
            _webServiceHandlerFactory = new System.Web.Services.Protocols.WebServiceHandlerFactory();
        }

        [SecuritySafeCritical]
        public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated) {
            IHttpHandler handler;
            IHttpHandlerFactory factory;
            if (RestHandlerFactory.IsRestRequest(context)) {
                // It's a REST request
                factory = _restHandlerFactory;
            }
            else {
                // It's a regular asmx web request, so delegate to the WebServiceHandlerFactory
                factory = _webServiceHandlerFactory;
            }

            handler = factory.GetHandler(context, requestType, url, pathTranslated);

            bool requiresSession = handler is IRequiresSessionState;

            if (handler is IHttpAsyncHandler) {
                if (requiresSession)
                    return new AsyncHandlerWrapperWithSession(handler, factory);
                else
                    return new AsyncHandlerWrapper(handler, factory);
            }

            if (requiresSession)
                return new HandlerWrapperWithSession(handler, factory);
            else
                return new HandlerWrapper(handler, factory);
        }

        public virtual void ReleaseHandler(IHttpHandler handler) {
            if (handler == null) {
                throw new ArgumentNullException("handler");
            }
            ((HandlerWrapper)handler).ReleaseHandler();
        }
    }
}
