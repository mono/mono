namespace System.Web.ModelBinding {
    /// <summary>
    /// This represents an IValueProviderSource that supports skipping request validation.
    /// </summary>
    public interface IUnvalidatedValueProviderSource : IValueProviderSource {
        bool ValidateInput {
            get;
            set;
        }
    }
}
