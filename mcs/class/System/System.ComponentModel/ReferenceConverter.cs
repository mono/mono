//
// System.ComponentModel.ReferenceConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Globalization;

namespace System.ComponentModel
{
	public class ReferenceConverter : TypeConverter
	{
		[MonoTODO]
		public ReferenceConverter (Type type)
		{
		}

		[MonoTODO]
		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual bool IsValueAllowed (ITypeDescriptorContext context, object value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ReferenceConverter()
		{
		}
	}
}
