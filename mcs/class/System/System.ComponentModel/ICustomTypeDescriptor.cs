//
// System.ComponentModel.ICustomTypeDescriptor.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//

namespace System.ComponentModel
{
	public interface ICustomTypeDescriptor
	{
		AttributeCollection GetAttributes();

		string GetClassName();

		string GetComponentName();

		TypeConverter GetConverter();

		EventDescriptor GetDefaultEvent();

		PropertyDescriptor GetDefaultProperty();

		object GetEditor(Type editorBaseType);

		EventDescriptorCollection GetEvents();

		EventDescriptorCollection GetEvents(Attribute[] arr);

		PropertyDescriptorCollection GetProperties();

		PropertyDescriptorCollection GetProperties(Attribute[] arr);

		object GetPropertyOwner(PropertyDescriptor pd);
	}
}
