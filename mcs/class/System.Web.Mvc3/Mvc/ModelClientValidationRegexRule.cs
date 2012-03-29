namespace System.Web.Mvc {
    public class ModelClientValidationRegexRule : ModelClientValidationRule {
        public ModelClientValidationRegexRule(string errorMessage, string pattern) {
            ErrorMessage = errorMessage;
            ValidationType = "regex";
            ValidationParameters.Add("pattern", pattern);
        }
    }
}
