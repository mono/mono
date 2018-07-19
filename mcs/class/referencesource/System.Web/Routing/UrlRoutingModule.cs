namespace System.Web.Routing {
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Web.Security;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UrlRoutingModule : IHttpModule {
        private static readonly object _contextKey = new Object();
        private static readonly object _requestDataKey = new Object();
        private RouteCollection _routeCollection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
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

        protected virtual void Dispose() {
        }

        protected virtual void Init(HttpApplication application) {

            //////////////////////////////////////////////////////////////////
            // Check if this module has been already addded
            if (application.Context.Items[_contextKey] != null) {
                return; // already added to the pipeline
            }
            application.Context.Items[_contextKey] = _contextKey;

            // Ideally we would use the MapRequestHandler event.  However, MapRequestHandler is not available
            // in II6 or IIS7 ISAPI Mode.  Instead, we use PostResolveRequestCache, which is the event immediately
            // before MapRequestHandler.  This allows use to use one common codepath for all versions of IIS.
            application.PostResolveRequestCache += OnApplicationPostResolveRequestCache;
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e) {
            HttpApplication app = (HttpApplication)sender;
            HttpContextBase context = new HttpContextWrapper(app.Context);
            PostResolveRequestCache(context);
        }

        [Obsolete("This method is obsolete. Override the Init method to use the PostMapRequestHandler event.")]
        public virtual void PostMapRequestHandler(HttpContextBase context) {
            // Backwards compat with 3.5 which used to have code here to Rewrite the URL
        }

        public virtual void PostResolveRequestCache(HttpContextBase context) {
            // Match the incoming URL against the route table
            RouteData routeData = RouteCollection.GetRouteData(context);

            // Do nothing if no route found
            if (routeData == null) {
                return;
            }

            // If a route was found, get an IHttpHandler from the route's RouteHandler
            IRouteHandler routeHandler = routeData.RouteHandler;
            if (routeHandler == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.UrlRoutingModule_NoRouteHandler)));
            }

            // This is a special IRouteHandler that tells the routing module to stop processing
            // routes and to let the fallback handler handle the request.
            if (routeHandler is StopRoutingHandler) {
                return;
            }

            RequestContext requestContext = new RequestContext(context, routeData);

            // Dev10 766875	Adding RouteData to HttpContext
            context.Request.RequestContext = requestContext;

            IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
            if (httpHandler == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        SR.GetString(SR.UrlRoutingModule_NoHttpHandler),
                        routeHandler.GetType()));
            }

            if (httpHandler is UrlAuthFailureHandler) {
                if (FormsAuthenticationModule.FormsAuthRequired) {
                    UrlAuthorizationModule.ReportUrlAuthorizationFailure(HttpContext.Current, this);
                    return;
                }
                else {
                    throw new HttpException(401, SR.GetString(SR.Assess_Denied_Description3));
                }
            }

            // Remap IIS7 to our handler
            context.RemapHandler(httpHandler);
        }

        #region IHttpModule Members
        void IHttpModule.Dispose() {
            Dispose();
        }

        void IHttpModule.Init(HttpApplication application) {
            Init(application);
        }
        #endregion
    }
}
