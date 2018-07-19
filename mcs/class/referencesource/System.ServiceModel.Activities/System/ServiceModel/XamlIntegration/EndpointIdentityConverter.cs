//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Windows.Markup;

    [SuppressMessage(FxCop.Category.Xaml, "XAML1012",
        Justification = "ConvertFrom methods are not required for MarkupExtension converters")]
    public class EndpointIdentityConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return null;
            }
            if (destinationType == typeof(MarkupExtension) && value is EndpointIdentity)
            {
                if (value is SpnEndpointIdentity)
                {
                    return new SpnEndpointIdentityExtension((SpnEndpointIdentity)value);
                }
                else if (value is UpnEndpointIdentity)
                {
                    return new UpnEndpointIdentityExtension((UpnEndpointIdentity)value);
                }
                else
                {
                    return new EndpointIdentityExtension((EndpointIdentity)value);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
