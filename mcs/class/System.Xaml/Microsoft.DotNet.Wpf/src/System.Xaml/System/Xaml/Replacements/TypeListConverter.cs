// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Xaml.Schema;
using XAML3 = System.Windows.Markup;

namespace System.Xaml.Replacements
{
    // Not sure if this type converter is used at all.
    // we need to either make this a useful type converter or remove the code.
    
    /// <summary>
    /// TypeConverter for System.Type[]
    /// </summary>
    internal class TypeListConverter : TypeConverter
    {
        private static readonly TypeTypeConverter typeTypeConverter = new TypeTypeConverter();
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string typeList = (string)value;
            if (null != context)
            {
                // Consider HashMap(int, int), HashMap(int, int)
                string[] tl = StringHelpers.SplitTypeList(typeList);
                Type[] types = new Type[tl.Length];
                for (int i = 0; i < tl.Length; i++)
                {
                    types[i] = (Type)typeTypeConverter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, tl[i]);
                }
                return types;
            }
            return base.ConvertFrom(context, culture, value);
        }

        
    }

    internal static class StringHelpers
    {
        // split top level types and strip out whitespace
        public static string[] SplitTypeList(string typeList)
        {
            return null;
        }
    }
}
