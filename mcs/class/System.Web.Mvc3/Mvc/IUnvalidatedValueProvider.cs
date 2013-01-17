namespace System.Web.Mvc {
    using System;

    // Represents a special IValueProvider that has the ability to skip request validation.
    public interface IUnvalidatedValueProvider : IValueProvider {
        ValueProviderResult GetValue(string key, bool skipValidation);
    }
}
