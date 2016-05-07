namespace System.Web.ModelBinding {
    using System.Globalization;
    using System.Web.Routing;

    public sealed class RouteDataValueProvider : DictionaryValueProvider<object> {

        // RouteData should use the invariant culture since it's part of the URL, and the URL should be
        // interpreted in a uniform fashion regardless of the origin of a particular request.
        public RouteDataValueProvider(ModelBindingExecutionContext modelBindingExecutionContext) : 
            base(GetRouteValues(modelBindingExecutionContext), CultureInfo.InvariantCulture) {
        }

        private static RouteValueDictionary GetRouteValues(ModelBindingExecutionContext modelBindingExecutionContext) {
            RouteData routeData = modelBindingExecutionContext.GetService<RouteData>();
            if (routeData != null) {
                return routeData.Values;
            }

            return new RouteValueDictionary();
        }
    }
}
