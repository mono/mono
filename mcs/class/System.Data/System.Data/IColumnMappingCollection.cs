//
// System.Data.IColumnMappingCollection.cs
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
	/// Contains a collection of ColumnMapping objects, and is implemented by the DataColumnMappingCollection, which is used in common by .NET data providers.
	/// </summary>
	public interface IColumnMappingCollection : IList, ICollection, IEnumerable
	{
		IColumnMapping Add(string sourceColumnName, string dataSetColumnName);

		bool Contains(string sourceColumnName);

		IColumnMapping GetByDataSetColumn(string dataSetColumnName);

		int IndexOf(string sourceColumnName);

		void RemoveAt(string sourceColumnName);
		
		object this[string index]
		{
			get;
			set;
		}
	}
}
