//------------------------------------------------------------------------------
// <copyright file="ControlValuePropertyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Web.Util;

    /// <devdoc>
    /// Specifies the default value property for a control.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ControlValuePropertyAttribute : Attribute {
        private readonly string _name;
        private readonly object _defaultValue;


        /// <devdoc>
        /// Initializes a new instance of the <see cref='System.Web.UI.ControlValuePropertyAttribute'/> class.
        /// </devdoc>
        public ControlValuePropertyAttribute(string name) {
            _name = name;
        }

        /// <devdoc>
        /// Initializes a new instance of the class, using the specified value as the default value.
        /// </devdoc>
        public ControlValuePropertyAttribute(string name, object defaultValue) {
            _name = name;
            _defaultValue = defaultValue;
        }

        /// <devdoc>
        /// Initializes a new instance of the class, converting the specified value to the
        /// specified type.
        /// </devdoc>
        public ControlValuePropertyAttribute(string name, Type type, string defaultValue) {
            _name = name;
            // The try/catch here is because attributes should never throw exceptions.  We would fail to
            // load an otherwise normal class.
            try {
                _defaultValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(defaultValue);
            }
            catch {
                System.Diagnostics.Debug.Fail("ControlValuePropertyAttribute: Default value of type " + type.FullName + " threw converting from the string '" + defaultValue + "'.");
            }
        }


        /// <devdoc>
        /// Gets the name of the default value property for the control this attribute is bound to.
        /// </devdoc>
        public string Name {
            get {
                return _name;
            }
        }

        /// <devdoc>
        /// Gets the value of the default value property for the control this attribute is bound to.
        /// </devdoc>
        public object DefaultValue {
            get {
                return _defaultValue;
            }
        }



        public override bool Equals(object obj) {
            ControlValuePropertyAttribute other = obj as ControlValuePropertyAttribute;

            if (other != null) {
                if (String.Equals(_name, other.Name, StringComparison.Ordinal)) {
                    if (_defaultValue != null) {
                        return _defaultValue.Equals(other.DefaultValue);
                    }
                    else {
                        return (other.DefaultValue == null);
                    }
                }
            }
            return false;
        }


        public override int GetHashCode() {
            return System.Web.Util.HashCodeCombiner.CombineHashCodes(
                ((Name != null) ? Name.GetHashCode() : 0),
                ((DefaultValue != null) ? DefaultValue.GetHashCode() : 0));
        }
    }
}
