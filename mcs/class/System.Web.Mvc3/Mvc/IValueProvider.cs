namespace System.Web.Mvc {
    using System;

    public interface IValueProvider {
        bool ContainsPrefix(string prefix);
        ValueProviderResult GetValue(string key);
    }
}
