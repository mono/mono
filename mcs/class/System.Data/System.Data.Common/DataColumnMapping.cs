//
// System.Data.Common.DataColumnMapping
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
//

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	[TypeConverterAttribute ("System.Data.Common.DataColumnMappingConverter, "+ Consts.AssemblySystem_Data)]
	public sealed class DataColumnMapping : MarshalByRefObject, IColumnMapping, ICloneable
	{
		#region Fields

		string sourceColumn;
		string dataSetColumn;

		#endregion // Fields

		#region Constructors
		
		public DataColumnMapping () 
		{
			sourceColumn = String.Empty;
			dataSetColumn = String.Empty;
		}

		public DataColumnMapping (string sourceColumn, string dataSetColumn) 
		{
			this.sourceColumn = sourceColumn;
			this.dataSetColumn = dataSetColumn;
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("DataColumn.ColumnName")]
		[DefaultValue ("")]
		public string DataSetColumn {
			get { return dataSetColumn; }
			set { dataSetColumn = value; }
		}

		[DataSysDescription ("Source column name - case sensitive.")]
		[DefaultValue ("")]
		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}

		#endregion // Properties

		#region Methods

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataColumn GetDataColumnBySchemaAction (DataTable dataTable, Type dataType, MissingSchemaAction schemaAction) 
		{
			if (dataTable.Columns.Contains (dataSetColumn))
				return dataTable.Columns [dataSetColumn];
			if (schemaAction == MissingSchemaAction.Ignore)
				return null;
			if (schemaAction == MissingSchemaAction.Error)
				throw new InvalidOperationException (String.Format ("Missing the DataColumn '{0}' in the DataTable '{1}' for the SourceColumn '{2}'", DataSetColumn, dataTable.TableName, SourceColumn));
			return new DataColumn (dataSetColumn, dataType);
		}

#if NET_1_2
		[MonoTODO]
		public static DataColumn GetDataColumnBySchemaAction (string sourceColumn, string dataSetColumn, DataTable dataTable, Type dataType, MissingSchemaAction schemaAction)
		{
			throw new NotImplementedException ();
		}
#endif

		object ICloneable.Clone ()
		{
			return new DataColumnMapping (SourceColumn, DataSetColumn);
		}

		public override string ToString ()
		{
			return SourceColumn; 
		}

		#endregion // Methods
	}
}
