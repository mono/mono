namespace System.Web.Mvc {
    using System;

    internal delegate bool TryGetValueDelegate(object dictionary, string key, out object value);
}
