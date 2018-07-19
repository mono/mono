namespace System.Web.ModelBinding {
    using System.Globalization;

    public sealed class FormValueProvider : NameValueCollectionValueProvider {

        public FormValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : this(modelBindingExecutionContext, modelBindingExecutionContext.HttpContext.Request.Unvalidated) {
        }

        // For unit testing
        internal FormValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, UnvalidatedRequestValuesBase unvalidatedValues)
            : base(modelBindingExecutionContext.HttpContext.Request.Form, unvalidatedValues.Form, CultureInfo.CurrentCulture) {
        }

    }
}
