//------------------------------------------------------------------------------
// <copyright file="CommaDelimitedStringCollectionConverter.cs" company="Microsoft">
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

    public sealed class CommaDelimitedStringCollectionConverter : ConfigurationConverterBase {

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type) {

            ValidateType(value, typeof(CommaDelimitedStringCollection));
            CommaDelimitedStringCollection internalValue = value as CommaDelimitedStringCollection;
            if (internalValue != null) {
                return internalValue.ToString();
            }
            else {
                return null;
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data) {
            CommaDelimitedStringCollection attributeCollection = new CommaDelimitedStringCollection();
            attributeCollection.FromString((string)data);
            return attributeCollection;
        }
    }
}
