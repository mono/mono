namespace System.Web.Mvc {
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.Helpers;

    public sealed class QueryStringValueProvider : NameValueCollectionValueProvider {

        // QueryString should use the invariant culture since it's part of the URL, and the URL should be
        // interpreted in a uniform fashion regardless of the origin of a particular request.
        public QueryStringValueProvider(ControllerContext controllerContext)
            : this(controllerContext, new UnvalidatedRequestValuesWrapper(controllerContext.HttpContext.Request.Unvalidated())) {
        }

        // For unit testing
        internal QueryStringValueProvider(ControllerContext controllerContext, IUnvalidatedRequestValues unvalidatedValues)
            : base(controllerContext.HttpContext.Request.QueryString, unvalidatedValues.QueryString, CultureInfo.InvariantCulture) {
        }

    }
}
