//
// System.ComponentModel.IEditableObject.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//

namespace System.ComponentModel
{
	public interface IEditableObject
	{
		void BeginEdit();

		void CancelEdit();

		void EndEdit();
	}
}
