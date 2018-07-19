//------------------------------------------------------------------------------
// <copyright file="GenericEnumConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;

namespace System.Configuration {

    public sealed class GenericEnumConverter : ConfigurationConverterBase {
        private Type _enumType;

        public GenericEnumConverter(Type typeEnum) {
            if (typeEnum == null) {
                throw new ArgumentNullException("typeEnum");
            }

            _enumType = typeEnum;
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type) {
            Debug.Assert(typeof(System.Enum).IsAssignableFrom(value.GetType()), "typeof(System.Enum).IsAssignableFrom(value.GetType())");

            return value.ToString();
        }
        
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data) {
            object result = null;
            //
            // For any error, throw the ArgumentException with SR.Invalid_enum_value
            //
            try {
                string value = (string)data;
                if (String.IsNullOrEmpty(value)) {
                    throw new Exception();
                }

                // Disallow numeric values for enums.
                if (!String.IsNullOrEmpty(value) &&
                        (Char.IsDigit(value[0]) ||
                        (value[0] == '-') ||
                        (value[0] == '+'))) {
                    throw new Exception();
                }

                if (value != value.Trim()) { // throw if the value has whitespace 
                    throw new Exception();
                }

                result = Enum.Parse(_enumType, value);
            }
            catch {
                StringBuilder names = new StringBuilder();

                foreach (string name in Enum.GetNames(_enumType)) {
                    if (names.Length != 0) {
                        names.Append(", ");
                    }
                    names.Append(name);
                }
                throw new ArgumentException(SR.GetString(SR.Invalid_enum_value, names.ToString()));
            }
            return result;
        }
    }
}
