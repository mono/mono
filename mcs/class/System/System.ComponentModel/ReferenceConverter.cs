//
// System.ComponentModel.ReferenceConverter.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Globalization;

namespace System.ComponentModel
{
	public class ReferenceConverter : TypeConverter
	{

		public ReferenceConverter (Type type)
		{
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			if (sourceType == typeof (string)) 
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		[MonoTODO]
		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			// Add implementation
			return base.ConvertFrom(context, culture, value);
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			// Add implementation
			return base.ConvertTo(context, culture, value, destinationType);
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		protected virtual bool IsValueAllowed (ITypeDescriptorContext context, object value)
		{
			return true;
		}
	}
}
