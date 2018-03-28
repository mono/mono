using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Allows overriding various display-related options for a given field. The options have the same meaning as in BoundField.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class DisplayFormatAttribute : Attribute {
        /// <summary>
        /// Gets or sets the format string
        /// </summary>
        public string DataFormatString { get; set; }

        /// <summary>
        /// Gets or sets the string to display when the value is null
        /// </summary>
        public string NullDisplayText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether empty strings should be set to null
        /// </summary>
        public bool ConvertEmptyStringToNull { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the format string should be used in edit mode
        /// </summary>
        public bool ApplyFormatInEditMode { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets or sets a value indicating whether the field should be html encoded
        /// </summary>
        public bool HtmlEncode { get; set; }
#endif

        /// <summary>
        /// Default constructor
        /// </summary>
        public DisplayFormatAttribute() {
            this.ConvertEmptyStringToNull = true; // default to true to match behavior in related components

#if !SILVERLIGHT
            this.HtmlEncode = true; // default to true to match behavior in related components
#endif
        }
    }
}
