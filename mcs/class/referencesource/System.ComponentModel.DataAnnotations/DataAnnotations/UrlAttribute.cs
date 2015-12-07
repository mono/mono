namespace System.ComponentModel.DataAnnotations {
    using System;
    using System.ComponentModel.DataAnnotations.Resources;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class UrlAttribute : DataTypeAttribute {

        // This attribute provides server-side url validation equivalent to jquery validate,
        // and therefore shares the same regular expression.  See unit tests for examples.
        private static Regex _regex = CreateRegEx();

        public UrlAttribute()
            : base(DataType.Url) {

            // DevDiv 468241: set DefaultErrorMessage not ErrorMessage, allowing user to set
            // ErrorMessageResourceType and ErrorMessageResourceName to use localized messages.
            DefaultErrorMessage = DataAnnotationsResources.UrlAttribute_Invalid;
        }

        public override bool IsValid(object value) {
            if (value == null) {
                return true;
            }

            string valueAsString = value as string;

            // Use RegEx implementation if it has been created, otherwise use a non RegEx version.
            if (_regex != null) { 
                return valueAsString != null && _regex.Match(valueAsString).Length > 0;
            }
            else {
                return valueAsString != null &&
                (valueAsString.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                 || valueAsString.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                 || valueAsString.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private static Regex CreateRegEx() {
            // We only need to create the RegEx if this switch is enabled.
            if (AppSettings.DisableRegEx) {
                return null;
            }

            const string pattern = @"^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";
        
            const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        
            // Set explicit regex match timeout, sufficient enough for url parsing
            // Unless the global REGEX_DEFAULT_MATCH_TIMEOUT is already set
            TimeSpan matchTimeout = TimeSpan.FromSeconds(2);
        
            try {
                if (AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") == null) {
                    return new Regex(pattern, options, matchTimeout);
                }
            }
            catch {
                // Fallback on error
            }
        
            // Legacy fallback (without explicit match timeout)
            return new Regex(pattern, options);
        }
    }
}
