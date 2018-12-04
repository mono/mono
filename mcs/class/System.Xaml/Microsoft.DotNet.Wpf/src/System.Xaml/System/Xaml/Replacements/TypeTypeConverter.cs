// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Xaml.Schema;
using System.Xaml;
using XAML3 = System.Windows.Markup;

namespace System.Xaml.Replacements
{
    /// <summary>
    /// TypeConverter for System.Type
    /// </summary>
    internal class TypeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string typeName = value as string;

            if (null != context && null != typeName)
            {
                var typeResolver = GetService<XAML3.IXamlTypeResolver>(context);

                if (null != typeResolver)
                {
                    Type type = typeResolver.Resolve(typeName);
                    return type;
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
            Type type = value as Type;

            if (context != null && type != null && destinationType == typeof(string))
            {
                string result = ConvertTypeToString(context, type);
                if (result != null)
                {
                    return result;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static string ConvertTypeToString(ITypeDescriptorContext context, Type type)
        {
            var schemaContextProvider = GetService<IXamlSchemaContextProvider>(context);
            if (schemaContextProvider == null)
            {
                return null;
            }
            if (schemaContextProvider.SchemaContext == null)
            {
                return null;
            }
            XamlType xamlType = schemaContextProvider.SchemaContext.GetXamlType(type);
            if (xamlType == null)
            {
                return null;
            }
            return XamlTypeTypeConverter.ConvertXamlTypeToString(context, xamlType);
        }

        private static TService GetService<TService>(ITypeDescriptorContext context) where TService : class
        {
            return context.GetService(typeof(TService)) as TService;
        }
    }
}
