//
// System.ComponentModel.CharConverter
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System.Globalization;

namespace System.ComponentModel
{
	public class CharConverter : TypeConverter  
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				string Test = (String) value;
				if (Test.Length != 1)
				// LAMESPEC: MS does throw FormatException here
					throw new FormatException ("String has to be exactly one char long");
				return Test[0];
			}
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string))
			if (value != null && value is char)
				return new string ((char) value, 1);

			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

