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
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    // represents a result that performs a redirection given some values dictionary
    public class RedirectToRouteResult : ActionResult {

        private RouteCollection _routes;

        public RedirectToRouteResult(RouteValueDictionary routeValues) :
            this(null, routeValues) {
        }

        public RedirectToRouteResult(string routeName, RouteValueDictionary routeValues) {
            RouteName = routeName ?? String.Empty;
            RouteValues = routeValues ?? new RouteValueDictionary();
        }

        public string RouteName {
            get;
            private set;
        }

        public RouteValueDictionary RouteValues {
            get;
            private set;
        }

        internal RouteCollection Routes {
            get {
                if (_routes == null) {
                    _routes = RouteTable.Routes;
                }
                return _routes;
            }
            set {
                _routes = value;
            }
        }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            string destinationUrl = UrlHelper.GenerateUrl(RouteName, null /* actionName */, null /* controllerName */, RouteValues, Routes, context.RequestContext, false /* includeImplicitMvcValues */);
            if (String.IsNullOrEmpty(destinationUrl)) {
                throw new InvalidOperationException(MvcResources.ActionRedirectResult_NoRouteMatched);
            }

            context.HttpContext.Response.Redirect(destinationUrl, false /* endResponse */);
        }
    }
}
