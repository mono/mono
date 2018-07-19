using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Routing;
using System.Web.UI;

namespace System.Web.DynamicData {
    /// <summary>
    /// Route handler used for Dynamic Data
    /// </summary>
    public class DynamicDataRouteHandler : IRouteHandler {

        private static object s_requestContextKey = new object();
        private static object s_metaTableKey = new object();

        private object _requestItemsKey = new object();

        /// <summary>
        /// ctor
        /// </summary>
        public DynamicDataRouteHandler() {
            VirtualPathProvider = HostingEnvironment.VirtualPathProvider;
            CreateHandlerCallback = delegate(string s) {
                return (Page)BuildManager.CreateInstanceFromVirtualPath(s, typeof(Page));
            };
        }

        /// <summary>
        /// The MetaModel that the handler is associated with
        /// </summary>
        public MetaModel Model { get; internal set; }

        // the following properties are for mocking purposes
        internal VirtualPathProvider VirtualPathProvider { get; set; }
        private HttpContextBase _context;
        internal HttpContextBase HttpContext {
            get {
                return _context ?? new HttpContextWrapper(System.Web.HttpContext.Current);
            }
            set { _context = value; }
        }
        internal Func<string, IHttpHandler> CreateHandlerCallback { get; set; }

        /// <summary>
        /// Create a handler to process a Dynamic Data request
        /// </summary>
        /// <param name="route">The Route that was matched</param>
        /// <param name="table">The MetaTable found in the route</param>
        /// <param name="action">The Action found in the route</param>
        /// <returns></returns>
        public virtual IHttpHandler CreateHandler(DynamicDataRoute route, MetaTable table, string action) {
            // First, get the path to the page (could be custom, shared, or null)
            string virtualPath = GetPageVirtualPath(route, table, action);

            if (virtualPath != null) {
                // Gets called only for custom pages that we know exist or templates that may or may not
                // exist. This method will throw if virtualPath does not exist, which is fine for templates
                // but is not fine for custom pages.
                return CreateHandlerCallback(virtualPath);
            } else {
                // This should only occur in the event that scaffolding is disabled and the custom page
                // virtual path does not exist.
                return null;
            }
        }

        private string GetPageVirtualPath(DynamicDataRoute route, MetaTable table, string action) {
            long cacheKey = Misc.CombineHashCodes(table, route.ViewName ?? action);

            Dictionary<long, string> virtualPathCache = GetVirtualPathCache();

            string virtualPath;
            if (!virtualPathCache.TryGetValue(cacheKey, out virtualPath)) {
                virtualPath = GetPageVirtualPathNoCache(route, table, action);
                lock (virtualPathCache) {
                    virtualPathCache[cacheKey] = virtualPath;
                }
            }
            return virtualPath;
        }

        private Dictionary<long, string> GetVirtualPathCache() {
            var httpContext = HttpContext;
            Dictionary<long, string> virtualPathCache = (Dictionary<long, string>)httpContext.Items[_requestItemsKey];
            if (virtualPathCache == null) {
                virtualPathCache = new Dictionary<long, string>();
                httpContext.Items[_requestItemsKey] = virtualPathCache;
            }
            return virtualPathCache;
        }

        private string GetPageVirtualPathNoCache(DynamicDataRoute route, MetaTable table, string action) {
            // The view name defaults to the action
            string viewName = route.ViewName ?? action;

            // First, get the path to the custom page
            string customPageVirtualPath = GetCustomPageVirtualPath(table, viewName);

            if (VirtualPathProvider.FileExists(customPageVirtualPath)) {
                return customPageVirtualPath;
            } else {
                if (table.Scaffold) {
                    // If it doesn't exist, try the scaffolded page, but only if scaffolding is enabled on this table
                    return GetScaffoldPageVirtualPath(table, viewName);
                } else {
                    // If scaffolding is disabled, null the path so BuildManager doesn't get called.
                    return null;
                }
            }
        }

        /// <summary>
        /// Build the path to a custom page. By default, it looks like ~/DynamicData/CustomPages/[tablename]/[viewname].aspx
        /// </summary>
        /// <param name="table">The MetaTable that the page is for</param>
        /// <param name="viewName">The view name</param>
        /// <returns></returns>
        protected virtual string GetCustomPageVirtualPath(MetaTable table, string viewName) {
            string pathPattern = "{0}CustomPages/{1}/{2}.aspx";
            return String.Format(CultureInfo.InvariantCulture, pathPattern, Model.DynamicDataFolderVirtualPath, table.Name, viewName);
        }

        /// <summary>
        /// Build the path to a page template. By default, it looks like ~/DynamicData/PageTemplates/[tablename]/[viewname].aspx
        /// </summary>
        /// <param name="table">The MetaTable that the page is for</param>
        /// <param name="viewName">The view name</param>
        /// <returns></returns>
        protected virtual string GetScaffoldPageVirtualPath(MetaTable table, string viewName) {
            string pathPattern = "{0}PageTemplates/{1}.aspx";
            return String.Format(CultureInfo.InvariantCulture, pathPattern, Model.DynamicDataFolderVirtualPath, viewName);
        }

        /// <summary>
        /// Return the RequestContext for this request. A new one is created if needed (can happen if the current request
        /// is not a Dynamic Data request)
        /// </summary>
        /// <param name="httpContext">The current HttpContext</param>
        /// <returns>The RequestContext</returns>
        public static RequestContext GetRequestContext(HttpContext httpContext) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }

            return GetRequestContext(new HttpContextWrapper(httpContext));
        }

        internal static RequestContext GetRequestContext(HttpContextBase httpContext) {
            Debug.Assert(httpContext != null);

            // Look for the RequestContext in the HttpContext
            var requestContext = httpContext.Items[s_requestContextKey] as RequestContext;

            // If the current request didn't go through the routing engine (e.g. normal page),
            // there won't be a RequestContext.  If so, create a new one and save it
            if (requestContext == null) {
                var routeData = new RouteData();
                requestContext = new RequestContext(httpContext, routeData);

                // Add the query string params to the route data.  This allows non routed pages to support filtering.
                DynamicDataRoute.AddQueryStringParamsToRouteData(httpContext, routeData);

                httpContext.Items[s_requestContextKey] = requestContext;
            }

            return requestContext;
        }

        /// <summary>
        /// The MetaTable associated with the current HttpRequest. Can be null for non-Dynamic Data requests.
        /// </summary>
        /// <param name="httpContext">The current HttpContext</param>
        public static MetaTable GetRequestMetaTable(HttpContext httpContext) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }

            return GetRequestMetaTable(new HttpContextWrapper(httpContext));
        }

        internal static MetaTable GetRequestMetaTable(HttpContextBase httpContext) {
            Debug.Assert(httpContext != null);

            return (MetaTable)httpContext.Items[s_metaTableKey];
        }

        /// <summary>
        /// Set the MetaTable associated with the current HttpRequest.  Normally, this is set automatically from the
        /// route, but this method is useful to set the table when used outside of routing.
        /// </summary>
        public static void SetRequestMetaTable(HttpContext httpContext, MetaTable table) {
            SetRequestMetaTable(new HttpContextWrapper(httpContext), table);
        }

        internal static void SetRequestMetaTable(HttpContextBase httpContext, MetaTable table) {
            Debug.Assert(httpContext != null);

            httpContext.Items[s_metaTableKey] = table;
        }

        #region IRouteHandler Members
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
            // Save the RequestContext
            Debug.Assert(requestContext.HttpContext.Items[s_requestContextKey] == null);
            requestContext.HttpContext.Items[s_requestContextKey] = requestContext;

            // Get the dynamic route
            var route = (DynamicDataRoute)requestContext.RouteData.Route;

            // Get the Model from the route
            MetaModel model = route.Model;

            // Get the MetaTable and save it in the HttpContext
            MetaTable table = route.GetTableFromRouteData(requestContext.RouteData);
            requestContext.HttpContext.Items[s_metaTableKey] = table;

            // Get the action from the request context
            string action = route.GetActionFromRouteData(requestContext.RouteData);

            return CreateHandler(route, table, action);
        }
        #endregion
    }
}
