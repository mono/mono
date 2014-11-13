namespace System.Web.ModelBinding {

    public interface IValueProvider {
        bool ContainsPrefix(string prefix);
        ValueProviderResult GetValue(string key);
    }
}
