//------------------------------------------------------------------------------
// <copyright file="DataSourceCacheDurationConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web.Util;


    /// <devdoc>
    /// Converts a cache duration such as an integer or the text "Infinite" to a cache duration, where "Infinite" implies zero (0).
    /// </devdoc>
    public class DataSourceCacheDurationConverter : Int32Converter {
        private StandardValuesCollection _values;


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            else {
                return base.CanConvertFrom(context, sourceType);
            }
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null)
                return null;

            string stringValue = value as string;
            if (stringValue != null) {
                string textValue = stringValue.Trim();
                if (textValue.Length == 0) {
                    return 0;
                }

                if (String.Equals(textValue, "infinite", StringComparison.OrdinalIgnoreCase)) {
                    return 0;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(string)) {
                return true;
            }
            else {
                return base.CanConvertTo(context, destinationType);
            }
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if ((value != null) && (destinationType == typeof(string)) && ((int)value == 0)) {
                return "Infinite";
            }
            else {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }


        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (_values == null) {
                object[] values = new object[] { 0 };

                _values = new StandardValuesCollection(values);
            }
            return _values;
        }


        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }


        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}

