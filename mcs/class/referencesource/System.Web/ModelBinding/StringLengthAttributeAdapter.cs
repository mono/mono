namespace System.Web.ModelBinding {
    using System.ComponentModel.DataAnnotations;

    public sealed class StringLengthAttributeAdapter : DataAnnotationsModelValidator<StringLengthAttribute> {
        public StringLengthAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, StringLengthAttribute attribute)
            : base(metadata, context, attribute) {
        }

        protected override string GetLocalizedErrorMessage(string errorMessage) {            
            return GetLocalizedString(errorMessage, Metadata.GetDisplayName(), Attribute.MinimumLength, Attribute.MaximumLength);
        }

#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationStringLengthRule(ErrorMessage, Attribute.MinimumLength, Attribute.MaximumLength) };
        }
#endif
    }
}
