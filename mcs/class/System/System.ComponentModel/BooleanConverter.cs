//
// System.ComponentModel.BooleanConverter
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
	public class BooleanConverter : TypeConverter
	{

		public BooleanConverter()
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string Test = ((String) value).ToLower (culture);
				if (Test.Equals ("true"))
					return true;
				else if (Test.Equals ("false"))
					return false;
				else
					throw new FormatException ("No valid boolean value");
			}

			return base.ConvertFrom (context, culture, value);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (new string[2] {"True", "False"} );
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}

