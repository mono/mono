namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.Routing;

    public class AreaRegistrationContext {

        private readonly HashSet<string> _namespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public AreaRegistrationContext(string areaName, RouteCollection routes)
            : this(areaName, routes, null) {
        }

        public AreaRegistrationContext(string areaName, RouteCollection routes, object state) {
            if (String.IsNullOrEmpty(areaName)) {
                throw Error.ParameterCannotBeNullOrEmpty("areaName");
            }
            if (routes == null) {
                throw new ArgumentNullException("routes");
            }

            AreaName = areaName;
            Routes = routes;
            State = state;
        }

        public string AreaName {
            get;
            private set;
        }

        public ICollection<string> Namespaces {
            get {
                return _namespaces;
            }
        }

        public RouteCollection Routes {
            get;
            private set;
        }

        public object State {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url) {
            return MapRoute(name, url, (object)null /* defaults */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url, object defaults) {
            return MapRoute(name, url, defaults, (object)null /* constraints */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url, object defaults, object constraints) {
            return MapRoute(name, url, defaults, constraints, null /* namespaces */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url, string[] namespaces) {
            return MapRoute(name, url, (object)null /* defaults */, namespaces);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url, object defaults, string[] namespaces) {
            return MapRoute(name, url, defaults, null /* constraints */, namespaces);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is not a regular URL as it may contain special routing characters.")]
        public Route MapRoute(string name, string url, object defaults, object constraints, string[] namespaces) {
            if (namespaces == null && Namespaces != null) {
                namespaces = Namespaces.ToArray();
            }

            Route route = Routes.MapRoute(name, url, defaults, constraints, namespaces);
            route.DataTokens["area"] = AreaName;

            // disabling the namespace lookup fallback mechanism keeps this areas from accidentally picking up
            // controllers belonging to other areas
            bool useNamespaceFallback = (namespaces == null || namespaces.Length == 0);
            route.DataTokens["UseNamespaceFallback"] = useNamespaceFallback;

            return route;
        }

    }
}
