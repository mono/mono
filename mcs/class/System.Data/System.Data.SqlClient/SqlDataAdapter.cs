//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//	 Veerapuram Varadhan  (vvaradhan@novell.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
//
// Copyright (C) 2004, 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient {
	[DefaultEvent ("RowUpdated")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterToolboxItem, "+ Consts.AssemblyMicrosoft_VSDesigner)]

	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
	{

#region Copy from old DataColumn
		internal static bool CanAutoIncrement (Type type)
		{
			switch (Type.GetTypeCode (type)) {
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
					return true;
			}

			return false;
		}
#endregion

#region Copy from old DataAdapter

		private const string DefaultSourceColumnName = "Column";

		internal FillErrorEventArgs CreateFillErrorEvent (DataTable dataTable, object[] values, Exception e)
		{
			FillErrorEventArgs args = new FillErrorEventArgs (dataTable, values);
			args.Errors = e;
			args.Continue = false;
			return args;
		}

		internal void OnFillErrorInternal (FillErrorEventArgs value)
		{
			OnFillError (value);
		}

		// this method builds the schema for a given datatable. it returns a int array with 
		// "array[ordinal of datatable column] == index of source column in data reader".
		// each column in the datatable has a mapping to a specific column in the datareader,
		// the int array represents this match.
		internal int[] BuildSchema (IDataReader reader, DataTable table, SchemaType schemaType)
		{
			return BuildSchema (reader, table, schemaType, MissingSchemaAction,
					    MissingMappingAction, TableMappings);
		}

		/// <summary>
		///     Creates or Modifies the schema of the given DataTable based on the schema of
		///     the reader and the arguments passed.
		/// </summary>
		internal static int[] BuildSchema (IDataReader reader, DataTable table,
                                                   SchemaType schemaType,
                                                   MissingSchemaAction missingSchAction,
                                                   MissingMappingAction missingMapAction,
                                                   DataTableMappingCollection dtMapping
                                                   )
		{
			int readerIndex = 0;
			// FIXME : this fails if query has fewer columns than a table
			int[] mapping = new int[table.Columns.Count]; // mapping the reader indexes to the datatable indexes
			
			for(int i=0; i < mapping.Length; i++) {
				mapping[i] = -1;
			}
			
			ArrayList primaryKey = new ArrayList ();
			ArrayList sourceColumns = new ArrayList ();
			bool createPrimaryKey = true;
			
			DataTable schemaTable = reader.GetSchemaTable ();

			DataColumn ColumnNameCol =  schemaTable.Columns["ColumnName"];
			DataColumn DataTypeCol = schemaTable.Columns["DataType"];
			DataColumn IsAutoIncrementCol = schemaTable.Columns["IsAutoIncrement"];
			DataColumn AllowDBNullCol = schemaTable.Columns["AllowDBNull"];
			DataColumn IsReadOnlyCol = schemaTable.Columns["IsReadOnly"];
			DataColumn IsKeyCol = schemaTable.Columns["IsKey"];
			DataColumn IsUniqueCol = schemaTable.Columns["IsUnique"];
			DataColumn ColumnSizeCol = schemaTable.Columns["ColumnSize"];

			foreach (DataRow schemaRow in schemaTable.Rows) {
				// generate a unique column name in the source table.
				string sourceColumnName;
				string realSourceColumnName ;
				if (ColumnNameCol == null || schemaRow.IsNull(ColumnNameCol) ||
				    (string)schemaRow [ColumnNameCol] == String.Empty) {
					sourceColumnName = DefaultSourceColumnName;
					realSourceColumnName = DefaultSourceColumnName + "1";
				} else {
					sourceColumnName = (string) schemaRow [ColumnNameCol];
					realSourceColumnName = sourceColumnName;
				}

				for (int i = 1; sourceColumns.Contains (realSourceColumnName); i += 1)
					realSourceColumnName = String.Format ("{0}{1}", sourceColumnName, i);
				sourceColumns.Add(realSourceColumnName);

				// generate DataSetColumnName from DataTableMapping, if any
				DataTableMapping tableMapping = null;

				//FIXME : The sourcetable name shud get passed as a parameter.. 
				int index = dtMapping.IndexOfDataSetTable (table.TableName);
				string srcTable = (index != -1 ? dtMapping[index].SourceTable : table.TableName);
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (dtMapping, ADP.IsEmpty (srcTable) ? " " : srcTable, table.TableName, missingMapAction); 
				if (tableMapping != null) {
					table.TableName = tableMapping.DataSetTable;
					// check to see if the column mapping exists
					DataColumnMapping columnMapping = DataColumnMappingCollection.GetColumnMappingBySchemaAction(tableMapping.ColumnMappings, realSourceColumnName, missingMapAction);
					if (columnMapping != null) {
						Type columnType = schemaRow[DataTypeCol] as Type;
						DataColumn col = columnType != null ? columnMapping.GetDataColumnBySchemaAction(
						                                                                                table ,
						                                                                                columnType,
						                                                                                missingSchAction) : null;

						if (col != null) {
							// if the column is not in the table - add it.
							if (table.Columns.IndexOf(col) == -1) {
								if (missingSchAction == MissingSchemaAction.Add 
								    || missingSchAction == MissingSchemaAction.AddWithKey)
									table.Columns.Add(col);

								int[] tmp = new int[mapping.Length + 1];
								Array.Copy(mapping,0,tmp,0,col.Ordinal);
								Array.Copy(mapping,col.Ordinal,tmp,col.Ordinal + 1,mapping.Length - col.Ordinal);
								mapping = tmp;
							}

							if (missingSchAction == MissingSchemaAction.AddWithKey) {
								object value = (AllowDBNullCol != null) ? schemaRow[AllowDBNullCol] : null;
								bool allowDBNull = value is bool ? (bool)value : true;

								value = (IsKeyCol != null) ? schemaRow[IsKeyCol] : null;
								bool isKey = value is bool ? (bool)value : false;

								value = (IsAutoIncrementCol != null) ? schemaRow[IsAutoIncrementCol] : null;
								bool isAutoIncrement = value is bool ? (bool)value : false;

								value = (IsReadOnlyCol != null) ? schemaRow[IsReadOnlyCol] : null;
								bool isReadOnly = value is bool ? (bool)value : false;

								value = (IsUniqueCol != null) ? schemaRow[IsUniqueCol] : null;
								bool isUnique = value is bool ? (bool)value : false;
								
								col.AllowDBNull = allowDBNull;
								// fill woth key info
								if (isAutoIncrement && CanAutoIncrement(columnType)) {
									col.AutoIncrement = true;
									if (!allowDBNull)
										col.AllowDBNull = false;
								}

								if (columnType == DbTypes.TypeOfString) {
									col.MaxLength = (ColumnSizeCol != null) ? (int)schemaRow[ColumnSizeCol] : 0;
								}

								if (isReadOnly)
									col.ReadOnly = true;
									
								if (!allowDBNull && (!isReadOnly || isKey))
									col.AllowDBNull = false;
								if (isUnique && !isKey && !columnType.IsArray) {
									col.Unique = true;
									if (!allowDBNull)
										col.AllowDBNull = false;
								}
								
								// This might not be set by all DataProviders
								bool isHidden = false;
								if (schemaTable.Columns.Contains ("IsHidden")) {
									value = schemaRow["IsHidden"];
									isHidden = ((value is bool) ? (bool)value : false);
								}

								if (isKey && !isHidden) {
									primaryKey.Add (col);
									if (allowDBNull)
										createPrimaryKey = false;
								}
							}
							// add the ordinal of the column as a key and the index of the column in the datareader as a value.
							mapping[col.Ordinal] = readerIndex++;
						}
					}
				}
			}
			if (primaryKey.Count > 0) {
				DataColumn[] colKey = (DataColumn[])(primaryKey.ToArray(typeof (DataColumn)));
				if (createPrimaryKey)
					table.PrimaryKey = colKey;
				else {
					UniqueConstraint uConstraint = new UniqueConstraint(colKey);
					for (int i = 0; i < table.Constraints.Count; i++) {
						if (table.Constraints[i].Equals(uConstraint)) {
							uConstraint = null;
							break;
						}
					}

					if (uConstraint != null)
						table.Constraints.Add(uConstraint);
				}
			}
			return mapping;
		}

		internal int FillInternal (DataTable dataTable, IDataReader dataReader)
		{
			if (dataReader.FieldCount == 0) {
				dataReader.Close ();
				return 0;
			}
			
			int count = 0;

			try {
				string tableName = SetupSchema (SchemaType.Mapped, dataTable.TableName);
				if (tableName != null) {
					dataTable.TableName = tableName;
					FillTable (dataTable, dataReader, 0, 0, ref count);
				}
			} finally {
				dataReader.Close ();
			}

			return count;
		}

		internal bool FillTable (DataTable dataTable, IDataReader dataReader, int startRecord, int maxRecords, ref int counter)
		{
			if (dataReader.FieldCount == 0)
				return false;

			int counterStart = counter;

			int[] mapping = BuildSchema (dataReader, dataTable, SchemaType.Mapped);
			
			int [] sortedMapping = new int [mapping.Length];
			int length = sortedMapping.Length;
			for (int i = 0; i < sortedMapping.Length; i++) {
				if (mapping [i] >= 0)
					sortedMapping [mapping [i]] = i;
				else
					sortedMapping [--length] = i;
			}

			for (int i = 0; i < startRecord; i++) {
				dataReader.Read ();
			}

			dataTable.BeginLoadData ();
			object [] values = new object [length];
			while (dataReader.Read () && (maxRecords == 0 || (counter - counterStart) < maxRecords)) {
				try {
					for (int iColumn = 0; iColumn < values.Length; iColumn++)
						values [iColumn] = dataReader [iColumn];
					dataTable.LoadDataRow (values, AcceptChangesDuringFill);
					counter++;
				}
				catch (Exception e) {
					object[] readerArray = new object [dataReader.FieldCount];
					object[] tableArray = new object [mapping.Length];
					// we get the values from the datareader
					dataReader.GetValues (readerArray);
					// copy from datareader columns to table columns according to given mapping
					for (int i = 0; i < mapping.Length; i++) {
						if (mapping [i] >= 0) {
							tableArray [i] = readerArray [mapping [i]];
						}
					}
					FillErrorEventArgs args = CreateFillErrorEvent (dataTable, tableArray, e);
					OnFillErrorInternal (args);

					// if args.Continue is not set to true or if a handler is not set, rethrow the error..
					if(!args.Continue)
						throw e;
				}
			}
			dataTable.EndLoadData ();
			return true;
		}

		internal string SetupSchema (SchemaType schemaType, string sourceTableName)
		{
			DataTableMapping tableMapping = null;

			if (schemaType == SchemaType.Mapped) {
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, sourceTableName, sourceTableName, MissingMappingAction);
				if (tableMapping != null)
					return tableMapping.DataSetTable;
				return null;
			} else
				return sourceTableName;
		}
#endregion

		#region Fields

		int updateBatchSize;
		#endregion

		#region Constructors
		
		public SqlDataAdapter () : this ((SqlCommand) null)
		{
		}

		public SqlDataAdapter (SqlCommand selectCommand) 
		{
			SelectCommand = selectCommand;
			UpdateBatchSize = 1;
		}

		public SqlDataAdapter (string selectCommandText, SqlConnection selectConnection) 
			: this (new SqlCommand (selectCommandText, selectConnection))
		{
		}

		public SqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand DeleteCommand { get; set; }

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand InsertCommand { get; set; }

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand SelectCommand { get; set; }

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new  SqlCommand UpdateCommand { get; set; }
		
		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { SelectCommand = (SqlCommand) value; }
		}
		
		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (SqlCommand) value; }
		}
		
		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (SqlCommand) value; }
		}
		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (SqlCommand) value; }
		}

		public override int UpdateBatchSize {
			get { return updateBatchSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("UpdateBatchSize");
				updateBatchSize = value; 
			}
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (SqlRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (SqlRowUpdatingEventArgs) value);
		}

		[MonoTODO]
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}

		// All the batch methods, should be implemented, if supported,
		// by individual providers 

		[MonoTODO]
		protected override int AddToBatch (IDbCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ClearBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int ExecuteBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IDataParameter GetBatchedParameter (int commandIdentifier, int  parameterIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void InitializeBatching ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void TerminateBatching ()
		{
			throw new NotImplementedException ();
		}
		#endregion // Methods

		#region Events and Delegates

		public event SqlRowUpdatedEventHandler RowUpdated;

		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates
	}
}
