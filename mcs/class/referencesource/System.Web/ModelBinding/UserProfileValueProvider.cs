namespace System.Web.ModelBinding {

    using System.Diagnostics.CodeAnalysis;

    public sealed class UserProfileValueProvider : SimpleValueProvider {

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.SimpleValueProvider.#ctor(System.Web.ModelBinding.ModelBindingExecutionContext)", 
            Justification = "SimpleValueProvider Constructor specifies the CultureInfo")]
        public UserProfileValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : base(modelBindingExecutionContext) {
        }

        protected override object FetchValue(string key) {
            return ModelBindingExecutionContext.HttpContext.Profile;
        }
    }
}
