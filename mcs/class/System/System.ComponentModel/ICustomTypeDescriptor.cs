//
// System.ComponentModel.ICustomTypeDescriptor.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
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
