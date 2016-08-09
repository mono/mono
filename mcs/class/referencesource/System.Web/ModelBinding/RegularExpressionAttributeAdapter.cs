namespace System.Web.ModelBinding {
    using System.ComponentModel.DataAnnotations;

    public sealed class RegularExpressionAttributeAdapter : DataAnnotationsModelValidator<RegularExpressionAttribute> {
        public RegularExpressionAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, RegularExpressionAttribute attribute)
            : base(metadata, context, attribute) {
        }

        protected override string GetLocalizedErrorMessage(string errorMessage) {
            return GetLocalizedString(errorMessage, Metadata.GetDisplayName(), Attribute.Pattern);
        }

#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Attribute.Pattern) };
        }
#endif
    }
}
