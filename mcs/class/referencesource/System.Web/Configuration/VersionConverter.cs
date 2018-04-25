//------------------------------------------------------------------------------
// <copyright file="VersionConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;

    internal sealed class VersionConverter : ConfigurationConverterBase {

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return new Version((string)value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            var version = (Version)value;
            return version.ToString();
        }

    }
}
