//
// System.ComponentModel.ComponentConverter.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public class ComponentConverter : ReferenceConverter
	{

		public ComponentConverter (Type type) : base (type)
		{
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value,
									    Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties (value, attributes);
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
