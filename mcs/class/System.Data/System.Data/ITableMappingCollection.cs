//
// System.Data.ITableMappingCollection.cs
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
	/// Contains a collection of TableMapping objects, and is implemented by the DataTableMappingCollection, which is used in common by .NET data providers.
	/// </summary>
	public interface ITableMappingCollection : IList, ICollection, IEnumerable
	{
		ITableMapping Add(string sourceTableName, string dataSetTableName);
		
		bool Contains(string sourceTableName);
		
		ITableMapping GetByDataSetTable(string dataSetTableName);
		
		int IndexOf(string sourceTableName);
		
		void RemoveAt(string sourceTableName);
		
		object this[string index]{get; set;}
	}
}
