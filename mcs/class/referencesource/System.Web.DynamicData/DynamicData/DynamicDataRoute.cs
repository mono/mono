using System.Web.Routing;
using System.Diagnostics;

namespace System.Web.DynamicData {
    /// <summary>
    /// Route used by Dynamic Data
    /// </summary>
    public class DynamicDataRoute : Route {
        internal const string ActionToken = "Action";
        internal const string TableToken = "Table";
        internal const string ModelToken = "__Model";

        private MetaModel _model;
        private volatile bool _initialized;
        private object _initializationLock = new object();

        /// <summary>
        /// Construct a DynamicDataRoute
        /// </summary>
        /// <param name="url">url passed to the base ctor</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public DynamicDataRoute(string url)
            : base(url, new DynamicDataRouteHandler()) {
        }

        /// <summary>
        /// Name of the table that this route applies to. Can be omitted.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Action that this route applies to. Can be omitted.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The ViewName is the name of the page used to handle the request. If omitted, it defaults to the Action name.
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// The MetaModel that this route applies to
        /// </summary>
        public MetaModel Model {
            get { return _model ?? MetaModel.Default; }
            set { _model = value; }
        }

        // Make sure that if the Table or Action properties were used, they get added to
        // the Defaults dictionary
        private void EnsureRouteInitialize() {
            if (!_initialized) {
                lock (_initializationLock) {
                    if (!_initialized) {
                        // Give the model to the handler
                        Debug.Assert(Model != null);
                        RouteHandler.Model = Model;

                        // If neither was specified, we don't need to do anything
                        if (Table == null && Action == null)
                            return;

                        // If we don't already have a Defaults dictionary, create one
                        if (Defaults == null)
                            Defaults = new RouteValueDictionary();

                        if (Table != null) {
                            // Try to get the table just to cause a failure if it doesn't exist
                            var metaTable = Model.GetTable(Table);

                            Defaults[TableToken] = Table;
                        }

                        if (Action != null)
                            Defaults[ActionToken] = Action;

                        _initialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        public override RouteData GetRouteData(HttpContextBase httpContext) {
            EnsureRouteInitialize();

            // Try to get the route data for this route
            RouteData routeData = base.GetRouteData(httpContext);

            // If not, we're done
            if (routeData == null) {
                return null;
            }

            // Add all the query string values to the RouteData
            // 
            AddQueryStringParamsToRouteData(httpContext, routeData);

            // Check if the route values match an existing table and if they can be served by a scaffolded or custom page
            if (!VerifyRouteValues(routeData.Values))
                return null;

            return routeData;
        }

        internal static void AddQueryStringParamsToRouteData(HttpContextBase httpContext, RouteData routeData) {
            foreach (string key in httpContext.Request.QueryString) {
                // Don't overwrite existing items
                if (!routeData.Values.ContainsKey(key)) {
                    routeData.Values[key] = httpContext.Request.QueryString[key];
                }
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            EnsureRouteInitialize();

            // Check if the route values include a MetaModel
            object modelObject;
            if (values.TryGetValue(ModelToken, out modelObject)) {
                var model = modelObject as MetaModel;
                if (model != null) {
                    // If it's different from the one for this route, fail the route matching
                    if (modelObject != Model)
                        return null;

                    // It has the right model, so we want to continue.  But we first need to
                    // remove this token so it doesn't affect the path
                    values.Remove(ModelToken);
                }
            }

            // Call the base to try to generate a path from this route
            VirtualPathData virtualPathData = base.GetVirtualPath(requestContext, values);

            // If not, we're done
            if (virtualPathData == null)
                return null;

            // Check if the route values match an existing table and if they can be served by a scaffolded or custom page
            if (VerifyRouteValues(values)) {
                return virtualPathData;
            }
            else {
                return null;
            }
        }

        private bool VerifyRouteValues(RouteValueDictionary values) {
            // Get the MetaTable and action.  If either is missing, return false to skip this route
            object tableNameObject, actionObject;
            if (!values.TryGetValue(TableToken, out tableNameObject) || !values.TryGetValue(ActionToken, out actionObject)) {
                return false;
            }

            MetaTable table;
            // If no table by such name is available, return false to move on to next route.
            if (!Model.TryGetTable((string)tableNameObject, out table)) {
                return false;
            }

            // Check if n Page can be accessed for the table/action (either scaffold or custom).
            // If not, return false so that this route is not used and the search goes on.
            return RouteHandler.CreateHandler(this, table, (string)actionObject) != null;
        }

        /// <summary>
        /// Extract the MetaTable from the RouteData. Fails if it can't find it
        /// </summary>
        /// <param name="routeData">The route data</param>
        /// <returns>The found MetaTable</returns>
        public MetaTable GetTableFromRouteData(RouteData routeData) {
            string tableName = routeData.GetRequiredString(TableToken);
            return Model.GetTable(tableName);
        }

        /// <summary>
        /// Extract the Action from the RouteData. Fails if it can't find it
        /// </summary>
        /// <param name="routeData">The route data</param>
        /// <returns>The found Action</returns>
        public string GetActionFromRouteData(RouteData routeData) {
            return routeData.GetRequiredString(ActionToken);
        }

        /// <summary>
        /// Strongly typed version of Route.RouteHandler for convenience
        /// </summary>
        public new DynamicDataRouteHandler RouteHandler {
            get { return (DynamicDataRouteHandler)base.RouteHandler; }
            set { base.RouteHandler = value; }
        }
    }
}
