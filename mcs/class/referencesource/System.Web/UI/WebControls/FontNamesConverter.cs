//------------------------------------------------------------------------------
// <copyright file="FontNamesConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel.Design;
    using System;
    using System.ComponentModel;
    using System.Collections;    
    using System.Globalization;

    /// <devdoc>
    ///   Converts a string with font names separated by commas to and from 
    ///   an array of strings containing individual names.
    /// </devdoc>
    public class FontNamesConverter : TypeConverter {


        /// <devdoc>
        ///   Determines if the specified data type can be converted to an array of strings
        ///   containing individual font names.
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return false;
        }


        /// <devdoc>
        ///   Parses a string that represents a list of font names separated by 
        ///   commas into an array of strings containing individual font names.
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                if (((string)value).Length == 0) {
                    return new string[0];
                }
                
                string[] names = ((string)value).Split(new char[] { culture.TextInfo.ListSeparator[0] });
                for (int i = 0; i < names.Length; i++) {
                    names[i] = names[i].Trim();
                }
                return names;
            }
            throw GetConvertFromException(value);
        }


        /// <devdoc>
        ///   Creates a string that represents a list of font names separated 
        ///   by commas from an array of strings containing individual font names.
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                
                if (value == null) {
                    return String.Empty;
                }
                return string.Join(culture.TextInfo.ListSeparator, ((string[])value));
            }
            throw GetConvertToException(value, destinationType);
        }
    }
}
