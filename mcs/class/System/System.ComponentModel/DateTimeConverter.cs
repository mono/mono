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

using System.Globalization;

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
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
