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

using System.Globalization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

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
			if (destinationType == typeof (string))
				return true;
			if (destinationType == typeof (InstanceDescriptor))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string DateString = (String) value;
				try {
					if (culture == null)
						// try to parse string
						return DateTime.Parse (DateString);
					else
						// try to parse string
						return DateTime.Parse (DateString, culture.DateTimeFormat);
				} catch {
					throw new FormatException (DateString + "is not a valid DateTime value.");
				}
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (destinationType == typeof (string) && value != null && (value is DateTime)) {
				CultureInfo CurrentCulture = culture;
				if (CurrentCulture == null)
					CurrentCulture = CultureInfo.CurrentCulture;
				DateTime ConvertTime = (DateTime) value;

				if (CurrentCulture.Equals(CultureInfo.InvariantCulture)) {
					if (ConvertTime.Equals(ConvertTime.Date))
						return ConvertTime.ToString("yyyy-mm-dd");
					else
						return ConvertTime.ToString ();
				} else {
					if (ConvertTime.Equals(ConvertTime.Date))
						return ConvertTime.ToString (CurrentCulture.DateTimeFormat.ShortDatePattern);
					else
						return ConvertTime.ToString (CurrentCulture.DateTimeFormat.ShortDatePattern + 
							" " + CurrentCulture.DateTimeFormat.ShortTimePattern);
				}
			}
			if (destinationType == typeof (InstanceDescriptor) && value is DateTime) {
				DateTime cval = (DateTime) value;
				ConstructorInfo ctor = typeof(DateTime).GetConstructor (new Type[] {typeof(long)});
				return new InstanceDescriptor (ctor, new object[] {cval.Ticks});
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
