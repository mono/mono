// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;

using System.Globalization;
using System.Text;
using System.Windows.Markup;

namespace System.Xaml.Replacements
{

    //+--------------------------------------------------------------------------------------
    // 
    //  DateTimeConverter2
    //
    //  This internal class simply wraps the DateTimeValueSerializer, to make it compatible with
    //  internal code that expects a type converter.
    //
    //+--------------------------------------------------------------------------------------
    
    internal class DateTimeConverter2 : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))     
            {
                return true;
            }
            
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return _dateTimeValueSerializer.ConvertFromString( value as string, _valueSerializerContext );
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DateTime)
            {
                return _dateTimeValueSerializer.ConvertToString(value, _valueSerializerContext);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private DateTimeValueSerializer _dateTimeValueSerializer = new DateTimeValueSerializer();
        private IValueSerializerContext _valueSerializerContext = new DateTimeValueSerializerContext();
    }
}
