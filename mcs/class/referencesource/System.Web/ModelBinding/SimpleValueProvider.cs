using System.Globalization;

namespace System.Web.ModelBinding {
    /// <summary>
    /// This value provider supports single key-value lookup.
    /// SimpleValueProvider itself is unaware of actual look-up mechanism and delegates to child classes using the FetchValue method.
    /// Example simple value providers are ViewStateValueProvider and ControlValueProvider.
    /// </summary>
    public abstract class SimpleValueProvider : IValueProvider {

        private CultureInfo _cultureInfo;

        protected ModelBindingExecutionContext ModelBindingExecutionContext {
            get;
            private set;
        }

        protected SimpleValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : this(modelBindingExecutionContext, CultureInfo.CurrentCulture) {
        }

        protected SimpleValueProvider(ModelBindingExecutionContext modelBindingExecutionContext, CultureInfo cultureInfo) {
            ModelBindingExecutionContext = modelBindingExecutionContext;
            _cultureInfo = cultureInfo;
        }

        public virtual bool ContainsPrefix(string prefix) {
            if (prefix == null) {
                throw new ArgumentNullException("prefix");
            }

            return FetchValue(prefix) != null;
        }

        public virtual ValueProviderResult GetValue(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            object rawValue = FetchValue(key);
            if (rawValue == null) {
                return null;
            }
            string attemptedValue = Convert.ToString(rawValue, _cultureInfo);
            return new ValueProviderResult(rawValue, attemptedValue, _cultureInfo);
        }

        protected abstract object FetchValue(string key);
    }
}
