namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class DataAnnotationsModelValidator : ModelValidator {
        public DataAnnotationsModelValidator(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context) {

            if (attribute == null) {
                throw new ArgumentNullException("attribute");
            }

            Attribute = attribute;
        }

        protected internal ValidationAttribute Attribute { get; private set; }

        protected internal string ErrorMessage {
            get {
                return Attribute.FormatErrorMessage(Metadata.GetDisplayName());
            }
        }

        public override bool IsRequired {
            get {
                return Attribute is RequiredAttribute;
            }
        }

        internal static ModelValidator Create(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) {
            return new DataAnnotationsModelValidator(metadata, context, attribute);
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            IEnumerable<ModelClientValidationRule> results = base.GetClientValidationRules();

            IClientValidatable clientValidatable = Attribute as IClientValidatable;
            if (clientValidatable != null) {
                results = results.Concat(clientValidatable.GetClientValidationRules(Metadata, ControllerContext));
            }

            return results;
        }

        public override IEnumerable<ModelValidationResult> Validate(object container) {
            // Per the WCF RIA Services team, instance can never be null (if you have
            // no parent, you pass yourself for the "instance" parameter).
            ValidationContext context = new ValidationContext(container ?? Metadata.Model, null, null);
            context.DisplayName = Metadata.GetDisplayName();

            ValidationResult result = Attribute.GetValidationResult(Metadata.Model, context);
            if (result != ValidationResult.Success) {
                yield return new ModelValidationResult {
                    Message = result.ErrorMessage
                };
            }
        }
    }
}
