//
// System.ComponentModel.DecimalConverter
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
    public class DecimalConverter : BaseNumberConverter
	{
		public DecimalConverter()
		{
		}

		public override bool CanConvertTo (ITypeDescriptorContext context,
			Type destinationType)
		{
			return base.CanConvertTo (context, destinationType);
		}
		
		public override object ConvertTo (ITypeDescriptorContext context,
			CultureInfo culture, object value, Type destinationType)
		{
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
