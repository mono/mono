namespace System.Web.ModelBinding {
    using System.ComponentModel.DataAnnotations;

    public sealed class RegularExpressionAttributeAdapter : DataAnnotationsModelValidator<RegularExpressionAttribute> {
        public RegularExpressionAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, RegularExpressionAttribute attribute)
            : base(metadata, context, attribute) {
        }

#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Attribute.Pattern) };
        }
#endif
    }
}
