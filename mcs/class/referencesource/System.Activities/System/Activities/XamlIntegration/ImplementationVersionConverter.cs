// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Markup;

    public class ImplementationVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Version deserializedVersion = null;
            string stringValue = value as string;

            if (stringValue != null && Version.TryParse(stringValue, out deserializedVersion))
            {
                return deserializedVersion;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            Version implementationVersion = value as Version;
            if (destinationType == typeof(string) && implementationVersion != null)
            {
                return implementationVersion.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
