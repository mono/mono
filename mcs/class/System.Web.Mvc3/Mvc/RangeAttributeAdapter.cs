namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RangeAttributeAdapter : DataAnnotationsModelValidator<RangeAttribute> {
        public RangeAttributeAdapter(ModelMetadata metadata, ControllerContext context, RangeAttribute attribute)
            : base(metadata, context, attribute) {
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            string errorMessage = ErrorMessage; // Per Dev10 Bug #923283, need to make sure ErrorMessage is called before Minimum/Maximum
            return new[] { new ModelClientValidationRangeRule(errorMessage, Attribute.Minimum, Attribute.Maximum) };
        }
    }
}
