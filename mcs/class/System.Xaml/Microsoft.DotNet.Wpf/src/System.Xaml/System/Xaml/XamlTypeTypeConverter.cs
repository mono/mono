// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Xaml.Schema
{
    public class XamlTypeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string typeName = value as string;

            if (context != null && typeName != null)
            {
                XamlType result = ConvertStringToXamlType(context, typeName);
                if (result != null)
                {
                    return result;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            XamlType xamlType = value as XamlType;

            if (context != null && xamlType != null && destinationType == typeof(string))
            {
                string result = ConvertXamlTypeToString(context, xamlType);
                if (result != null)
                {
                    return result;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        internal static string ConvertXamlTypeToString(ITypeDescriptorContext context, XamlType xamlType)
        {
            var prefixLookup = GetService<INamespacePrefixLookup>(context);
            if (prefixLookup == null)
            {
                return null;
            }
            XamlTypeName typeName = new XamlTypeName(xamlType);
            return typeName.ToString(prefixLookup);
        }

        private static XamlType ConvertStringToXamlType(ITypeDescriptorContext context, string typeName)
        {
            var namespaceResolver = GetService<IXamlNamespaceResolver>(context);
            if (namespaceResolver == null)
            {
                return null;
            }
            XamlTypeName xamlTypeName = XamlTypeName.Parse(typeName, namespaceResolver);
            var schemaContextProvider = GetService<IXamlSchemaContextProvider>(context);
            if (schemaContextProvider == null)
            {
                return null;
            }
            if (schemaContextProvider.SchemaContext == null)
            {
                return null;
            }
            return GetXamlTypeOrUnknown(schemaContextProvider.SchemaContext, xamlTypeName);
        }

        private static TService GetService<TService>(ITypeDescriptorContext context) where TService : class
        {
            return context.GetService(typeof(TService)) as TService;
        }

        private static XamlType GetXamlTypeOrUnknown(XamlSchemaContext schemaContext, XamlTypeName typeName)
        {
            XamlType result = schemaContext.GetXamlType(typeName);
            if (result != null)
            {
                return result;
            }
            XamlType[] typeArgs = null;
            if (typeName.HasTypeArgs)
            {
                typeArgs = new XamlType[typeName.TypeArguments.Count];
                for (int i = 0; i < typeName.TypeArguments.Count; i++)
                {
                    typeArgs[i] = GetXamlTypeOrUnknown(schemaContext, typeName.TypeArguments[i]);
                }
            }
            result = new XamlType(typeName.Namespace, typeName.Name, typeArgs, schemaContext);
            return result;
        }
    }
}
