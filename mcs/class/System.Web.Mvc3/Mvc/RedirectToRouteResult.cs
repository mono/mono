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

        public RedirectToRouteResult(string routeName, RouteValueDictionary routeValues)
            : this(routeName, routeValues, permanent: false) {
        }

        public RedirectToRouteResult(string routeName, RouteValueDictionary routeValues, bool permanent) {
            Permanent = permanent;
            RouteName = routeName ?? String.Empty;
            RouteValues = routeValues ?? new RouteValueDictionary();
        }

        public bool Permanent {
            get;
            private set;
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
            if (context.IsChildAction) {
                throw new InvalidOperationException(MvcResources.RedirectAction_CannotRedirectInChildAction);
            }

            string destinationUrl = UrlHelper.GenerateUrl(RouteName, null /* actionName */, null /* controllerName */, RouteValues, Routes, context.RequestContext, false /* includeImplicitMvcValues */);
            if (String.IsNullOrEmpty(destinationUrl)) {
                throw new InvalidOperationException(MvcResources.Common_NoRouteMatched);
            }

            context.Controller.TempData.Keep();

            if (Permanent) {
                context.HttpContext.Response.RedirectPermanent(destinationUrl, endResponse: false);
            }
            else {
                context.HttpContext.Response.Redirect(destinationUrl, endResponse: false);
            }
        }

    }
}
