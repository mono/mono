namespace System.ComponentModel.DataAnnotations {
    using System;
    using System.ComponentModel.DataAnnotations.Resources;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PhoneAttribute : DataTypeAttribute {
        // see unit tests for examples
        private static Regex _regex = new Regex(@"^(\+\s?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s?(x|ext\.?)\s?\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public PhoneAttribute()
            : base(DataType.PhoneNumber) {
            
            // DevDiv 468241: set DefaultErrorMessage not ErrorMessage, allowing user to set
            // ErrorMessageResourceType and ErrorMessageResourceName to use localized messages.
            DefaultErrorMessage = DataAnnotationsResources.PhoneAttribute_Invalid;
        }

        public override bool IsValid(object value) {
            if (value == null) {
                return true;
            }

            string valueAsString = value as string;
            return valueAsString != null && _regex.Match(valueAsString).Length > 0;
        }
    }
}
