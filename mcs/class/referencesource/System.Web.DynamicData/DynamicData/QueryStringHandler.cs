using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web.Resources;

namespace System.Web.DynamicData {
    internal static class QueryStringHandler {

        public static string AddFiltersToPath(string virtualPath, IDictionary<string, object> filters) {
            if (String.IsNullOrEmpty(virtualPath))
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, DynamicDataResources.QueryStringHandler_VirtualPathCannotBeEmpty),
                    "virtualPath");

            if (filters == null)
                throw new ArgumentNullException("filters");

            string queryString = ConcatenateQueryStringElements(filters);
            if (String.IsNullOrEmpty(queryString))
                return virtualPath;

            StringBuilder result = new StringBuilder(virtualPath);
            if (!virtualPath.Contains("?")) {
                result.Append('?');
            }
            else {
                if (!virtualPath.EndsWith("?", StringComparison.Ordinal) && !virtualPath.EndsWith("&", StringComparison.Ordinal)) {
                    result.Append('&');
                }
            }

            result.Append(queryString);

            return result.ToString();
        }

        private static string ConcatenateQueryStringElements(IDictionary<string, object> parameters) {
            if (parameters.Count == 0) {
                return String.Empty;
            }

            StringBuilder result = new StringBuilder();
            bool firstParam = true;
            foreach (String s in parameters.Keys) {
                if (!String.IsNullOrEmpty(s)) {
                    string key = SanitizeParameterComponent(s);
                    string value = SanitizeParameterComponent(parameters[s]);

                    if (firstParam)
                        firstParam = false;
                    else
                        result.Append('&');

                    result.Append(key);
                    result.Append('=');
                    result.Append(value);
                }
            }
            return result.ToString();
        }

        private static string SanitizeParameterComponent(object value) {
            if (value == null)
                return String.Empty;

            string strValue = value.ToString();

            // Trim trailing spaces, as they are typically meaningless, and make the url look ugly
            strValue = strValue.TrimEnd();

            return Uri.EscapeDataString(strValue);
        }
    }
}
