//------------------------------------------------------------------------------
// <copyright file="HandlerFactoryWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Config related classes for HttpApplication
 */

namespace System.Web.Configuration {

    using System;
    using System.Web.Util;

    /*
     * Single instance handler factory
     */
    internal class HandlerFactoryWrapper : IHttpHandlerFactory {
        private IHttpHandler _handler;
        private Type _handlerType;

        internal HandlerFactoryWrapper(IHttpHandler handler, Type handlerType) {
            _handler = handler;
            _handlerType = handlerType;
        }

        public IHttpHandler GetHandler(HttpContext context, String requestType, String url, String pathTranslated) {
            if (_handler == null)
                _handler = (IHttpHandler)HttpRuntime.CreateNonPublicInstance(_handlerType);

            return _handler;
        }

        public void ReleaseHandler(IHttpHandler handler) {
            if (_handler != null) {
                Debug.Assert(handler == _handler);

                if (!_handler.IsReusable) {
                    _handler = null;
                }
            }
        }
    }
}
