namespace System.Web.Mvc {
    public class ModelClientValidationRequiredRule : ModelClientValidationRule {
        public ModelClientValidationRequiredRule(string errorMessage) {
            ErrorMessage = errorMessage;
            ValidationType = "required";
        }
    }
}
