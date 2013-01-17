namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class StringLengthAttributeAdapter : DataAnnotationsModelValidator<StringLengthAttribute> {
        public StringLengthAttributeAdapter(ModelMetadata metadata, ControllerContext context, StringLengthAttribute attribute)
            : base(metadata, context, attribute) {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationStringLengthRule(ErrorMessage, Attribute.MinimumLength, Attribute.MaximumLength) };
        }
    }
}
