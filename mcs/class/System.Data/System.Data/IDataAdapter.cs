//
// System.Data.IDataAdapter.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Allows an object to implement a DataAdapter, and represents a set of methods and mapping action-related properties used to fill and refresh a DataSet and update a data source.
	/// </summary>
	public interface IDataAdapter
	{
		int Fill(DataSet dataSet);

		DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType);

		IDataParameter[] GetFillParameters();

		int Update(DataSet dataSet);
		
		MissingMappingAction MissingMappingAction{get;set;}

		MissingSchemaAction MissingSchemaAction{get;set;}

		ITableMappingCollection TableMappings{get;}


	}
}