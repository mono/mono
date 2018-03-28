namespace System.Web.ModelBinding {
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Profile;

    /// <summary>
    /// Provides a value from Page's ViewState.
    /// </summary>
    public sealed class ProfileValueProvider : SimpleValueProvider {

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.SimpleValueProvider.#ctor(System.Web.ModelBinding.ModelBindingExecutionContext)",
            Justification = "SimpleValueProvider Constructor specifies the CultureInfo")]
        public ProfileValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : base(modelBindingExecutionContext) {
        }

        protected override object FetchValue(string key) {
            object value = null;

            try {
                value = ModelBindingExecutionContext.HttpContext.Profile[key];
            }
            catch (System.Configuration.SettingsPropertyNotFoundException) {
            }
            
            return value;
        }
    }
}
