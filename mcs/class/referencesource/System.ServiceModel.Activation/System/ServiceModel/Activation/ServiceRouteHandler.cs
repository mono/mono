//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Routing;

    class ServiceRouteHandler : IRouteHandler
    {
        string baseAddress;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile IHttpHandler handler;
        object locker = new object();
        [Fx.Tag.Cache(
                typeof(ServiceDeploymentInfo),
                Fx.Tag.CacheAttrition.None,
                Scope = "instance of declaring class",
                SizeLimit = "unbounded",
                Timeout = "infinite"
                )]
        static Hashtable routeServiceTable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

        public ServiceRouteHandler(string baseAddress, ServiceHostFactoryBase serviceHostFactory, Type webServiceType)
        {
            this.baseAddress = string.Format(CultureInfo.CurrentCulture, "~/{0}", baseAddress);
            if (webServiceType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("webServiceType"));
            }
            if (serviceHostFactory == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("serviceHostFactory"));
            }
            string serviceType = webServiceType.AssemblyQualifiedName;

            AddServiceInfo(this.baseAddress, new ServiceDeploymentInfo(this.baseAddress, serviceHostFactory, serviceType));
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // we create httphandler only we the request map to the corresponding route.
            // we thus do not need to check whether the baseAddress has been added  
            // even though Asp.Net allows duplicated routes but it picks the first match 
            if (handler == null)
            {
                // use local lock to prevent multiple httphanders from being created
                lock (locker)
                {
                    if (handler == null)
                    {
                        IHttpHandler tempHandler = new AspNetRouteServiceHttpHandler(this.baseAddress);
                        MarkRouteAsActive(this.baseAddress);
                        handler = tempHandler;
                    }
                }
            }
            return handler;
        }

        static void AddServiceInfo(string virtualPath, ServiceDeploymentInfo serviceInfo)
        {
            Fx.Assert(!string.IsNullOrEmpty(virtualPath), "virtualPath should not be empty or null");
            Fx.Assert(serviceInfo != null, "serviceInfo should not be null");
            // We cannot support dulicated route routes even Asp.Net route allows it
            try
            {
                routeServiceTable.Add(virtualPath, serviceInfo);
            }
            catch (ArgumentException)
            {
                throw FxTrace.Exception.Argument("virtualPath", SR.Hosting_RouteHasAlreadyBeenAdded(virtualPath));
            }
        }

        public static ServiceDeploymentInfo GetServiceInfo(string normalizedVirtualPath)
        {
            return (ServiceDeploymentInfo)routeServiceTable[normalizedVirtualPath];
        }

        public static bool IsActiveAspNetRoute(string virtualPath)
        {
            bool isRouteService = false;
            if (!string.IsNullOrEmpty(virtualPath))
            {
                ServiceDeploymentInfo serviceInfo = (ServiceDeploymentInfo)routeServiceTable[virtualPath];
                if (serviceInfo != null)
                {
                    isRouteService = serviceInfo.MessageHandledByRoute;
                }
            }
            return isRouteService;
        }       

        // A route in routetable does not always mean the route will be picked
        // we update IsRouteService only when Asp.Net picks this route
        public static void MarkRouteAsActive(string normalizedVirtualPath)
        {
            ServiceDeploymentInfo serviceInfo = (ServiceDeploymentInfo)routeServiceTable[normalizedVirtualPath];
            if (serviceInfo != null)
            {
                serviceInfo.MessageHandledByRoute = true;
            }
        }
        // a route should be marked as inactive in the case that CBA should be used
        public static void MarkARouteAsInactive(string normalizedVirtualPath)
        {
            ServiceDeploymentInfo serviceInfo = (ServiceDeploymentInfo)routeServiceTable[normalizedVirtualPath];
            if (serviceInfo != null)
            {
                serviceInfo.MessageHandledByRoute = false;
            }
        }
    }
}
