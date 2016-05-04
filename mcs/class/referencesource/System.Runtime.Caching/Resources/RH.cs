using System;
using System.Globalization;

namespace System.Runtime.Caching.Resources
{
    internal static class RH
    {
        public static string Format(string resource, params object[] args) {
            return String.Format(CultureInfo.CurrentCulture, resource, args);
        }
    }
}
