namespace System.Web.ModelBinding {
    using System.ComponentModel.DataAnnotations;

    public sealed class RangeAttributeAdapter : DataAnnotationsModelValidator<RangeAttribute> {
        public RangeAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, RangeAttribute attribute)
            : base(metadata, context, attribute) {
        }

        protected override string GetLocalizedErrorMessage(string errorMessage) {
            return GetLocalizedString(errorMessage, Metadata.GetDisplayName(), Attribute.Minimum, Attribute.Maximum);

        }

#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            string errorMessage = ErrorMessage; // Per Dev10 Bug #923283, need to make sure ErrorMessage is called before Minimum/Maximum
            return new[] { new ModelClientValidationRangeRule(errorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
#endif
    }
}
