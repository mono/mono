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

using System.Globalization;

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
			if (destinationType == typeof (string) && value != null && value.GetType() == typeof (TimeSpan)) {
				// LAMESPEC Doc says TimeSpan uses Ticks, but MS uses time format
				// [ws][-][d.]hh:mm:ss[.ff][ws]
				return ((TimeSpan) value).ToString();
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
