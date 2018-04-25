//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;

    public class WorkflowIdentityConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string valueString = value as string;
            if (valueString != null)
            {
                return WorkflowIdentity.Parse(valueString);
            }
            return base.ConvertFrom(context, culture, value);
        }

        // No need to override [Can]ConvertTo, it automatically calls ToString
    }
}
