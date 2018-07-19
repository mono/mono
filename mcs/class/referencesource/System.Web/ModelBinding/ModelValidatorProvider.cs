namespace System.Web.ModelBinding {
    using System.Collections.Generic;

    public abstract class ModelValidatorProvider {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context);
    }
}
