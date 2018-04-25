//------------------------------------------------------------------------------
// <copyright file="ConfigurationConverterBase.cs" company="Microsoft">
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

    public abstract class ConfigurationConverterBase : TypeConverter {

        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type) {
            return (type == typeof(string));
        }
        
        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type) {
            return (type == typeof(string));
        }

        internal void ValidateType(object value, Type expected) {
            if ((value != null) && (value.GetType() != expected)) {
                throw new ArgumentException(SR.GetString(SR.Converter_unsupported_value_type, expected.Name));
            }
        }
    }
}
