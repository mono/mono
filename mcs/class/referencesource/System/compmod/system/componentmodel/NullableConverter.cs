//------------------------------------------------------------------------------
// <copyright file="NullableConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter"]/*' />
    /// <devdoc>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class NullableConverter : TypeConverter {
        Type nullableType;
        Type simpleType;
        TypeConverter simpleTypeConverter;

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.NullableConverter"]/*' />
        /// <devdoc>
        /// </devdoc>
        public NullableConverter(Type type)
        {
            this.nullableType = type;

            this.simpleType = Nullable.GetUnderlyingType(type);
            if (this.simpleType == null) {
                throw new ArgumentException(SR.GetString(SR.NullableConverterBadCtorArg), "type");
            }

            this.simpleTypeConverter = TypeDescriptor.GetConverter(this.simpleType);
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.CanConvertFrom"]/*' />
        /// <devdoc>
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == this.simpleType) {
                return true;
            }
            else if (this.simpleTypeConverter != null) {
                return this.simpleTypeConverter.CanConvertFrom(context, sourceType);
            }
            else {
                return base.CanConvertFrom(context, sourceType);
            }
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.ConvertFrom"]/*' />
        /// <devdoc>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null || value.GetType() == this.simpleType) {
                return value;
            }
            else if (value is String && String.IsNullOrEmpty(value as String)) {
                return null;
            }
            else if (this.simpleTypeConverter != null) {
                object convertedValue = this.simpleTypeConverter.ConvertFrom(context, culture, value);
                return convertedValue;
            }
            else {
                return base.ConvertFrom(context, culture, value);
            }
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.CanConvertTo"]/*' />
        /// <devdoc>
        /// </devdoc>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == this.simpleType) {
                return true;
            }
            else if (destinationType == typeof(InstanceDescriptor)) {
                return true;
            }
            else if (this.simpleTypeConverter != null) {
                return this.simpleTypeConverter.CanConvertTo(context, destinationType);
            }
            else {
                return base.CanConvertTo(context, destinationType);
            }
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.ConvertTo"]/*' />
        /// <devdoc>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == this.simpleType && this.nullableType.IsInstanceOfType(value)) {
                return value;
            }
            else if (destinationType == typeof(InstanceDescriptor)) {
                ConstructorInfo ci = nullableType.GetConstructor(new Type[] {simpleType});
                Debug.Assert(ci != null, "Couldn't find constructor");
                return new InstanceDescriptor(ci, new object[] {value}, true);
            }
            else if (value == null) {
                // Handle our own nulls here
                if (destinationType == typeof(string)) {
                    return string.Empty;
                }
            }
            else if (this.simpleTypeConverter != null) {
                return this.simpleTypeConverter.ConvertTo(context, culture, value, destinationType);
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.CreateInstance"]/*' />
        /// <devdoc>
        /// </devdoc>
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues) {
            if (simpleTypeConverter != null) {
                object instance = simpleTypeConverter.CreateInstance(context, propertyValues);
                return instance;
            }

            return base.CreateInstance(context, propertyValues);
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether changing a value on this object requires a 
        ///       call to <see cref='System.ComponentModel.TypeConverter.CreateInstance'/> to create a new value,
        ///       using the specified context.</para>
        /// </devdoc>
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
            if (simpleTypeConverter != null) {
                return simpleTypeConverter.GetCreateInstanceSupported(context);
            }

            return base.GetCreateInstanceSupported(context);
        }        

        /// <devdoc>
        ///    <para>Gets a collection of properties for
        ///       the type of array specified by the value parameter using the specified context and
        ///       attributes.</para>
        /// </devdoc>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
            if (simpleTypeConverter != null) {
                object unwrappedValue = value;
                return simpleTypeConverter.GetProperties(context, unwrappedValue, attributes);
            }

            return base.GetProperties(context, value, attributes);
        }        

        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object supports properties using the
        ///       specified context.</para>
        /// </devdoc>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
            if (simpleTypeConverter != null) {
                return simpleTypeConverter.GetPropertiesSupported(context);
            }

            return base.GetPropertiesSupported(context);
        }        

        /// <devdoc>
        ///    <para>Gets a collection of standard values for the data type this type converter is
        ///       designed for.</para>
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (simpleTypeConverter != null) {
                StandardValuesCollection values = simpleTypeConverter.GetStandardValues(context);
                if (GetStandardValuesSupported(context) && values != null) {
                    // Create a set of standard values around nullable instances.  
                    object[] wrappedValues = new object[values.Count + 1];
                    int idx = 0;

                    wrappedValues[idx++] = null;
                    foreach(object value in values) {
                        wrappedValues[idx++] = value;
                    }

                    return new StandardValuesCollection(wrappedValues);
                }
            }

            return base.GetStandardValues(context);
        }        

        /// <devdoc>
        ///    <para>Gets a value indicating whether the collection of standard values returned from
        ///    <see cref='System.ComponentModel.TypeConverter.GetStandardValues'/> is an exclusive 
        ///       list of possible values, using the specified context.</para>
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            if (simpleTypeConverter != null) {
                return simpleTypeConverter.GetStandardValuesExclusive(context);
            }

            return base.GetStandardValuesExclusive(context);
        }        

        /// <devdoc>
        ///    <para>Gets a value indicating
        ///       whether this object
        ///       supports a standard set of values that can be picked
        ///       from a list using the specified context.</para>
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            if (simpleTypeConverter != null) {
                return simpleTypeConverter.GetStandardValuesSupported(context);
            }

            return base.GetStandardValuesSupported(context);
        }        

        /// <devdoc>
        ///    <para>Gets
        ///       a value indicating whether the given value object is valid for this type.</para>
        /// </devdoc>
        public override bool IsValid(ITypeDescriptorContext context, object value) {
            if (simpleTypeConverter != null) {
                object unwrappedValue = value;
                if (unwrappedValue == null) {
                    return true; // null is valid for nullable.
                }
                else {
                    return simpleTypeConverter.IsValid(context, unwrappedValue);
                }
            }

            return base.IsValid(context, value);
        }        
        
        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.NullableType"]/*' />
        /// <devdoc>
        /// </devdoc>
        public Type NullableType
        {
            get
            {
                return nullableType;
            }
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.UnderlyingType"]/*' />
        /// <devdoc>
        /// </devdoc>
        public Type UnderlyingType
        {
            get
            {
                return simpleType;
            }
        }

        /// <include file='doc\NullableConverter.uex' path='docs/doc[@for="NullableConverter.UnderlyingTypeConverter"]/*' />
        /// <devdoc>
        /// </devdoc>
        public TypeConverter UnderlyingTypeConverter
        {
            get
            {
                return simpleTypeConverter;
            }
        }
    }
}
