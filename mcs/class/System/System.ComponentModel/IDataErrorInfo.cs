//
// System.ComponentModel.IDataErrorInfo.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//

namespace System.ComponentModel
{
	public interface IDataErrorInfo
	{
		string Error { get; }

		string this[string columnName] { get; }
	}
}
