namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading;
    using System.Web.Globalization;
    using System.Web.Util;

    public class DataAnnotationsModelValidator : ModelValidator {        
        public DataAnnotationsModelValidator(ModelMetadata metadata, ModelBindingExecutionContext context, ValidationAttribute attribute)
            : base(metadata, context) {

            if (attribute == null) {
                throw new ArgumentNullException("attribute");
            }

            Attribute = attribute;
        }

        protected internal ValidationAttribute Attribute { get; private set; }

        protected internal string ErrorMessage {
            get {
                if (UseStringLocalizerProvider) {
                    var errorMsg = GetLocalizedString(Attribute.ErrorMessage);

                    return errorMsg ?? Attribute.FormatErrorMessage(Metadata.GetDisplayName());
                }
                else {
                    return Attribute.FormatErrorMessage(Metadata.GetDisplayName());
                }
            }
        }

        protected string GetLocalizedString(string name, params object[] arguments) {
            if (StringLocalizerProviders.DataAnnotationStringLocalizerProvider != null) {
                return StringLocalizerProviders.DataAnnotationStringLocalizerProvider
                    .GetLocalizedString(Thread.CurrentThread.CurrentUICulture, name, arguments);
            }
            else {
                return null;
            }
        }

        public override bool IsRequired {
            get {
                return Attribute is RequiredAttribute;
            }
        }

        internal static ModelValidator Create(ModelMetadata metadata, ModelBindingExecutionContext context, ValidationAttribute attribute) {
            return new DataAnnotationsModelValidator(metadata, context, attribute);
        }

        //Client Validation is different for Web Forms. This will probably need to be enabled when MVC merges with Webforms model binding.
#if UNDEF
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules() {
            IEnumerable<ModelClientValidationRule> results = base.GetClientValidationRules();

            IClientValidatable clientValidatable = Attribute as IClientValidatable;
            if (clientValidatable != null) {
                results = results.Concat(clientValidatable.GetClientValidationRules(Metadata, ModelBindingExecutionContext));
            }

            return results;
        }
#endif

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            // Per the WCF RIA Services team, instance can never be null (if you have
            // no parent, you pass yourself for the "instance" parameter).
            ValidationContext context = new ValidationContext(container ?? Metadata.Model, null, null);
            context.DisplayName = Metadata.GetDisplayName();
            // Bug#563497 - Fix the issue that MemberName is not set when using custom ValidationAtrribute
            string memberName = null;
            if (AppSettings.GetValidationMemberName)
            {
                memberName = Metadata.PropertyName ?? Metadata.ModelType.Name;
                context.MemberName = memberName;
            }

            ValidationResult result = Attribute.GetValidationResult(Metadata.Model, context);
            if (result != ValidationResult.Success)
            {
                yield return new ModelValidationResult
                {
                    Message = GetValidationErrorMessage(result),
                    // Bug#563497 - Fix the issue that MemberName is not set when using custom ValidationAtrribute
                    MemberName = GetValidationErrorMemberName(result, memberName)
                };
            }

        }

        protected virtual string GetLocalizedErrorMessage(string errorMessage) {
            return GetLocalizedString(errorMessage, Metadata.GetDisplayName());
        }

        private string GetValidationErrorMessage(ValidationResult result) {
            string errorMsg;

            if (UseStringLocalizerProvider) {
                errorMsg = GetLocalizedErrorMessage(Attribute.ErrorMessage);

                errorMsg = errorMsg ?? result.ErrorMessage;
            }
            else {
                errorMsg = result.ErrorMessage;
            }
            return errorMsg;
        }

        private static string GetValidationErrorMemberName(ValidationResult result, string memberName)
        {
            string errorMemberName = null;
            if (AppSettings.GetValidationMemberName)
            {
                // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to 
                // construct the ModelKey for ModelStateDictionary. When validating at type level we want to append the
                // returned MemberNames if specified (e.g. person.Address.FirstName). For property validation, the
                // ModelKey can be constructed using the ModelMetadata and we should ignore MemberName (we don't want 
                // (person.Name.Name). However the invoking validator does not have a way to distinguish between these two
                // cases. Consequently we'll only set MemberName if this validation returns a MemberName that is different 
                // from the property being validated. 
                errorMemberName = result.MemberNames.FirstOrDefault();
                if (String.Equals(errorMemberName, memberName, StringComparison.Ordinal))
                {
                    errorMemberName = null;
                }
            }
            return errorMemberName;
        }
        

        private bool UseStringLocalizerProvider {
            get {
                // if developer already uses existing localization feature,
                // then we don't opt in the new localization feature.
                return (!string.IsNullOrEmpty(Attribute.ErrorMessage) &&
                    string.IsNullOrEmpty(Attribute.ErrorMessageResourceName) &&
                    Attribute.ErrorMessageResourceType == null);
            }
        }
    }
}
