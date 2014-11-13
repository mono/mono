namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;

    public abstract class ModelValidator {
        protected ModelValidator(ModelMetadata metadata, ModelBindingExecutionContext modelBindingExecutionContext) {
            if (metadata == null) {
                throw new ArgumentNullException("metadata");
            }
            if (modelBindingExecutionContext == null) {
                throw new ArgumentNullException("modelBindingExecutionContext");
            }

            Metadata = metadata;
            ModelBindingExecutionContext = modelBindingExecutionContext;
        }

        protected internal ModelBindingExecutionContext ModelBindingExecutionContext { get; private set; }

        public virtual bool IsRequired {
            get {
                return false;
            }
        }

        protected internal ModelMetadata Metadata { get; private set; }

#if UNDEF
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may perform non-trivial work.")]
        public virtual IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            return Enumerable.Empty<ModelClientValidationRule>();
        }
#endif

        public static ModelValidator GetModelValidator(ModelMetadata metadata, ModelBindingExecutionContext context) {
            return new CompositeModelValidator(metadata, context);
        }

        public abstract IEnumerable<ModelValidationResult> Validate(object container);

        private class CompositeModelValidator : ModelValidator {
            public CompositeModelValidator(ModelMetadata metadata, ModelBindingExecutionContext modelBindingExecutionContext)
                : base(metadata, modelBindingExecutionContext) {
            }

            public override IEnumerable<ModelValidationResult> Validate(object container) {
                bool propertiesValid = true;

                foreach (ModelMetadata propertyMetadata in Metadata.Properties) {
                    foreach (ModelValidator propertyValidator in propertyMetadata.GetValidators(ModelBindingExecutionContext)) {
                        foreach (ModelValidationResult propertyResult in propertyValidator.Validate(Metadata.Model)) {
                            propertiesValid = false;
                            yield return new ModelValidationResult {
                                MemberName = ValueProviderUtil.CreateSubPropertyName(propertyMetadata.PropertyName, propertyResult.MemberName),
                                Message = propertyResult.Message
                            };
                        }
                    }
                }

                if (propertiesValid) {
                    foreach (ModelValidator typeValidator in Metadata.GetValidators(ModelBindingExecutionContext)) {
                        foreach (ModelValidationResult typeResult in typeValidator.Validate(container)) {
                            yield return typeResult;
                        }
                    }
                }
            }
        }
    }
}
