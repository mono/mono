//
// System.ComponentModel.IComNativeDescriptorHandler.cs
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	public interface IComNativeDescriptorHandler
	{
		AttributeCollection GetAttributes (object component);
		string GetClassName (object component);
		TypeConverter GetConverter (object component);
		EventDescriptor GetDefaultEvent (object component);
		PropertyDescriptor GetDefaultProperty (object component);
		object GetEditor (object component, Type baseEditorType);
		EventDescriptorCollection GetEvents (object component);
		EventDescriptorCollection GetEvents (object component, Attribute [] attributes);
		string GetName (object component);
		PropertyDescriptorCollection GetProperties (object component, Attribute [] attributes);
		object GetPropertyValue (object component, int dispid, ref bool success);
		object GetPropertyValue (object component, string propertyName, ref bool success);
	}
}

