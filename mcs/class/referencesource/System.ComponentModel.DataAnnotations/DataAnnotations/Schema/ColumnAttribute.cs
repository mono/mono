using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations.Schema {
    /// <summary>
    /// Specifies the database column that a property is mapped to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class ColumnAttribute : Attribute {
        private readonly string _name;
        private string _typeName;
        private int _order = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
        /// </summary>
        public ColumnAttribute() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the column the property is mapped to.</param>
        public ColumnAttribute(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ArgumentIsNullOrWhitespace, "name"));
            }

            _name = name;
        }

        /// <summary>
        /// The name of the column the property is mapped to.
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The zero-based order of the column the property is mapped to.
        /// </summary>
        public int Order {
            get { return _order; }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                _order = value;
            }
        }

        /// <summary>
        /// The database provider specific data type of the column the property is mapped to.
        /// </summary>
        public string TypeName {
            get { return _typeName; }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ArgumentIsNullOrWhitespace, "value"));
                }

                _typeName = value;
            }
        }
    }
}
