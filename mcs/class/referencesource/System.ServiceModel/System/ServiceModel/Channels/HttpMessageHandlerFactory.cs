// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Configuration;

    /// <summary>
    /// Default HTTP message handler factory used by <see cref="HttpChannelListener"/> upon creation of an <see cref="HttpMessageHandler"/> 
    /// for instantiating a set of HTTP message handler types using their default constructors.
    /// For more complex initialization scenarios, derive from <see cref="HttpMessageHandlerFactory"/>
    /// and override the <see cref="HttpMessageHandlerFactory.OnCreate"/> method.
    /// </summary>
    public class HttpMessageHandlerFactory
    {
        static readonly Type delegatingHandlerType = typeof(DelegatingHandler);

        Type[] httpMessageHandlers;
        ConstructorInfo[] handlerCtors;
        Func<IEnumerable<DelegatingHandler>> handlerFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageHandlerFactory"/> class given
        /// a set of HTTP message handler types to instantiate using their default constructors.
        /// </summary>
        /// <param name="handlers">An ordered list of HTTP message handler types to be invoked as part of an 
        /// <see cref="HttpMessageHandler"/> instance.
        /// HTTP message handler types must derive from <see cref="DelegatingHandler"/> and have a public constructor
        /// taking exactly one argument of type <see cref="HttpMessageHandler"/>. The handlers are invoked in a 
        /// bottom-up fashion in the incoming path and top-down in the outgoing path. That is, the last entry is called first 
        /// for an incoming request messasge but invoked last for an outgoing response message.</param>
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public HttpMessageHandlerFactory(params Type[] handlers)
        {
            if (handlers == null)
            {
                throw FxTrace.Exception.ArgumentNull("handlers");
            }

            if (handlers.Length == 0)
            {
                throw FxTrace.Exception.Argument("handlers", SR.GetString(SR.InputTypeListEmptyError));
            }

            this.handlerCtors = new ConstructorInfo[handlers.Length];
            for (int cnt = 0; cnt < handlers.Length; cnt++)
            {
                Type handler = handlers[cnt];
                if (handler == null)
                {
                    throw FxTrace.Exception.Argument(
                        string.Format(CultureInfo.InvariantCulture, "handlers[<<{0}>>]", cnt),
                        SR.GetString(SR.HttpMessageHandlerTypeNotSupported, "null", delegatingHandlerType.Name));
                }

                if (!delegatingHandlerType.IsAssignableFrom(handler) || handler.IsAbstract)
                {
                    throw FxTrace.Exception.Argument(
                        string.Format(CultureInfo.InvariantCulture, "handlers[<<{0}>>]", cnt),
                        SR.GetString(SR.HttpMessageHandlerTypeNotSupported, handler.Name, delegatingHandlerType.Name));
                }

                ConstructorInfo ctorInfo = handler.GetConstructor(Type.EmptyTypes);
                if (ctorInfo == null)
                {
                    throw FxTrace.Exception.Argument(
                        string.Format(CultureInfo.InvariantCulture, "handlers[<<{0}>>]", cnt),
                        SR.GetString(SR.HttpMessageHandlerTypeNotSupported, handler.Name, delegatingHandlerType.Name));
                }

                this.handlerCtors[cnt] = ctorInfo;
            }

            this.httpMessageHandlers = handlers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageHandlerFactory"/> class given
        /// a function to create a set of <see cref="DelegatingHandler"/> instances.
        /// </summary>
        /// <param name="handlers">A function to generate an ordered list of <see cref="DelegatingHandler"/> instances 
        /// to be invoked as part of an <see cref="HttpMessageHandler"/> instance.
        /// The handlers are invoked in a bottom-up fashion in the incoming path and top-down in the outgoing path. That is, 
        /// the last entry is called first for an incoming request messasge but invoked last for an outgoing response message.</param>
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public HttpMessageHandlerFactory(Func<IEnumerable<DelegatingHandler>> handlers)
        {
            if (handlers == null)
            {
                throw FxTrace.Exception.ArgumentNull("handlers");
            }

            this.handlerFunc = handlers;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageHandlerFactory"/> class.
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected HttpMessageHandlerFactory()
        {
        }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the HTTP message handlers
        /// provided in the constructor.
        /// </summary>
        /// <param name="innerChannel">The inner channel represents the destination of the HTTP message channel.</param>
        /// <returns>The HTTP message channel.</returns>
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public HttpMessageHandler Create(HttpMessageHandler innerChannel)
        {
            if (innerChannel == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerChannel");
            }

            return this.OnCreate(innerChannel);
        }

        internal static HttpMessageHandlerFactory CreateFromConfigurationElement(HttpMessageHandlerFactoryElement configElement)
        {
            Fx.Assert(configElement != null, "configElement should not be null.");

            if (!string.IsNullOrWhiteSpace(configElement.Type))
            {
                if (configElement.Handlers != null && configElement.Handlers.Count > 0)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.GetString(SR.HttpMessageHandlerFactoryConfigInvalid_WithBothTypeAndHandlerList, ConfigurationStrings.MessageHandlerFactory, ConfigurationStrings.Type, ConfigurationStrings.Handlers)));
                }

                Type factoryType = HttpChannelUtilities.GetTypeFromAssembliesInCurrentDomain(configElement.Type);
                if (factoryType == null)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.GetString(SR.CanNotLoadTypeGotFromConfig, configElement.Type)));
                }

                if (!typeof(HttpMessageHandlerFactory).IsAssignableFrom(factoryType) || factoryType.IsAbstract)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(
                        SR.WebSocketElementConfigInvalidHttpMessageHandlerFactoryType,
                        typeof(HttpMessageHandlerFactory).Name,
                        factoryType,
                        typeof(HttpMessageHandlerFactory).AssemblyQualifiedName)));
                }

                return Activator.CreateInstance(factoryType) as HttpMessageHandlerFactory;
            }
            else
            {
                if (configElement.Handlers == null || configElement.Handlers.Count == 0)
                {
                    return null;
                }

                Type[] handlerList = new Type[configElement.Handlers.Count];
                for (int i = 0; i < configElement.Handlers.Count; i++)
                {
                    Type handlerType = HttpChannelUtilities.GetTypeFromAssembliesInCurrentDomain(configElement.Handlers[i].Type);
                    if (handlerType == null)
                    {
                        throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.GetString(SR.CanNotLoadTypeGotFromConfig, configElement.Handlers[i].Type)));
                    }

                    handlerList[i] = handlerType;
                }

                try
                {
                    return new HttpMessageHandlerFactory(handlerList);
                }
                catch (ArgumentException ex)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(ex.Message, ex));
                }
            }
        }

        internal HttpMessageHandlerFactoryElement GenerateConfigurationElement()
        {
            if (this.handlerFunc != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpMessageHandlerFactoryWithFuncCannotGenerateConfig, typeof(HttpMessageHandlerFactory).Name, typeof(Func<IEnumerable<DelegatingHandler>>).Name)));
            }

            Type thisType = this.GetType();
            if (thisType != typeof(HttpMessageHandlerFactory))
            {
                return new HttpMessageHandlerFactoryElement
                {
                    Type = thisType.AssemblyQualifiedName
                };
            }
            else
            {
                if (this.httpMessageHandlers != null)
                {
                    DelegatingHandlerElementCollection handlerCollection = new DelegatingHandlerElementCollection();
                    for (int i = 0; i < this.httpMessageHandlers.Length; i++)
                    {
                        handlerCollection.Add(new DelegatingHandlerElement(this.httpMessageHandlers[i]));
                    }

                    return new HttpMessageHandlerFactoryElement
                    {
                        Handlers = handlerCollection
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the HTTP message handlers
        /// provided in the constructor.
        /// </summary>
        /// <param name="innerChannel">The inner channel represents the destination of the HTTP message channel.</param>
        /// <returns>The HTTP message channel.</returns>
        protected virtual HttpMessageHandler OnCreate(HttpMessageHandler innerChannel)
        {
            if (innerChannel == null)
            {
                throw FxTrace.Exception.ArgumentNull("innerChannel");
            }

            // Get handlers either by constructing types or by calling Func
            IEnumerable<DelegatingHandler> handlerInstances = null;
            try
            {
                if (this.handlerFunc != null)
                {
                    handlerInstances = this.handlerFunc.Invoke();
                    if (handlerInstances != null)
                    {
                        foreach (DelegatingHandler handler in handlerInstances)
                        {
                            if (handler == null)
                            {
                                throw FxTrace.Exception.Argument("handlers", SR.GetString(SR.DelegatingHandlerArrayFromFuncContainsNullItem, delegatingHandlerType.Name, GetFuncDetails(this.handlerFunc)));
                            }
                        }
                    }
                }
                else if (this.handlerCtors != null)
                {
                    DelegatingHandler[] instances = new DelegatingHandler[this.handlerCtors.Length];
                    for (int cnt = 0; cnt < this.handlerCtors.Length; cnt++)
                    {
                        instances[cnt] = (DelegatingHandler)this.handlerCtors[cnt].Invoke(Type.EmptyTypes);
                    }

                    handlerInstances = instances;
                }
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw FxTrace.Exception.AsError(targetInvocationException);
            }

            // Wire handlers up
            HttpMessageHandler pipeline = innerChannel;
            if (handlerInstances != null)
            {
                foreach (DelegatingHandler handler in handlerInstances)
                {
                    if (handler.InnerHandler != null)
                    {
                        throw FxTrace.Exception.Argument("handlers", SR.GetString(SR.DelegatingHandlerArrayHasNonNullInnerHandler, delegatingHandlerType.Name, "InnerHandler", handler.GetType().Name));
                    }

                    handler.InnerHandler = pipeline;
                    pipeline = handler;
                }
            }

            return pipeline;
        }

        static string GetFuncDetails(Func<IEnumerable<DelegatingHandler>> func)
        {
            Fx.Assert(func != null, "Func should not be null.");
            MethodInfo m = func.Method;
            Type t = m.DeclaringType;
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", t.FullName, m.Name);
        }
    }
}
