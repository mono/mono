//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xaml.Hosting
{
    using System;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Compilation;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Xaml.Hosting.Configuration;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading;
    using System.Net;
    using System.Runtime;
    using System.Security;
    using System.Collections;

    [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUninstantiatedInternalClasses,
        Justification = "This is instantiated by AspNet.")]
    sealed class XamlHttpHandlerFactory : IHttpHandlerFactory
    {
        public IHttpHandler GetHandler(HttpContext context, string requestType,
            string url, string pathTranslated)
        {
            //Get the "cache pointer" for the virtual path - if does not exist, create a cache pointer
            //This should happen under global lock
            PathInfo pathInfo = PathCache.EnsurePathInfo(context.Request.AppRelativeCurrentExecutionFilePath);
            return pathInfo.GetHandler(context, requestType, url, pathTranslated);
        }

        public void ReleaseHandler(IHttpHandler httphandler)
        {
            //Check whether the handler was created by an internal factory
            if (httphandler is HandlerWrapper)
            {
                ((HandlerWrapper)(httphandler)).ReleaseWrappedHandler();
            }
        }

        static object CreateInstance(Type type)
        {
            //The handler/factory should have an empty constructor but need not be public
            return Activator.CreateInstance(type,
                BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, null, null);
        }
                
        static class PathCache
        {
            [Fx.Tag.Cache(
                typeof(PathInfo),
                Fx.Tag.CacheAttrition.None,
                Scope = "instance of declaring class", 
                SizeLimit = "unbounded",
                Timeout = "infinite"
                )] 
            static Hashtable pathCache = new Hashtable(StringComparer.OrdinalIgnoreCase);
            static object writeLock = new object();

            public static PathInfo EnsurePathInfo(string path)
            {
                PathInfo pathInfo = (PathInfo)pathCache[path];
                if (pathInfo != null)
                {
                    return pathInfo;
                }

                lock (writeLock)
                {
                    pathInfo = (PathInfo)pathCache[path];
                    if (pathInfo != null)
                    {
                        return pathInfo;
                    }

                    if (HostingEnvironment.VirtualPathProvider.FileExists(path))
                    {
                        pathInfo = new PathInfo();
                        pathCache.Add(path, pathInfo);
                        return pathInfo;
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new HttpException((int)HttpStatusCode.NotFound, SR.ResourceNotFound));
                    }
                }
            }
        }

        class HandlerWrapper : IHttpHandler
        {
            IHttpHandlerFactory factory;

            IHttpHandler httpHandler;

            private HandlerWrapper(IHttpHandler httpHandler, IHttpHandlerFactory factory)
            {
                this.httpHandler = httpHandler;
                this.factory = factory;
            }

            public bool IsReusable
            {
                get { return httpHandler.IsReusable; }
            }

            public static IHttpHandler Create(
                IHttpHandler httpHandler, IHttpHandlerFactory factory)
            {
                if (httpHandler is IHttpAsyncHandler)
                {
                    return new AsyncHandlerWrapper((IHttpAsyncHandler)httpHandler, factory);
                }
                else
                {
                    return new HandlerWrapper(httpHandler, factory);
                }
            }

            public void ProcessRequest(HttpContext context)
            {
                httpHandler.ProcessRequest(context);
            }

            public void ReleaseWrappedHandler()
            {
                this.factory.ReleaseHandler(httpHandler);
            }

            class AsyncHandlerWrapper : HandlerWrapper, IHttpAsyncHandler
            {
                //Storing a local copy to avoid unnecessary typecasts during begin/end
                IHttpAsyncHandler httpAsyncHandler;

                public AsyncHandlerWrapper(IHttpAsyncHandler httpAsyncHandler, IHttpHandlerFactory factory)
                    : base(httpAsyncHandler, factory)
                {
                    this.httpAsyncHandler = httpAsyncHandler;
                }

                public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
                {
                    return this.httpAsyncHandler.BeginProcessRequest(context, cb, extraData);
                }

                public void EndProcessRequest(IAsyncResult result)
                {
                    this.httpAsyncHandler.EndProcessRequest(result);
                }
            }
        }

        class PathInfo
        {
            object cachedResult;
            Type hostedXamlType;
            object writeLock;

            public PathInfo()
            {
                this.writeLock = new object();
            }

            [Fx.Tag.Throws(typeof(ConfigurationErrorsException), "Invalid Configuration.")]
            public IHttpHandler GetHandler(HttpContext context, string requestType,
                string url, string pathTranslated)
            {
                if (this.cachedResult == null)
                {
                    //Cache won't be available if it is invoked first time 
                    //Use a local "lock" specifically for this url 
                    lock (this.writeLock)
                    {
                        if (this.cachedResult == null)
                        {
                            return GetHandlerFirstTime(context, requestType, url, pathTranslated);
                        }
                    }
                }

                return GetHandlerSubSequent(context, requestType, url, pathTranslated);
            }

            [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeImpersonate to establish the impersonation context",
                Safe = "Does not leak anything, does not let caller influence impersonation.")]
            // Why this triple try blocks instead of using "using" statement:
            // 1. "using" will do the impersonation prior to entering the try, 
            //    which leaves an opertunity to Thread.Abort this thread and get it to exit the method still impersonated.
            // 2. put the assignment of unsafeImpersonate in a finally block 
            //    in order to prevent Threat.Abort after impersonation but before the assignment.
            // 3. the finally of a "using" doesn't run until exception filters higher up the stack have executed.
            //    they will do so in the impersonated context if an exception is thrown inside the try.
            // In sumary, this should prevent the thread from existing this method well still impersonated. 
            Type GetCompiledCustomString(string normalizedVirtualPath)
            {
                try
                {
                    IDisposable unsafeImpersonate = null;
                    try
                    {
                        try
                        {
                        }
                        finally
                        {
                            unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                        }
                        return BuildManager.GetCompiledType(normalizedVirtualPath);
                    }
                    finally
                    {
                        if (null != unsafeImpersonate)
                        {
                            unsafeImpersonate.Dispose();
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }


            //This function is invoked the first time a request is made to XAMLx file 
            //It caches url as key and one of the 
            //following 4 as value -> "Handler/Factory/HandlerCLRType/Exception"
            IHttpHandler GetHandlerFirstTime(HttpContext context, string requestType,
                string url, string pathTranslated)
            {
                Type httpHandlerType;
                ConfigurationErrorsException configException;

                //GetCompiledType is costly - invoke it just once. 
                //This null check is required for "error after GetCompiledType on first attempt" cases only
                if (this.hostedXamlType == null)
                {
                    this.hostedXamlType = GetCompiledCustomString(context.Request.AppRelativeCurrentExecutionFilePath);
                }

                if (XamlHostingConfiguration.TryGetHttpHandlerType(url, this.hostedXamlType, out httpHandlerType))
                {
                    if (TD.HttpHandlerPickedForUrlIsEnabled())
                    {
                        TD.HttpHandlerPickedForUrl(url, hostedXamlType.FullName, httpHandlerType.FullName);
                    }
                    if (typeof(IHttpHandler).IsAssignableFrom(httpHandlerType))
                    {
                        IHttpHandler handler = (IHttpHandler)CreateInstance(httpHandlerType);
                        if (handler.IsReusable)
                        {
                            this.cachedResult = handler;
                        }
                        else
                        {
                            this.cachedResult = httpHandlerType;
                        }
                        return handler;
                    }
                    else if (typeof(IHttpHandlerFactory).IsAssignableFrom(httpHandlerType))
                    {
                        IHttpHandlerFactory factory = (IHttpHandlerFactory)CreateInstance(httpHandlerType);
                        this.cachedResult = factory;
                        IHttpHandler handler = factory.GetHandler(context, requestType, url, pathTranslated);
                        return HandlerWrapper.Create(handler, factory);
                    }
                    else
                    {
                        configException =
                            new ConfigurationErrorsException(SR.NotHttpHandlerType(url, this.hostedXamlType, httpHandlerType.FullName));
                        this.cachedResult = configException;
                        throw FxTrace.Exception.AsError(configException);
                    }
                }
                configException =
                    new ConfigurationErrorsException(SR.HttpHandlerForXamlTypeNotFound(url, this.hostedXamlType, XamlHostingConfiguration.XamlHostingSection));
                this.cachedResult = configException;
                throw FxTrace.Exception.AsError(configException);
            }

            //This function retrievs the cached object and uses it to get handler or exception
            IHttpHandler GetHandlerSubSequent(HttpContext context, string requestType,
                string url, string pathTranslated)
            {
                if (this.cachedResult is IHttpHandler)
                {
                    return ((IHttpHandler)this.cachedResult);
                }
                else if (this.cachedResult is IHttpHandlerFactory)
                {
                    IHttpHandlerFactory factory = ((IHttpHandlerFactory)this.cachedResult);
                    IHttpHandler handler = factory.GetHandler(context, requestType, url, pathTranslated);
                    return HandlerWrapper.Create(handler, factory);
                }
                else if (this.cachedResult is Type)
                {
                    return (IHttpHandler)CreateInstance((Type)this.cachedResult);
                }
                else
                {
                    throw FxTrace.Exception.AsError((ConfigurationErrorsException)this.cachedResult);
                }
            }

        }
    }
}

