//
// System.Data.ITableMapping.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Associates a source table with a table in a DataSet, and is implemented by the DataTableMapping class, which is used in common by .NET data providers.
	/// </summary>
	public interface ITableMapping
	{
		
		IColumnMappingCollection ColumnMappings
		{
			get
			{
			}
		}

		string DataSetTable
		{
			get
			{
			}
			set
			{
			}
		}

		string SourceTable
		{
			get
			{
			}
			set
			{
			}
		}
	}
}