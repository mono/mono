//
// System.ComponentModel.TypeListConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Collections;
using System.Globalization;

namespace System.ComponentModel
{
	public abstract class TypeListConverter : TypeConverter
	{
		private Type[] types;

		protected TypeListConverter (Type[] types)
		{
			this.types = types;
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
			// LAMESPEC also it delivers true for CanConvertFrom (string)
			// it fails in the actual conversion (MS implementation)
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture,
						  object value, Type destinationType)
		{
			if (destinationType == typeof (string) && value != null && value.GetType() == typeof (Type)) {
				return ((Type) value).ToString();
			}
			// LAMESPEC MS throws InvalidCastException here
			throw new InvalidCastException("Cannot cast to System.Type");
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (types);
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

