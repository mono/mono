//
// System.ComponentModel.ComponentConverter
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.ComponentModel
{
	public class ComponentConverter : ReferenceConverter
	{
		[MonoTODO]
		public ComponentConverter (Type type) : base (type)
		{
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
		~ComponentConverter()
		{
		}
	}
}
