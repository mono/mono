namespace System.Web.Mvc {
    using System.Diagnostics.CodeAnalysis;

    public class ModelClientValidationRemoteRule : ModelClientValidationRule {

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "The value is a not a regular URL since it may contain ~/ ASP.NET-specific characters")]
        public ModelClientValidationRemoteRule(string errorMessage, string url, string httpMethod, string additionalFields) {
            ErrorMessage = errorMessage;
            ValidationType = "remote";
            ValidationParameters["url"] = url;

            if (!string.IsNullOrEmpty(httpMethod)) {
                ValidationParameters["type"] = httpMethod;
            }

            ValidationParameters["additionalfields"] = additionalFields;
        }
    }
}