//
// System.ComponentModel.CollectionConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Globalization;

namespace System.ComponentModel
{
	public class CollectionConverter : TypeConverter
	{
		[MonoTODO]
		public CollectionConverter()
		{
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
		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value,
									    Attribute[] attributes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~CollectionConverter()
		{
		}
	}
}
