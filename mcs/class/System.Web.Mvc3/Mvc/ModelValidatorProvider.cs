namespace System.Web.Mvc {
    using System.Collections.Generic;

    public abstract class ModelValidatorProvider {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context);
    }
}
