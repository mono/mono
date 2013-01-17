//
// NullableConverter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//      Ivan N. Zlatev  <contact@i-nz.net>
//
// Copyright (C) 2007 Novell, Inc. http://www.novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Globalization;

namespace System.ComponentModel
{
	public class NullableConverter : TypeConverter
	{
		private Type nullableType;
		private Type underlyingType;
		private TypeConverter underlyingTypeConverter;

		public NullableConverter (Type nullableType)
		{
			if (nullableType == null)
				throw new ArgumentNullException ("nullableType");

			this.nullableType = nullableType;
			underlyingType = Nullable.GetUnderlyingType (nullableType);
			underlyingTypeConverter = TypeDescriptor.GetConverter (underlyingType);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == underlyingType)
				return true;

			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.CanConvertFrom (context, sourceType);

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == underlyingType)
				return true;

			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.CanConvertTo (context, destinationType);

			return base.CanConvertFrom (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			// Because:
			//    1) Nullable<> has an expliciit generic cast operator
			//    2) We are returning an "Object" type here
			// we don't have to bother creating the nullable instance,
			// since the user will have to explicitly cast anyway.
			//
			if (value == null || value.GetType() == underlyingType)
				return value;

			if (value is string && String.IsNullOrEmpty ((string)value))
				return null;

			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.ConvertFrom (context, culture, value);

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			// Explicit cast operator in Nullable when the user casts will take care 
			// of extracting the inner value.
			if (destinationType == underlyingType && value.GetType() == underlyingType)
				return value;

			if (underlyingTypeConverter != null && value != null)
				return underlyingTypeConverter.ConvertTo (context, culture, value, destinationType);

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override object CreateInstance (ITypeDescriptorContext context, IDictionary propertyValues)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.CreateInstance (context, propertyValues);

			return base.CreateInstance (context, propertyValues);
		}

		public override bool GetCreateInstanceSupported (ITypeDescriptorContext context)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.GetCreateInstanceSupported (context);

			return base.GetCreateInstanceSupported (context);
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute [] attributes)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.GetProperties (context, value, attributes);

			return base.GetProperties (context, value, attributes);
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.GetCreateInstanceSupported (context);

			return base.GetCreateInstanceSupported (context);
		}

		public override TypeConverter.StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			// Adds a "null" values to the standard values if supported and available
			//
			if (underlyingTypeConverter != null && 
			    underlyingTypeConverter.GetStandardValuesSupported (context)) {
				TypeConverter.StandardValuesCollection values = underlyingTypeConverter.GetStandardValues (context);
				if (values != null) {
					ArrayList valuesWithNull = new ArrayList (values);
					valuesWithNull.Add (null);
					return new TypeConverter.StandardValuesCollection (valuesWithNull);
				}
			}

			return base.GetStandardValues (context);
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{

			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.GetStandardValuesExclusive (context);

			return base.GetStandardValuesExclusive (context);
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.GetStandardValuesSupported (context);

			return base.GetStandardValuesSupported (context);
		}

		public override bool IsValid (ITypeDescriptorContext context, object value)
		{
			if (underlyingTypeConverter != null)
				return underlyingTypeConverter.IsValid (context, value);

			return base.IsValid (context, value);
		}

		public Type NullableType {
			get { return nullableType; }
		}

		public Type UnderlyingType {
			get { return underlyingType; }
		}

		public TypeConverter UnderlyingTypeConverter {
			get { return underlyingTypeConverter; }
		}
	}
}

