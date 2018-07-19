//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.XamlIntegration
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Xaml;
    using System.Xml.Linq;
      
    internal static class XNameTypeConverterHelper
    {
        public static bool CanConvertFrom(Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public static object ConvertFrom(ITypeDescriptorContext context, object value)
        {
            return XNameTypeConverterHelper.ConvertFromHelper(context, value);
        }
        public static bool CanConvertTo(Type destinationType)
        {
            return destinationType == typeof(string);
        }
        public static object ConvertTo(ITypeDescriptorContext context, object value, Type destinationType)
        {
            return XNameTypeConverterHelper.ConvertToHelper(context, value, destinationType);
        }

        internal static object ConvertFromHelper(ITypeDescriptorContext context, object value)
        {
            if (value == null)
            {
                return null;
            }

            String stringValue = value as String;
            if (stringValue == null)
            {
                return null;
            }

            stringValue = stringValue.Trim();
            if (stringValue == String.Empty)
            {
                return null;
            }

            IXamlNamespaceResolver resolver =
                context.GetService(typeof(IXamlNamespaceResolver)) as IXamlNamespaceResolver;
            if (resolver == null)
            {
                return null;
            }

            if (stringValue[0] == '{')
            {
                return XName.Get(stringValue);
            }

            int indexOfColon = stringValue.IndexOf(':');
            string prefix, localName;
            if (indexOfColon >= 0)
            {
                prefix = stringValue.Substring(0, indexOfColon);
                localName = stringValue.Substring(indexOfColon + 1);
            }
            else
            {
                prefix = string.Empty;
                localName = stringValue;
            }

            string ns = resolver.GetNamespace(prefix);
            if (ns == null)
            {
                throw FxTrace.Exception.AsError(new FormatException(SRCore.CouldNotResolveNamespacePrefix(prefix)));                
            }

            return XName.Get(localName, ns);
        }

        internal static object ConvertToHelper(ITypeDescriptorContext context, object value, Type destinationType)
        {
            XName name = value as XName;
            if (destinationType == typeof(string) && name != null)
            {
                if (context != null)
                {
                    var lookupPrefix = (INamespacePrefixLookup)context.GetService(typeof(INamespacePrefixLookup));
                    if (lookupPrefix != null)
                    {
                        string prefix = lookupPrefix.LookupPrefix(name.Namespace.NamespaceName);
                        if (String.IsNullOrEmpty(prefix))
                        {
                            // Default namespace is in scope
                            //
                            return name.LocalName;
                        }
                        else
                        {
                            return prefix + ":" + name.LocalName;
                        }
                    }
                }
            }
            return null;
        }
    }
}
