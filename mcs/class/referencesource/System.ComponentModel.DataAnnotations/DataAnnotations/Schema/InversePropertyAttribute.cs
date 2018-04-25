using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations.Schema {
    /// <summary>
    /// Specifies the inverse of a navigation property that represents the other end of the same relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class InversePropertyAttribute : Attribute {
        private readonly string _property;

        /// <summary>
        /// Initializes a new instance of the <see cref="InversePropertyAttribute"/> class.
        /// </summary>
        /// <param name="property">The navigation property representing the other end of the same relationship.</param>
        public InversePropertyAttribute(string property) {
            if (string.IsNullOrWhiteSpace(property)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ArgumentIsNullOrWhitespace, "property"));
            }
            _property = property;
        }

        /// <summary>
        /// The navigation property representing the other end of the same relationship. 
        /// </summary>
        public string Property {
            get { return _property; }
        }
    }
}
