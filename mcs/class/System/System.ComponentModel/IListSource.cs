//
// System.ComponentModel.IListSource.cs
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
	/// Provides functionality to an object to return a list that can be bound to a data source.
	/// </summary>
	public interface IListSource
	{
		IList GetList ();
		
		bool ContainsListCollection { get; }
	}
}
