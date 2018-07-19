namespace System.Web.ModelBinding {
    /// <summary>
    /// The interface providers a way for specifying an alternate name to use 
    /// for model binding instead of the parameter name.
    /// This should be implemented by custom value provider attributes applicable
    /// on method parameters that take alternate key than the parameter name for
    /// resolving the value.
    /// </summary>
    public interface IModelNameProvider {
        string GetModelName();
    }
}
