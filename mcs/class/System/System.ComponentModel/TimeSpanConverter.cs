//
// System.ComponentModel.TimeSpanConverter
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
	public class TimeSpanConverter : TypeConverter
	{

		public TimeSpanConverter()
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
				string TimeSpanString = (string) value;
				try {
					// LAMESPEC Doc says TimeSpan uses Ticks, but MS uses time format:
					// [ws][-][d.]hh:mm:ss[.ff][ws]
					return TimeSpan.Parse (TimeSpanString);
				} catch {
					throw new FormatException (TimeSpanString + "is not valid for a TimeSpan.");
				}
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (value is TimeSpan) {
				TimeSpan timespan = (TimeSpan) value;
				if (destinationType == typeof (string) && value != null) {
					// LAMESPEC Doc says TimeSpan uses Ticks, but MS uses time format
					// [ws][-][d.]hh:mm:ss[.ff][ws]
					return timespan.ToString ();
				}
				if (destinationType == typeof (InstanceDescriptor)) {
					ConstructorInfo ctor = typeof(TimeSpan).GetConstructor (new Type[] {typeof(long)});
					return new InstanceDescriptor (ctor, new object[] { timespan.Ticks });
				}
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
