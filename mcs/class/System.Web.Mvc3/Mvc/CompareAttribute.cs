namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    [AttributeUsage(AttributeTargets.Property)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed to be a base class for other attributes.")]
    public class CompareAttribute : ValidationAttribute, IClientValidatable {

        public CompareAttribute(string otherProperty)
            : base(MvcResources.CompareAttribute_MustMatch) {
            if (otherProperty == null) {
                throw new ArgumentNullException("otherProperty");
            }
            OtherProperty = otherProperty;
        }

        public string OtherProperty { get; private set; }

        public override string FormatErrorMessage(string name) {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, OtherProperty);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherPropertyInfo == null) {
                return new ValidationResult(String.Format(CultureInfo.CurrentCulture, MvcResources.CompareAttribute_UnknownProperty, OtherProperty));
            }

            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (!Equals(value, otherPropertyValue)) {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }

        public static string FormatPropertyForClientValidation(string property) {
            if (property == null) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "property");
            }
            return "*." + property;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) {
            yield return new ModelClientValidationEqualToRule(FormatErrorMessage(metadata.GetDisplayName()), FormatPropertyForClientValidation(OtherProperty));
        }
    }
}