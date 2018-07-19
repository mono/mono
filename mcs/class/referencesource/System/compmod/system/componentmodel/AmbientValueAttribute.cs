//------------------------------------------------------------------------------
// <copyright file="AmbientValueAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies the ambient value for a property.  The ambient value is the value you
    ///    can set into a property to make it inherit its ambient.</para>
    /// </devdoc>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.All)]
    public sealed class AmbientValueAttribute : Attribute {
    
        private readonly object value;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class, converting the
        ///    specified value to the
        ///    specified type, and using the U.S. English culture as the
        ///    translation
        ///    context.</para>
        /// </devdoc>
        public AmbientValueAttribute(Type type, string value) {
        
            // The try/catch here is because attributes should never throw exceptions.  We would fail to
            // load an otherwise normal class.
            try {
                this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
            }
            catch {
                Debug.Fail("Ambient value attribute of type " + type.FullName + " threw converting from the string '" + value + "'.");
            }
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a Unicode
        ///    character.</para>
        /// </devdoc>
        public AmbientValueAttribute(char value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using an 8-bit unsigned
        ///    integer.</para>
        /// </devdoc>
        public AmbientValueAttribute(byte value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a 16-bit signed
        ///    integer.</para>
        /// </devdoc>
        public AmbientValueAttribute(short value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a 32-bit signed
        ///    integer.</para>
        /// </devdoc>
        public AmbientValueAttribute(int value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a 64-bit signed
        ///    integer.</para>
        /// </devdoc>
        public AmbientValueAttribute(long value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a
        ///    single-precision floating point
        ///    number.</para>
        /// </devdoc>
        public AmbientValueAttribute(float value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a
        ///    double-precision floating point
        ///    number.</para>
        /// </devdoc>
        public AmbientValueAttribute(double value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a <see cref='System.Boolean'/>
        /// value.</para>
        /// </devdoc>
        public AmbientValueAttribute(bool value) {
            this.value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/> class using a <see cref='System.String'/>.</para>
        /// </devdoc>
        public AmbientValueAttribute(string value) {
            this.value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.AmbientValueAttribute'/>
        /// class.</para>
        /// </devdoc>
        public AmbientValueAttribute(object value) {
            this.value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the ambient value of the property this
        ///       attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public object Value {
            get {
                return value;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            AmbientValueAttribute other = obj as AmbientValueAttribute;

            if (other != null) {
                if (value != null) {
                    return value.Equals(other.Value);
                }
                else {
                    return (other.Value == null);           
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}

