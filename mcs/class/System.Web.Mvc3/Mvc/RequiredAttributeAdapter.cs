namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RequiredAttributeAdapter : DataAnnotationsModelValidator<RequiredAttribute> {
        public RequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, RequiredAttribute attribute)
            : base(metadata, context, attribute) {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };
        }
    }
}
