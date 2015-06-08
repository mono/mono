//
// System.Data.Common.DataTableMapping.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
#if NET_2_0
	[TypeConverterAttribute ("System.Data.Common.DataTableMapping+DataTableMappingConverter, " + Consts.AssemblySystem_Data)]
#else
	[TypeConverterAttribute (typeof (DataTableMappingConverter))]
#endif
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

#if !NET_2_0
		[DataSysDescription ("Individual columns mappings when this table mapping is matched.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataColumnMappingCollection ColumnMappings {
			get { return columnMappings; }
		}

#if !NET_2_0
		[DataSysDescription ("DataTable.TableName")]
#endif
		[DefaultValue ("")]
		public string DataSetTable {
			get { return dataSetTable; } 
			set { dataSetTable = value; }
		}

		IColumnMappingCollection ITableMapping.ColumnMappings {
			get { return ColumnMappings; }
		}
	
#if !NET_2_0
		[DataSysDescription ("The DataTableMapping source table name. This name is case sensitive.")]
#endif
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

#if NET_2_0
		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataColumn GetDataColumn (string sourceColumn, 
						 Type dataType, 
						 DataTable dataTable, 
						 MissingMappingAction mappingAction, 
						 MissingSchemaAction schemaAction)
		{
			throw new NotImplementedException ();
		}
#endif

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
			DataColumnMapping [] arr = new DataColumnMapping [columnMappings.Count];
			columnMappings.CopyTo (arr, 0);
			return new DataTableMapping (SourceTable, DataSetTable, arr);
		}

		public override string ToString ()
		{
			return SourceTable; 
		}
		
		#endregion // Methods
	}
}
