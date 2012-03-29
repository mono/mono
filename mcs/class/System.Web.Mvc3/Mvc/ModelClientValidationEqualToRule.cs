namespace System.Web.Mvc {
    public class ModelClientValidationEqualToRule: ModelClientValidationRule {
        public ModelClientValidationEqualToRule(string errorMessage, object other){
            ErrorMessage = errorMessage;
            ValidationType = "equalto";
            ValidationParameters["other"] = other;
        }
    }
}
