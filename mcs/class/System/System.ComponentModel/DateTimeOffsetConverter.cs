//
// DateTimeOffsetConverter.cs
//
// Author:
// 	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace System.ComponentModel
{
	public class DateTimeOffsetConverter : TypeConverter
	{
		static readonly string OffsetPattern = "K";
		static readonly string InvariantDatePattern = "yyyy-MM-dd";

		public DateTimeOffsetConverter ()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string) || destinationType == typeof (InstanceDescriptor))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string) {
				string s = ((string) value).Trim ();
				if (s.Length == 0)
					return DateTimeOffset.MinValue;

				DateTimeOffset retval;
				if (culture == null) {
					if (DateTimeOffset.TryParse (s, out retval))
						return retval;
				} else {
					DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
					if (DateTimeOffset.TryParse (s, info, DateTimeStyles.None, out retval))
						return retval;
				}

				throw new FormatException (s + " is not a valid DateTimeOffset value.");
			}

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is DateTimeOffset) {
				DateTimeOffset dt_offset = (DateTimeOffset) value;

				if (destinationType == typeof (string)) {
					if (dt_offset == DateTimeOffset.MinValue)
						return String.Empty;

					if (culture == null)
						culture = CultureInfo.CurrentCulture;

					// InvariantCulture gets special handling.
					if (culture == CultureInfo.InvariantCulture) {
						if (dt_offset.DateTime == dt_offset.Date)
							return dt_offset.ToString (InvariantDatePattern + " " + OffsetPattern);

						return dt_offset.ToString (culture);
					}

					DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
					if (dt_offset.DateTime == dt_offset.Date)
						return dt_offset.ToString (info.ShortDatePattern + " " + OffsetPattern);

					// No need to pass CultureInfo, as we already consumed the proper patterns.
					return dt_offset.ToString (info.ShortDatePattern + " " + info.ShortTimePattern + " " + OffsetPattern, null);
				}

				if (destinationType == typeof (InstanceDescriptor)) {
					ConstructorInfo ctor = typeof (DateTimeOffset).GetConstructor ( GetDateTimeOffsetArgumentTypes ());
					object [] ctor_args = new object [] { dt_offset.Year, dt_offset.Month, dt_offset.Day, 
						dt_offset.Hour, dt_offset.Minute, dt_offset.Second, dt_offset.Millisecond,
						dt_offset.Offset };
					return new InstanceDescriptor (ctor, ctor_args);
				}
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

		static Type [] ctor_argument_types;

		static Type [] GetDateTimeOffsetArgumentTypes ()
		{
			if (ctor_argument_types == null) {
				Type int_type = typeof (int);
				ctor_argument_types = new Type [] { int_type, int_type, int_type, int_type, int_type, int_type, int_type,
					typeof (TimeSpan) };
			}

			return ctor_argument_types;
		}
	}
}

#endif

