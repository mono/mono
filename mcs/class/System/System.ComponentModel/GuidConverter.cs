//
// System.ComponentModel.GuidConverter
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System.Globalization;

namespace System.ComponentModel
{
	public class GuidConverter : TypeConverter
	{

		public GuidConverter()
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

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string GuidString = (string) value;
				try {
					return new Guid (GuidString);
				} catch {
					throw new FormatException (GuidString + "is not a valid GUID.");
				}
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string) && value != null &&value.GetType() == typeof (Guid))
				// LAMESPEC MS seems to always parse "D" type
				return ((Guid) value).ToString("D");
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
