//
// System.ComponentModel.TypeListConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Collections;
using System.Globalization;

namespace System.ComponentModel
{
        public abstract class TypeListConverter : TypeConverter
	{
		[MonoTODO]
		protected TypeListConverter (Type[] types)
		{
		}

		[MonoTODO]
		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool CanConvertTo (ITypeDescriptorContext context,
						   Type destinationType)
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
		~TypeListConverter()
		{
		}
	}
}
