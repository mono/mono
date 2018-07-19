using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Validation attribute to indicate that a property field or parameter is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class RequiredAttribute : ValidationAttribute {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>This constructor selects a reasonable default error message for <see cref="ValidationAttribute.FormatErrorMessage"/></remarks>
        public RequiredAttribute()
            : base(() => DataAnnotationsResources.RequiredAttribute_ValidationError) {
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the attribute should allow empty strings.
        /// </summary>
        public bool AllowEmptyStrings { get; set; }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns><c>false</c> if the <paramref name="value"/> is null or an empty string. If <see cref="RequiredAttribute.AllowEmptyStrings"/>
        /// then <c>false</c> is returned only if <paramref name="vale"/> is null.</returns>
#if !SILVERLIGHT
        public
#else
        internal
#endif
        override bool IsValid(object value) {
            if (value == null) {
                return false;
            }

            // only check string length if empty strings are not allowed
            var stringValue = value as string;
            if (stringValue != null && !AllowEmptyStrings) {
                return stringValue.Trim().Length != 0;
            }

            return true;
        }
    }
}
