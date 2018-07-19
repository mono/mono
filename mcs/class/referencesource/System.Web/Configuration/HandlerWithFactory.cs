//------------------------------------------------------------------------------
// <copyright file="HandlerWithFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Config related classes for HttpApplication
 */

namespace System.Web.Configuration {

    using System;

    /*
     * Factory / handler pair for recycling
     */
    internal class HandlerWithFactory {
        private IHttpHandler _handler;
        private IHttpHandlerFactory _factory;

        internal HandlerWithFactory(IHttpHandler handler, IHttpHandlerFactory factory) {
            _handler = handler;
            _factory = factory;
        }

        internal void Recycle() {
            _factory.ReleaseHandler(_handler);
        }
    }
}
