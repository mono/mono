//
// System.ComponentModel.BaseNumberConverter.cs
//
// Authors:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/2003 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;
using System.Globalization;

namespace System.ComponentModel
{
	public abstract class BaseNumberConverter : TypeConverter
	{

		protected Type InnerType;

		protected BaseNumberConverter()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string)) 
			return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
		{
			if (t == typeof (string))
				return true;

			return base.CanConvertTo (context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType() == typeof (string)) {
				try {
					return Convert.ChangeType (value, InnerType, culture.NumberFormat);
				} catch (Exception e) {
					// LAMESPEC MS just seems to pass the internal Exception on to the user
					// so it throws a pure Exception here. We should probably throw a 
					// ArgumentException or something like that
					throw e;
				}
			}

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
						 object value, Type destinationType)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (destinationType == typeof (string) && value.GetType() == InnerType)
				return Convert.ChangeType (value, typeof (string), culture.NumberFormat);

			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}

