//
// System.Data.Common.DataTableMapping.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public sealed class DataTableMapping : MarshalByRefObject, ITableMapping, ICloneable
	{
		#region Fields

		string sourceTable;
		string dataSetTable;
		DataColumnMappingCollection columnMappings;

		#endregion // Fields

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

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("Individual columns mappings when this table mapping is matched.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataColumnMappingCollection ColumnMappings {
			get { return columnMappings; }
		}

		[DataSysDescription ("DataTable.TableName")]
		[DefaultValue ("")]
		public string DataSetTable {
			get { return dataSetTable; } 
			set { dataSetTable = value; }
		}

		IColumnMappingCollection ITableMapping.ColumnMappings {
			get { return ColumnMappings; }
		}
	
		[DataSysDescription ("The DataTableMapping source table name. This name is case sensitive.")]
		[DefaultValue ("")]
		public string SourceTable {
			get { return sourceTable; }
			set { sourceTable = value; }
		}

		#endregion // Properties

		#region Methods

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataColumnMapping GetColumnMappingBySchemaAction (string sourceColumn, MissingMappingAction mappingAction) 
		{
			return DataColumnMappingCollection.GetColumnMappingBySchemaAction (columnMappings, sourceColumn, mappingAction);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataTable GetDataTableBySchemaAction (DataSet dataSet, MissingSchemaAction schemaAction) 
		{
			if (dataSet.Tables.Contains (DataSetTable))
				return dataSet.Tables [DataSetTable];
			if (schemaAction == MissingSchemaAction.Ignore)
				return null;
			if (schemaAction == MissingSchemaAction.Error)
				throw new InvalidOperationException (String.Format ("Missing the '{0} DataTable for the '{1}' SourceTable", DataSetTable, SourceTable));
			return new DataTable (DataSetTable);
		}

		object ICloneable.Clone ()
		{
			return new DataTableMapping (SourceTable, DataSetTable);
		}

		public override string ToString ()
		{
			return SourceTable; 
		}
		
		#endregion // Methods
	}
}
