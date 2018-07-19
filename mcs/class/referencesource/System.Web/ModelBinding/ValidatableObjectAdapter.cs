namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;

    public class ValidatableObjectAdapter : ModelValidator {
        public ValidatableObjectAdapter(ModelMetadata metadata, ModelBindingExecutionContext context)
            : base(metadata, context) {
        }

        public override IEnumerable<ModelValidationResult> Validate(object container) {
            // NOTE: Container is never used here, because IValidatableObject doesn't give you
            // any way to get access to your container.

            object model = Metadata.Model;
            if (model == null) {
                return Enumerable.Empty<ModelValidationResult>();
            }

            IValidatableObject validatable = model as IValidatableObject;
            if (validatable == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.ValidatableObjectAdapter_IncompatibleType),
                        typeof(IValidatableObject).FullName,
                        model.GetType().FullName
                    )
                );
            }

            ValidationContext validationContext = new ValidationContext(validatable, null, null);
            return ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results) {
            foreach (ValidationResult result in results) {
                if (result != ValidationResult.Success) {
                    if (result.MemberNames == null || !result.MemberNames.Any()) {
                        yield return new ModelValidationResult { Message = result.ErrorMessage };
                    }
                    else {
                        foreach (string memberName in result.MemberNames) {
                            yield return new ModelValidationResult { Message = result.ErrorMessage, MemberName = memberName };
                        }
                    }
                }
            }
        }
    }
}
