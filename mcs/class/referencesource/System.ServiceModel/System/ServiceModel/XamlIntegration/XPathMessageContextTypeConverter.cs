//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    public class XPathMessageContextTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(MarkupExtension) == sourceType)
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(MarkupExtension) == destinationType)
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is XPathMessageContextMarkupExtension)
            {
                return ((MarkupExtension)value).ProvideValue(null);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XPathMessageContext contextValue = value as XPathMessageContext;

            if (contextValue != null && typeof(MarkupExtension) == destinationType)
            {
                return new XPathMessageContextMarkupExtension(contextValue);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
