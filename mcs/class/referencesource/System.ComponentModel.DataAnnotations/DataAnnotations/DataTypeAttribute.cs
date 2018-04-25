using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Allows for clarification of the <see cref="DataType"/> represented by a given
    /// property (such as <see cref="System.ComponentModel.DataAnnotations.DataType.PhoneNumber"/>
    /// or <see cref="System.ComponentModel.DataAnnotations.DataType.Url"/>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class DataTypeAttribute : ValidationAttribute {
        /// <summary>
        /// Gets the DataType. If it equals DataType.Custom, <see cref="CustomDataType"/> should also be retrieved.
        /// </summary>
        public DataType DataType { get; private set; }

        /// <summary>
        /// Gets the string representing a custom data type. Returns a non-null value only if <see cref="DataType"/> is DataType.Custom.
        /// </summary>
        public string CustomDataType { get; private set; }

        /// <summary>
        /// Return the name of the data type, either using the <see cref="DataType"/> enum or <see cref="CustomDataType"/> string
        /// </summary>
        /// <returns>The name of the data type enum</returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method throws an exception if the properties have not been configured correctly")]
        public virtual string GetDataTypeName() {
            this.EnsureValidDataType();

            if (DataType == DataType.Custom) {
                // If it's a custom type string, use it as the template name
                return this.CustomDataType;
            } else {
                // If it's an enum, turn it into a string
                // Use the cached array with enum string values instead of ToString() as the latter is too slow
                return _dataTypeStrings[(int)DataType];
            }
        }

        /// <summary>
        /// Gets the default display format that gets used along with this DataType.
        /// </summary>
        public DisplayFormatAttribute DisplayFormat { get; protected set; }

        /// <summary>
        /// Constructor that accepts a data type enumeration
        /// </summary>
        /// <param name="dataType">The <see cref="DataType"/> enum value indicating the type to apply.</param>
        public DataTypeAttribute(DataType dataType) {
            DataType = dataType;

            // Set some DisplayFormat for a few specific data types
            switch (dataType) {
                case DataType.Date:
                    this.DisplayFormat = new DisplayFormatAttribute();
                    this.DisplayFormat.DataFormatString = "{0:d}";
                    this.DisplayFormat.ApplyFormatInEditMode = true;
                    break;
                case DataType.Time:
                    this.DisplayFormat = new DisplayFormatAttribute();
                    this.DisplayFormat.DataFormatString = "{0:t}";
                    this.DisplayFormat.ApplyFormatInEditMode = true;
                    break;
                case DataType.Currency:
                    this.DisplayFormat = new DisplayFormatAttribute();
                    this.DisplayFormat.DataFormatString = "{0:C}";

                    // Don't set ApplyFormatInEditMode for currencies because the currency
                    // symbol can't be parsed
                    break;
            }
        }

        /// <summary>
        /// Constructor that accepts the string name of a custom data type
        /// </summary>
        /// <param name="customDataType">The string name of the custom data type.</param>
        public DataTypeAttribute(string customDataType)
            : this(DataType.Custom) {
            this.CustomDataType = customDataType;
        }

        /// <summary>
        /// Override of <see cref="ValidationAttribute.IsValid(object)"/>
        /// </summary>
        /// <remarks>This override always returns <c>true</c>.  Subclasses should override this to provide the correct result.</remarks>
        /// <param name="value">The value to validate</param>
        /// <returns>Unconditionally returns <c>true</c></returns>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
#if !SILVERLIGHT
        public
#else
        internal
#endif
        override bool IsValid(object value) {
            this.EnsureValidDataType();

            return true;
        }

        /// <summary>
        /// Throws an exception if this attribute is not correctly formed
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
        private void EnsureValidDataType() {
            if (this.DataType == DataType.Custom && String.IsNullOrEmpty(this.CustomDataType)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DataTypeAttribute_EmptyDataTypeString));
            }
        }

        private static string[] _dataTypeStrings = Enum.GetNames(typeof(DataType));
    }
}
