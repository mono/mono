//
// System.Data.IDataParameterCollection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System.Collections;

namespace System.Data
{
	/// <summary>
	/// Collects all parameters relevant to a Command object and their mappings to DataSet columns, and is implemented by .NET data providers that access data sources.
	/// </summary>
	public interface IDataParameterCollection : IList, ICollection, IEnumerable
	{
		void RemoveAt(string parameterName);
		
		int IndexOf(string parameterName);
		
		bool Contains(string parameterName);

		object this[string parameterName]{get; set;}
	}
}
