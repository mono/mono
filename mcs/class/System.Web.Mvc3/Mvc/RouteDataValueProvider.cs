namespace System.Web.Mvc {
    using System;
    using System.Globalization;

    public sealed class RouteDataValueProvider : DictionaryValueProvider<object> {

        // RouteData should use the invariant culture since it's part of the URL, and the URL should be
        // interpreted in a uniform fashion regardless of the origin of a particular request.
        public RouteDataValueProvider(ControllerContext controllerContext)
            : base(controllerContext.RouteData.Values, CultureInfo.InvariantCulture) {
        }

    }
}
