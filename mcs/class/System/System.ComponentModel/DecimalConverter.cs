//
// System.ComponentModel.DecimalConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Globalization;

namespace System.ComponentModel
{
        public class DecimalConverter : BaseNumberConverter
	{
		[MonoTODO]
		public DecimalConverter()
		{
		}

		[MonoTODO]
		public override bool CanConvertTo (ITypeDescriptorContext context,
						   Type destinationType)
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
		~DecimalConverter()
		{
		}
	}
}
