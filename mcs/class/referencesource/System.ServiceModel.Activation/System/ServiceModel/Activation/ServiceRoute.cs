//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.Globalization;
    using System.Web.Routing;

    public class ServiceRoute : Route
    {
        internal const string LeftCurlyBracket = "{";
        internal const string RightCurlyBracket = "}";
        internal const string UnmatchedPathSegment = "{*pathInfo}";
        internal const char PathSeperator = '/';

        public ServiceRoute(string routePrefix, ServiceHostFactoryBase serviceHostFactory, Type serviceType)
            : base(CheckAndCreateRouteString(routePrefix), new ServiceRouteHandler(routePrefix, serviceHostFactory, serviceType))
        {
            if (TD.AspNetRouteIsEnabled())
            {
                TD.AspNetRoute(routePrefix, serviceType.AssemblyQualifiedName, serviceHostFactory.GetType().AssemblyQualifiedName);
            }
        }

        static string CheckAndCreateRouteString(string routePrefix)
        {
            // aspnet routing integration is supported only under aspnet compat mode
            ServiceHostingEnvironment.EnsureInitialized();
            if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Hosting_RouteServiceRequiresCompatibilityMode));
            }
            // we support emptystring as routeprfix as aspnet allows it
            if (routePrefix == null)
            {
                throw FxTrace.Exception.ArgumentNull("routePrefix");
            }
            else if (routePrefix.Contains(LeftCurlyBracket) || routePrefix.Contains(RightCurlyBracket))
            {
                throw FxTrace.Exception.Argument("routePrefix", SR.Hosting_CurlyBracketFoundInRoutePrefix("{", "}"));
            }

            if (routePrefix.EndsWith(PathSeperator.ToString(), StringComparison.CurrentCultureIgnoreCase)
                || routePrefix.Equals(String.Empty, StringComparison.CurrentCultureIgnoreCase))
            {
                routePrefix = string.Format(CultureInfo.CurrentCulture, "{0}{1}", routePrefix, UnmatchedPathSegment);
            }
            else
            {
                routePrefix = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", routePrefix, UnmatchedPathSegment);
            }
            return routePrefix;
        }
    }
}
