//
// System.Data.Common.DataAdapter
//
// Author:
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

using System;
using System.Data;
using System.Collections;
using System.ComponentModel;

namespace System.Data.Common
{
	/// <summary>
	/// Represents a set of data commands and a database connection that are used to fill the DataSet and update the data source.
	/// </summary>
	public
#if ONLY_1_1
	abstract
#endif
	class DataAdapter : Component, IDataAdapter
	{
		#region Fields

		private bool acceptChangesDuringFill;
		private bool continueUpdateOnError;
		private MissingMappingAction missingMappingAction;
		private MissingSchemaAction missingSchemaAction;
		private DataTableMappingCollection tableMappings;
		private const string DefaultSourceTableName = "Table";
		private const string DefaultSourceColumnName = "Column";

#if NET_2_0
		private bool acceptChangesDuringUpdate;
		private LoadOption fillLoadOption;
		private bool returnProviderSpecificTypes;
#endif
		#endregion

		#region Constructors

		protected DataAdapter () 
		{
			acceptChangesDuringFill = true;
			continueUpdateOnError = false;
			missingMappingAction = MissingMappingAction.Passthrough;
			missingSchemaAction = MissingSchemaAction.Add;
			tableMappings = new DataTableMappingCollection ();
#if NET_2_0
			acceptChangesDuringUpdate = true;
			fillLoadOption = LoadOption.OverwriteChanges;
			returnProviderSpecificTypes = false;
#endif 
		}

		protected DataAdapter (DataAdapter from)
		{
			AcceptChangesDuringFill = from.AcceptChangesDuringFill;
			ContinueUpdateOnError = from.ContinueUpdateOnError;
			MissingMappingAction = from.MissingMappingAction;
			MissingSchemaAction = from.MissingSchemaAction;

			if (from.tableMappings != null)
				foreach (ICloneable cloneable in from.TableMappings)
					TableMappings.Add (cloneable.Clone ());
#if NET_2_0
			acceptChangesDuringUpdate = from.AcceptChangesDuringUpdate;
			fillLoadOption = from.FillLoadOption;
			returnProviderSpecificTypes = from.ReturnProviderSpecificTypes;
#endif 
		}

		#endregion

		#region Properties

		[DataCategory ("Fill")]
#if !NET_2_0
		[DataSysDescription ("Whether or not Fill will call DataRow.AcceptChanges.")]
#endif
		[DefaultValue (true)]
		public bool AcceptChangesDuringFill {
			get { return acceptChangesDuringFill; }
			set { acceptChangesDuringFill = value; }
		}

#if NET_2_0
		[DefaultValue (true)]
		public bool AcceptChangesDuringUpdate {
			get { return acceptChangesDuringUpdate; }
			set { acceptChangesDuringUpdate = value; }
		}
#endif

		[DataCategory ("Update")]
#if !NET_2_0
		[DataSysDescription ("Whether or not to continue to the next DataRow when the Update events, RowUpdating and RowUpdated, Status is UpdateStatus.ErrorsOccurred.")]
#endif
		[DefaultValue (false)]
		public bool ContinueUpdateOnError {
			get { return continueUpdateOnError; }
			set { continueUpdateOnError = value; }
		}

#if NET_2_0
		[RefreshProperties (RefreshProperties.All)]
		public LoadOption FillLoadOption {
			get { return fillLoadOption; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (LoadOption), value);
				fillLoadOption = value;
		}
		}
#endif

		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		[DataCategory ("Mapping")]
#if !NET_2_0
		[DataSysDescription ("The action taken when a table or column in the TableMappings is missing.")]
#endif
		[DefaultValue (MissingMappingAction.Passthrough)]
		public MissingMappingAction MissingMappingAction {
			get { return missingMappingAction; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (MissingMappingAction), value);
				missingMappingAction = value;
			}
		}

		[DataCategory ("Mapping")]
#if !NET_2_0
		[DataSysDescription ("The action taken when a table or column in the DataSet is missing.")]
#endif
		[DefaultValue (MissingSchemaAction.Add)]
		public MissingSchemaAction MissingSchemaAction {
			get { return missingSchemaAction; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (MissingSchemaAction), value);
				missingSchemaAction = value; 
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		public virtual bool ReturnProviderSpecificTypes {
			get { return returnProviderSpecificTypes; }
			set { returnProviderSpecificTypes = value; }
		}
#endif

		[DataCategory ("Mapping")]
#if !NET_2_0
		[DataSysDescription ("How to map source table to DataSet table.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableMappingCollection TableMappings {
			get { return tableMappings; }
		}

		#endregion

		#region Events

#if NET_2_0
		public event FillErrorEventHandler FillError;
#endif

		#endregion

		#region Methods

		[Obsolete ("Use the protected constructor instead")]
		[MonoTODO]
		protected virtual DataAdapter CloneInternals ()
		{
			throw new NotImplementedException ();
		}

		protected virtual DataTableMappingCollection CreateTableMappings ()
		{
			return new DataTableMappingCollection ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool ShouldSerializeTableMappings ()
		{
			return true;
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
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (dtMapping, srcTable, table.TableName, missingMapAction); 
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
								if (isAutoIncrement && DataColumn.CanAutoIncrement(columnType)) {
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
			while (dataReader.Read () && (maxRecords == 0 || (counter - counterStart) < maxRecords)) {
				try {
					dataTable.LoadDataRow (dataReader, sortedMapping, length, AcceptChangesDuringFill);
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

		internal virtual void OnFillErrorInternal (FillErrorEventArgs value)
		{
#if NET_2_0
			OnFillError (value);
#endif
		}

		internal FillErrorEventArgs CreateFillErrorEvent (DataTable dataTable, object[] values, Exception e)
		{
			FillErrorEventArgs args = new FillErrorEventArgs (dataTable, values);
			args.Errors = e;
			args.Continue = false;
			return args;
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

		internal int FillInternal (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			if (dataSet == null)
				throw new ArgumentNullException ("DataSet");

			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

			DataTable dataTable = null;
			int resultIndex = 0;
			int count = 0;
			
			try {
				string tableName = srcTable;
				do {
					// Non-resultset queries like insert, delete or update aren't processed.
					if (dataReader.FieldCount != -1) {
						tableName = SetupSchema (SchemaType.Mapped, tableName);
						if (tableName != null) {
							
							// check if the table exists in the dataset
							if (dataSet.Tables.Contains (tableName))
								// get the table from the dataset
								dataTable = dataSet.Tables [tableName];
							else {
								// Do not create schema if MissingSchemAction is set to Ignore
								if (this.MissingSchemaAction == MissingSchemaAction.Ignore)
									continue;
								dataTable = dataSet.Tables.Add (tableName);
							}
	
							if (!FillTable (dataTable, dataReader, startRecord, maxRecords, ref count))
								continue;
	
							tableName = String.Format ("{0}{1}", srcTable, ++resultIndex);
	
							startRecord = 0;
							maxRecords = 0;
						}
					}
				} while (dataReader.NextResult ());
			} finally {
				dataReader.Close ();
			}

			return count;
		}

#if NET_2_0
		public virtual int Fill (DataSet dataSet)
		{
			throw new NotSupportedException();
		}

		protected virtual int Fill (DataTable dataTable, IDataReader dataReader)
		{
			return FillInternal (dataTable, dataReader);
		}

		protected virtual int Fill (DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
		{
			int count = 0;
			if (dataReader.IsClosed)
				return 0;

			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

			try {
				foreach (DataTable dataTable in dataTables) {
					string tableName = SetupSchema (SchemaType.Mapped, dataTable.TableName);
					if (tableName != null) {
						dataTable.TableName = tableName;
						FillTable (dataTable, dataReader, 0, 0, ref count);
					}
				}
			} finally {
				dataReader.Close ();
			}

			return count;
		}

		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			return FillInternal (dataSet, srcTable, dataReader, startRecord, maxRecords);
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDataReader dataReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader)
		{
			throw new NotImplementedException ();
		}

		public virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType)
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual IDataParameter[] GetFillParameters ()
		{
			throw new NotImplementedException ();
		}

		protected bool HasTableMappings ()
		{
			return (TableMappings.Count != 0);
		}

		protected virtual void OnFillError (FillErrorEventArgs value)
		{
			if (FillError != null)
				FillError (this, value);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void ResetFillLoadOption ()
		{
			//FIXME: what else ??
			FillLoadOption = LoadOption.OverwriteChanges;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual bool ShouldSerializeAcceptChangesDuringFill ()
		{
			return true;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual bool ShouldSerializeFillLoadOption ()
		{
			return false;
		}

		[MonoTODO]
		public virtual int Update (DataSet dataSet)
		{
			throw new NotImplementedException ();
		}
#else
		public abstract int Fill (DataSet dataSet);
		public abstract DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType);
		public abstract IDataParameter[] GetFillParameters ();
		public abstract int Update (DataSet dataSet);
#endif

		#endregion
		
	}
}
