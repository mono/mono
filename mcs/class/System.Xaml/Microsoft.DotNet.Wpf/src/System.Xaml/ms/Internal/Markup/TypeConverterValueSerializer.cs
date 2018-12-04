// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  TypeConverter to ValueSerializer adapter
//
//  Created:   04/28/2005 Microsoft
//

using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xaml;

namespace MS.Internal.Serialization
{
    /// <summary>
    /// The TypeConverter value serializer uses a TypeConverter to implement the translation
    /// to and from a string. The caller of the constructor must ensure the TypeConverter supports
    /// converstion to and from string.
    /// </summary>
    internal sealed class TypeConverterValueSerializer : ValueSerializer
    {
        private TypeConverter converter;

        public TypeConverterValueSerializer(TypeConverter converter)
        {
            this.converter = converter;
        }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return converter.CanConvertTo(context, typeof(string));
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return converter.ConvertToString(context, TypeConverterHelper.InvariantEnglishUS, value);
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            return converter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, value);
        }
    }
}
