//
// System.ComponentModel.CultureInfoConverter
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
	public class CultureInfoConverter : TypeConverter
	{

		public CultureInfoConverter()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context,
			Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
			Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
			CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string CultureString = (String) value;
				try {
					// try to create a new CultureInfo if form is RFC 1766
					return new CultureInfo (CultureString);
				} catch {
					// try to create a new CultureInfo if form is verbose name
					foreach (CultureInfo CI in CultureInfo.GetCultures (CultureTypes.AllCultures))
					// LAMESPEC MS seems to use EnglishName (culture invariant) - check this
						if (CI.EnglishName.ToString().IndexOf (CultureString) > 0)
							return CI;
				}
				throw new ArgumentException ("Culture incorrect or not available in this environment.", "value");
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string) && value != null && (value is CultureInfo))
				// LAMESPEC MS seems to use EnglishName (culture invariant) - check this
				return ((CultureInfo) value).EnglishName;
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (CultureInfo.GetCultures (CultureTypes.AllCultures));
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

	}
}
