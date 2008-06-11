//
// System.ComponentModel.DateTimeConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.ComponentModel
{
	public class DateTimeConverter : TypeConverter
	{
		public DateTimeConverter()
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
			if (destinationType == typeof (InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string) {
				string DateString = (string) value;
				try {
					if (DateString != null && DateString.Trim ().Length == 0) {
						return DateTime.MinValue;
					} else if (culture == null) {
						return DateTime.Parse (DateString);
					} else {
						DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));
						return DateTime.Parse (DateString, info);
					}
				} catch {
					throw new FormatException (DateString + " is not a valid DateTime value.");
				}
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (value is DateTime) {
				DateTime datetime = (DateTime) value;
				if (destinationType == typeof (string)) {
					if (culture == null)
						culture = CultureInfo.CurrentCulture;

					if (datetime == DateTime.MinValue)
						return string.Empty;

					DateTimeFormatInfo info = (DateTimeFormatInfo) culture.GetFormat (typeof (DateTimeFormatInfo));

					if (culture == CultureInfo.InvariantCulture) {
						if (datetime.Equals (datetime.Date)) {
							return datetime.ToString ("yyyy-MM-dd", culture);
						}
						return datetime.ToString (culture);
					} else {
						if (datetime == datetime.Date) {
							return datetime.ToString (info.ShortDatePattern, culture);
						} else {
							return datetime.ToString (info.ShortDatePattern + " " + 
								info.ShortTimePattern, culture);
						}
					}
				} else if (destinationType == typeof (InstanceDescriptor)) {
					ConstructorInfo ctor = typeof(DateTime).GetConstructor (new Type[] {typeof(long)});
					return new InstanceDescriptor (ctor, new object[] { datetime.Ticks });
				}
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
