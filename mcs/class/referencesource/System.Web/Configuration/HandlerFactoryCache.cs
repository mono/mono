//------------------------------------------------------------------------------
// <copyright file="HandlerFactoryCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Config related classes for HttpApplication
 */

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Web.Compilation;
    using System.Security;
    using System.Security.Permissions;

    /*
     * An object to cache a factory
     */
    internal class HandlerFactoryCache {
        private IHttpHandlerFactory _factory;

        internal HandlerFactoryCache(string type) {
            Object instance = Create(type);

            // make sure it is either handler or handler factory

            if (instance is IHttpHandler) {
                // create bogus factory around it
                _factory = new HandlerFactoryWrapper((IHttpHandler)instance, GetHandlerType(type));
            }
            else if (instance is IHttpHandlerFactory) {
                _factory = (IHttpHandlerFactory)instance;
            }
            else {
                throw new HttpException(SR.GetString(SR.Type_not_factory_or_handler, instance.GetType().FullName));
            }
        }

        internal HandlerFactoryCache(HttpHandlerAction mapping) {
            Object instance = mapping.Create();

            // make sure it is either handler or handler factory

            if (instance is IHttpHandler) {
                // create bogus factory around it
                _factory = new HandlerFactoryWrapper((IHttpHandler)instance, GetHandlerType(mapping));
            }
            else if (instance is IHttpHandlerFactory) {
                _factory = (IHttpHandlerFactory)instance;
            }
            else {
                throw new HttpException(SR.GetString(SR.Type_not_factory_or_handler, instance.GetType().FullName));
            }
        }

        internal IHttpHandlerFactory Factory {
            get {
                return _factory;
            }
        }

        // Dev10 732000: In a homogenous AppDomain, it is necessary to assert FileIoPermission to load types outside
        // the AppDomain's grant set.
        [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery)]
        private Type GetTypeWithAssert(string type) {
            return BuildManager.GetType(type, throwOnError: true, ignoreCase: false);
        }

        internal Type GetHandlerType( HttpHandlerAction handlerAction ) {
            // HACKHACK: for now, let uncreatable types through and error later (for .soap factory)
            // This design should change - developers will want to know immediately
            // when they misspell a type

            Type t = GetTypeWithAssert(handlerAction.Type);

            // throw for bad types in deferred case
            if (!ConfigUtil.IsTypeHandlerOrFactory(t))
                throw new ConfigurationErrorsException(SR.GetString(SR.Type_not_factory_or_handler, handlerAction.Type),
                    handlerAction.ElementInformation.Source, handlerAction.ElementInformation.LineNumber);

            return t;

        }

        internal Type GetHandlerType(string type) {
            // HACKHACK: for now, let uncreatable types through and error later (for .soap factory)
            // This design should change - developers will want to know immediately
            // when they misspell a type

            Type t = GetTypeWithAssert(type);

            HttpRuntime.FailIfNoAPTCABit(t, null, null);

            // throw for bad types in deferred case
            if (!ConfigUtil.IsTypeHandlerOrFactory(t))
                throw new ConfigurationErrorsException(SR.GetString(SR.Type_not_factory_or_handler, type));

            return t;

        }

        internal object Create(string type) {
            // HACKHACK: for now, let uncreatable types through and error later (for .soap factory)
            // This design should change - developers will want to know immediately
            // when they misspell a type

            return HttpRuntime.CreateNonPublicInstance(GetHandlerType(type));
        }
    }
}
