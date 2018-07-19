//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.XamlIntegration
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Xml.Linq;

    public class ServiceXNameTypeConverter : TypeConverter
    {                           
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return XNameTypeConverterHelper.CanConvertFrom(sourceType);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                if (!IsQualifiedName(stringValue))
                {
                    // We want the name to remain unqualified; we don't want XNameTypeConverter to add the default namespace
                    return XName.Get(stringValue);
                }
            }
            return XNameTypeConverterHelper.ConvertFrom(context, value) ?? base.ConvertFrom(context, culture, value);
        }
        
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return XNameTypeConverterHelper.CanConvertTo(destinationType);
        }
        
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XName name = value as XName;
            if (destinationType == typeof(string) && name != null)
            {
                if (name.Namespace == XNamespace.None)
                {
                    // return unqualified name
                    return name.LocalName;
                }
                else
                {
                    string result = (string)(XNameTypeConverterHelper.ConvertTo(context, value, destinationType) ??
                        base.ConvertTo(context, culture, value, destinationType));
                    if (IsQualifiedName(result))
                    {
                        return result;
                    }
                    else
                    {
                        // The name is in the default XAML namespace, so we need to fully-qualify it,
                        // or we'll interpret it as unqualified in ConvertFrom
                        // Also need to escape the {} so it doesn't get interpreted as MarkupExtension
                        return name.ToString().Replace("{", "{{").Replace("}", "}}");
                    }
                }
            }
            else
            {
                return XNameTypeConverterHelper.ConvertTo(context, value, destinationType) ?? 
                    base.ConvertTo(context, culture, value, destinationType);
            }
        }

        bool IsQualifiedName(string name)
        {
            return (name.IndexOf(':') >= 1) || (name.Length > 0 && name[0] == '{');
        }
    }
}
