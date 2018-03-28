//------------------------------------------------------------------------------
// <copyright file="ArrayConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Provides a type converter to convert <see cref='System.Array'/>
    /// objects to and from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class ArrayConverter : CollectionConverter
    {

        /// <devdoc>
        ///    <para>Converts the given value object to the specified destination type.</para>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string)) {
                if (value is Array) {
                    return SR.GetString(SR.ArrayConverterText, value.GetType().Name);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <devdoc>
        ///    <para>Gets a collection of properties for the type of array
        ///       specified by the value
        ///       parameter.</para>
        /// </devdoc>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {

            PropertyDescriptor[] props = null;

            if (value.GetType().IsArray) {
                Array valueArray = (Array)value;
                int length = valueArray.GetLength(0);
                props = new PropertyDescriptor[length];
                
                Type arrayType = value.GetType();
                Type elementType = arrayType.GetElementType();
                
                for (int i = 0; i < length; i++) {
                    props[i] = new ArrayPropertyDescriptor(arrayType, elementType, i);
                }
            }

            return new PropertyDescriptorCollection(props);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether this object
        ///       supports properties.</para>
        /// </devdoc>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return true;
        }

        private class ArrayPropertyDescriptor : SimplePropertyDescriptor {
            private int index;

            public ArrayPropertyDescriptor(Type arrayType, Type elementType, int index) : base(arrayType, "[" + index + "]", elementType, null) {
                this.index = index;
            }
            
            public override object GetValue(object instance) {
                if (instance is Array) {
                    Array array = (Array)instance;
                    if (array.GetLength(0) > index) {
                        return array.GetValue(index);
                    }
                }
                
                return null;
            }
            
            public override void SetValue(object instance, object value) {
                if (instance is Array) {
                    Array array = (Array)instance;
                    if (array.GetLength(0) > index) {
                        array.SetValue(value, index);
                    }
                    
                    OnValueChanged(instance, EventArgs.Empty);
                }
            }
        }
    }
}


