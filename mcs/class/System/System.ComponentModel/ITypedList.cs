//
// System.ComponentModel.ITypedList.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Collections;

namespace System.ComponentModel
{
	/// <summary>
	/// Provides functionality to discover the schema for a bindable list, where the properties available for binding differ from the public properties of the object to bind to. For instance, using a DataView object that represents a customer table, you want to bind to the properties on the customer object that the DataView represents, not the properties of the DataView.
	/// </summary>
	public interface ITypedList
	{
		PropertyDescriptorCollection GetItemProperties (
			PropertyDescriptor[] listAccessors);

		string GetListName (PropertyDescriptor[] listAccessors);
	}
}
