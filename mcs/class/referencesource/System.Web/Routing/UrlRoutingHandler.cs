namespace System.Web.Routing {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class UrlRoutingHandler : IHttpHandler {

        private RouteCollection _routeCollection;

        protected virtual bool IsReusable {
            get {
                return false;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This needs to be settable for unit tests.")]
        public RouteCollection RouteCollection {
            get {
                if (_routeCollection == null) {
                    _routeCollection = RouteTable.Routes;
                }
                return _routeCollection;
            }
            set {
                _routeCollection = value;
            }
        }

        protected virtual void ProcessRequest(HttpContext httpContext) {
            ProcessRequest(new HttpContextWrapper(httpContext));
        }

        protected virtual void ProcessRequest(HttpContextBase httpContext) {
            RouteData routeData = RouteCollection.GetRouteData(httpContext);
            if (routeData == null) {
                throw new HttpException(404, SR.GetString(SR.UrlRoutingHandler_NoRouteMatches));
            }

            IRouteHandler routeHandler = routeData.RouteHandler;
            if (routeHandler == null) {
                throw new InvalidOperationException(SR.GetString(SR.UrlRoutingModule_NoRouteHandler));
            }

            RequestContext requestContext = new RequestContext(httpContext, routeData);
            IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
            if (httpHandler == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        SR.GetString(SR.UrlRoutingModule_NoHttpHandler),
                        routeHandler.GetType()));
            }

            VerifyAndProcessRequest(httpHandler, httpContext);
        }

        protected abstract void VerifyAndProcessRequest(IHttpHandler httpHandler, HttpContextBase httpContext);

        #region IHttpHandler Members
        bool IHttpHandler.IsReusable {
            get {
                return IsReusable;
            }
        }

        void IHttpHandler.ProcessRequest(HttpContext context) {
            ProcessRequest(context);
        }
        #endregion
    }
}
