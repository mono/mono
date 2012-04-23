namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Linq;

    public class EmptyModelValidatorProvider : ModelValidatorProvider {
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context) {
            return Enumerable.Empty<ModelValidator>();
        }
    }
}
