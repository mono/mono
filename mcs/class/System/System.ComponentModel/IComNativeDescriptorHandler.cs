//
// System.ComponentModel.IComNativeDescriptorHandler.cs
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.ComponentModel
{
	[Obsolete ("Use TypeDescriptionProvider and TypeDescriptor.ComObjectType instead")]
	public interface IComNativeDescriptorHandler
	{
		AttributeCollection GetAttributes(object component);

		string GetClassName(object component);

		TypeConverter GetConverter(object component);

		EventDescriptor GetDefaultEvent(object component);

		PropertyDescriptor GetDefaultProperty(object component);

		object GetEditor(object component, Type baseEditorType);

		EventDescriptorCollection GetEvents(object component);

		EventDescriptorCollection GetEvents(object component, Attribute[] attributes);

		string GetName(object component);

		PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes);

		object GetPropertyValue(object component, int dispid, ref bool success);

		object GetPropertyValue(object component, string propertyName, ref bool success);
	}
}

