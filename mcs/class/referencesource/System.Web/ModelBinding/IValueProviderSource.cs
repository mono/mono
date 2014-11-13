namespace System.Web.ModelBinding {
    /// <summary>
    /// This interface provides a way for model binding system to use custom value providers like
    /// Form, QueryString, ViewState.
    /// </summary>
    public interface IValueProviderSource {
        IValueProvider GetValueProvider(ModelBindingExecutionContext modelBindingExecutionContext);
    }
}
