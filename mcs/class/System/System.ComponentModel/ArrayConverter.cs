//
// System.ComponentModel.ArrayConverter
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
	public class ArrayConverter : CollectionConverter
	{
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (destinationType == typeof (string) && (value is Array))
				return "(Array)";

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value, Attribute[] attributes)
		{
			// LAMESPEC MS seems to always parse an PropertyDescriptorCollection without any PropertyDescriptors
			return PropertyDescriptorCollection.Empty;
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}

