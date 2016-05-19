namespace System.Web.ModelBinding {
    using System.Globalization;

    public sealed class QueryStringValueProvider : NameValueCollectionValueProvider {

        // QueryString should use the invariant culture since it's part of the URL, and the URL should be
        // interpreted in a uniform fashion regardless of the origin of a particular request.
        public QueryStringValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : this(modelBindingExecutionContext, modelBindingExecutionContext.HttpContext.Request.Unvalidated) {
        }

        // For unit testing
        internal QueryStringValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, UnvalidatedRequestValuesBase unvalidatedValues)
            : base(modelBindingExecutionContext.HttpContext.Request.QueryString, unvalidatedValues.QueryString, CultureInfo.InvariantCulture) {
        }

    }
}
