//------------------------------------------------------------------------------
// <copyright file="BooleanConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides a type converter to convert
    ///       Boolean objects to and from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class BooleanConverter : TypeConverter {
        private static volatile StandardValuesCollection values;


        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object in the given source type to a Boolean object using the
        ///       specified context.</para>
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <devdoc>
        ///    <para>Converts the given value
        ///       object to a Boolean object.</para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                string text = ((string)value).Trim();
                try {
                    return Boolean.Parse(text);
                }
                catch (FormatException e) {
                    throw new FormatException(SR.GetString(SR.ConvertInvalidPrimitive, (string)value, "Boolean"), e);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    
        /// <devdoc>
        ///    <para>Gets a collection of standard values
        ///       for the Boolean data type.</para>
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (values == null) {
                values = new StandardValuesCollection(new object[] {true, false});
            }
            return values;
        }
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether the list of standard values returned from
        ///    <see cref='System.ComponentModel.BooleanConverter.GetStandardValues'/> is an exclusive list. </para>
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return true;
        }
        
        /// <devdoc>
        ///    <para>Gets a value indicating whether this object supports a standard set of values
        ///       that can be picked from a list.</para>
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}

