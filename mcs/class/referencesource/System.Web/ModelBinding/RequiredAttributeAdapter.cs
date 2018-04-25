namespace System.Web.ModelBinding {
    using System.ComponentModel.DataAnnotations;

    public sealed class RequiredAttributeAdapter : DataAnnotationsModelValidator<RequiredAttribute> {
        public RequiredAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, RequiredAttribute attribute)
            : base(metadata, context, attribute) {
        }

#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };
        }
#endif
    }
}
