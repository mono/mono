//
// System.ComponentModel.GuidConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Globalization;

namespace System.ComponentModel
{
	public class GuidConverter : TypeConverter
	{
		[MonoTODO]
		public GuidConverter()
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
		~GuidConverter()
		{
		}
	}
}
