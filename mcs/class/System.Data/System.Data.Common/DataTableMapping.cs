//
// System.Data.Common.DataTableMapping.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Contains a description of a mapped relationship between a source table and a DataTable. This class is used by a DataAdapter when populating a DataSet.
	/// </summary>
	public sealed class DataTableMapping : MarshalByRefObject, ITableMapping, ICloneable
	{
		#region Fields

		string sourceTable;
		string dataSetTable;
		DataColumnMappingCollection columnMappings;

		#endregion

		#region Constructors

		public DataTableMapping () 
		{
			dataSetTable = String.Empty;
			sourceTable = String.Empty;
			columnMappings = new DataColumnMappingCollection ();
		}

		public DataTableMapping (string sourceTable, string dataSetTable) 
			: this ()
		{
			this.sourceTable = sourceTable;
			this.dataSetTable = dataSetTable;
		}
		
		public DataTableMapping (string sourceTable, string dataSetTable, DataColumnMapping[] columnMappings) 
			: this (sourceTable, dataSetTable)
		{
			this.columnMappings.AddRange (columnMappings);
		}

		#endregion

		#region Properties

		public DataColumnMappingCollection ColumnMappings {
			get { return columnMappings; }
		}

		public string DataSetTable {
			get { return dataSetTable; } 
			set { dataSetTable = value; }
		}

		public string SourceTable {
			get { return sourceTable; }
			set { sourceTable = value; }
		}

		IColumnMappingCollection ITableMapping.ColumnMappings {
			get { return ColumnMappings; }
		}
	
		#endregion

		#region Methods

		public DataColumnMapping GetColumnMappingBySchemaAction (string sourceColumn, MissingMappingAction mappingAction) 
		{
			return DataColumnMappingCollection.GetColumnMappingBySchemaAction (columnMappings, sourceColumn, mappingAction);
		}

		[MonoTODO]
		public DataTable GetDataTableBySchemaAction (DataSet dataSet, MissingSchemaAction schemaAction) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return SourceTable; 
		}
		
		#endregion
	}
}
