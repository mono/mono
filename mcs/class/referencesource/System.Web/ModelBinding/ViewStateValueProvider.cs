using System.Diagnostics.CodeAnalysis;
using System.Web.UI;

namespace System.Web.ModelBinding {
    /// <summary>
    /// Provides a value from Page's ViewState.
    /// </summary>
    public sealed class ViewStateValueProvider : SimpleValueProvider {

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.SimpleValueProvider.#ctor(System.Web.ModelBinding.ModelBindingExecutionContext)",
            Justification = "SimpleValueProvider Constructor specifies the CultureInfo")]
        public ViewStateValueProvider(ModelBindingExecutionContext modelBindingExecutionContext)
            : base(modelBindingExecutionContext) {
        }

        protected override object FetchValue(string key) {
            StateBag pageViewState = ModelBindingExecutionContext.GetService<StateBag>();
            if (pageViewState != null) {
                return pageViewState[key];
            }
            return null;
        }
    }
}
