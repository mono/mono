//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Sureshkumar T <tsureshkumar@novell.com>
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.Common {
#if NET_2_0
	public abstract class DbDataAdapter : DataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
#else
	public abstract class DbDataAdapter : DataAdapter, ICloneable
#endif
	{
		#region Fields

		public const string DefaultSourceTableName = "Table";
		const string DefaultSourceColumnName = "Column";

		#endregion // Fields
		
		#region Constructors

		protected DbDataAdapter() 
		{
		}

		[MonoTODO]
		protected DbDataAdapter(DbDataAdapter adapter) : base(adapter)
		{
		}

		#endregion // Fields

		#region Properties

#if NET_2_0
		[MonoTODO]
		protected virtual IDbConnection BaseConnection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public IDbConnection Connection { 
			get { return BaseConnection; }
			set { BaseConnection = value; }
		}
#endif


#if NET_2_0
		protected internal CommandBehavior FillCommandBehavior {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif


#if NET_2_0
		[MonoTODO]
		protected virtual IDbCommand this [[Optional] StatementType statementType] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual DbProviderFactory ProviderFactory {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDbCommand IDbDataAdapter.SelectCommand {
			get { return ((IDbDataAdapter) this).SelectCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		IDbCommand IDbDataAdapter.UpdateCommand{
			get { return ((IDbDataAdapter) this).UpdateCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		IDbCommand IDbDataAdapter.DeleteCommand{
			get { return ((IDbDataAdapter) this).DeleteCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		IDbCommand IDbDataAdapter.InsertCommand{
			get { return ((IDbDataAdapter) this).InsertCommand; }
			set { throw new NotImplementedException(); }
		}
		
		[MonoTODO]
		public DbCommand SelectCommand {
			get { return (DbCommand) ((IDbDataAdapter) this).SelectCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public DbCommand DeleteCommand {
			get { return (DbCommand) ((IDbDataAdapter) this).DeleteCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public DbCommand InsertCommand {
			get { return (DbCommand) ((IDbDataAdapter) this).InsertCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public DbCommand UpdateCommand {
			get { return (DbCommand) ((IDbDataAdapter) this).UpdateCommand; }
			set { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public IDbTransaction Transaction {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public int UpdateBatchSize {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#else
		IDbCommand SelectCommand {
			get { return ((IDbDataAdapter) this).SelectCommand; }
		}

		IDbCommand UpdateCommand {
			get { return ((IDbDataAdapter) this).UpdateCommand; }
		}

		IDbCommand DeleteCommand {
			get { return ((IDbDataAdapter) this).DeleteCommand; }
		}

		IDbCommand InsertCommand {
			get { return ((IDbDataAdapter) this).InsertCommand; }
		} 
#endif

		#endregion // Properties
		
		#region Events

#if ONLY_1_0 || ONLY_1_1

		[DataCategory ("Fill")]
		[DataSysDescription ("Event triggered when a recoverable error occurs during Fill.")]
		public event FillErrorEventHandler FillError;

#endif
		#endregion // Events

		#region Methods

#if NET_2_0
		[MonoTODO]
		public virtual void BeginInit ()
		{
			throw new NotImplementedException ();
		}
#endif

		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);
		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping);

		private FillErrorEventArgs CreateFillErrorEvent (DataTable dataTable, object[] values, Exception e)
		{
			FillErrorEventArgs args = new FillErrorEventArgs (dataTable, values);
			args.Errors = e;
			args.Continue = false;
			return args;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				IDbDataAdapter da = (IDbDataAdapter) this;
				if (da.SelectCommand != null) {
					da.SelectCommand.Dispose();
					da.SelectCommand = null;
				}
				if (da.InsertCommand != null) {
					da.InsertCommand.Dispose();
					da.InsertCommand = null;
				}
				if (da.UpdateCommand != null) {
					da.UpdateCommand.Dispose();
					da.UpdateCommand = null;
				}
				if (da.DeleteCommand != null) {
					da.DeleteCommand.Dispose();
					da.DeleteCommand = null;
				}
			}
		}

#if NET_2_0
		[MonoTODO]
		public virtual void EndInit ()
		{
			throw new NotImplementedException ();
		}
#endif

		public override int Fill (DataSet dataSet)
		{
			return Fill (dataSet, 0, 0, DefaultSourceTableName, ((IDbDataAdapter) this).SelectCommand, CommandBehavior.Default);
		}

		public int Fill (DataTable dataTable) 
		{
			if (dataTable == null)
				throw new ArgumentNullException ("DataTable");

			return Fill (dataTable, ((IDbDataAdapter) this).SelectCommand, CommandBehavior.Default);
		}

		public int Fill (DataSet dataSet, string srcTable) 
		{
			return Fill (dataSet, 0, 0, srcTable, ((IDbDataAdapter) this).SelectCommand, CommandBehavior.Default);
		}

#if NET_2_0
		protected override int Fill (DataTable dataTable, IDataReader dataReader) 
#else
			protected virtual int Fill (DataTable dataTable, IDataReader dataReader) 
#endif
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

		protected virtual int Fill (DataTable dataTable, IDbCommand command, CommandBehavior behavior) 
		{
			CommandBehavior commandBehavior = behavior;

			// first see that the connection is not close.
			if (command.Connection.State == ConnectionState.Closed) 
			{
				command.Connection.Open ();
				commandBehavior |= CommandBehavior.CloseConnection;
			}
			return Fill (dataTable, command.ExecuteReader (commandBehavior));
		}

#if NET_2_0
		[MonoTODO]
		public int Fill (int startRecord, int maxRecords, DataTable[] dataTables)
		{
			throw new NotImplementedException ();
		}
#endif

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable) 
		{
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, ((IDbDataAdapter) this).SelectCommand, CommandBehavior.Default);
		}

#if NET_2_0
		[MonoTODO]
		protected virtual int Fill (DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		protected override int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
#else
		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
#endif
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
					if (dataReader.FieldCount != -1)
					{
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
	
							if (!FillTable (dataTable, dataReader, startRecord, maxRecords, ref count)) {
								continue;
							}
	
							tableName = String.Format ("{0}{1}", srcTable, ++resultIndex);
	
							startRecord = 0;
							maxRecords = 0;
						}
					}
				} while (dataReader.NextResult ());
			} 
			finally {
				dataReader.Close ();
			}

                        return count;
		}
		
		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) 
		{
			if (MissingSchemaAction == MissingSchemaAction.AddWithKey)
				behavior |= CommandBehavior.KeyInfo;
			CommandBehavior commandBehavior = behavior;

			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				commandBehavior |= CommandBehavior.CloseConnection;
			}
			return Fill (dataSet, srcTable, command.ExecuteReader (commandBehavior), startRecord, maxRecords);
		}

		private bool FillTable (DataTable dataTable, IDataReader dataReader, int startRecord, int maxRecords, ref int counter)
		{
			if (dataReader.FieldCount == 0)
				return false;

			int counterStart = counter;

			int[] mapping = BuildSchema (dataReader, dataTable, SchemaType.Mapped);
			
			int[] sortedMapping = new int[mapping.Length];
			int length = sortedMapping.Length;
			for(int i=0; i < sortedMapping.Length; i++) {
				if (mapping[i] >= 0)
					sortedMapping[mapping[i]] = i;
				else
					sortedMapping[--length] = i;
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
					object[] readerArray = new object[dataReader.FieldCount];
					object[] tableArray = new object[mapping.Length];
					// we get the values from the datareader
					dataReader.GetValues (readerArray);
					// copy from datareader columns to table columns according to given mapping
					for (int i = 0; i < mapping.Length; i++) {
						if (mapping[i] >= 0) {
							tableArray[i] = readerArray[mapping[i]];
						}
					}
					FillErrorEventArgs args = CreateFillErrorEvent (dataTable, tableArray, e);
					OnFillError (args);

					// if args.Continue is not set to true or if a handler is not set, rethrow the error..
					if(!args.Continue)
						throw e;
				}
			}
			dataTable.EndLoadData ();
			return true;
		}

#if NET_2_0
		/// <summary>
		///     Fills the given datatable using values from reader. if a value 
		///     for a column is  null, that will be filled with default value. 
		/// </summary>
		/// <returns>No. of rows affected </returns>
		internal static int FillFromReader (DataTable table,
                                                    IDataReader reader,
                                                    int start, 
                                                    int length,
                                                    int [] mapping,
                                                    LoadOption loadOption
                                                    )
                {
                        if (reader.FieldCount == 0)
				return 0 ;

                        for (int i = 0; i < start; i++)
                                reader.Read ();

                        int counter = 0;
                        object [] values = new object [mapping.Length];
                        while (reader.Read () &&
                               (length == 0 || counter < length)) {
                                
                                for (int i = 0 ; i < mapping.Length; i++)
                                        values [i] = mapping [i] < 0 ? null : reader [mapping [i]];
                                        
                                table.BeginLoadData ();
                                table.LoadDataRow (values, loadOption);
                                table.EndLoadData ();
                                counter++;
                        }
                        return counter;
                }

#endif // NET_2_0

		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType) 
		{
			return FillSchema (dataSet, schemaType, ((IDbDataAdapter) this).SelectCommand, DefaultSourceTableName, CommandBehavior.Default);
		}

		public DataTable FillSchema (DataTable dataTable, SchemaType schemaType) 
		{
			return FillSchema (dataTable, schemaType, ((IDbDataAdapter) this).SelectCommand, CommandBehavior.Default);
		}

		public DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable) 
		{
			return FillSchema (dataSet, schemaType, ((IDbDataAdapter) this).SelectCommand, srcTable, CommandBehavior.Default);
		}

		[MonoTODO ("Verify")]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior) 
		{
			if (dataTable == null)
				throw new ArgumentNullException ("DataTable");

			behavior |= CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo;
			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				behavior |= CommandBehavior.CloseConnection;
			}

			IDataReader reader = command.ExecuteReader (behavior);
			try
			{
				string tableName =  SetupSchema (schemaType, dataTable.TableName);
				if (tableName != null)
				{
					// FillSchema should add the KeyInfo unless MissingSchemaAction
					// is set to Ignore or Error.
					MissingSchemaAction schemaAction = MissingSchemaAction;
					if (!(schemaAction == MissingSchemaAction.Ignore ||
						schemaAction == MissingSchemaAction.Error))
						schemaAction = MissingSchemaAction.AddWithKey;

					BuildSchema (reader, dataTable, schemaType, schemaAction,
						MissingMappingAction, TableMappings);
				}
			}
			finally
			{
				reader.Close ();
			}
			return dataTable;
		}

		[MonoTODO ("Verify")]
		protected virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior) 
		{
			if (dataSet == null)
				throw new ArgumentNullException ("DataSet");

			behavior |= CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo;
			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				behavior |= CommandBehavior.CloseConnection;
			}

			IDataReader reader = command.ExecuteReader (behavior);
			ArrayList output = new ArrayList ();
			string tableName = srcTable;
			int index = 0;
			DataTable table;
			try
			{
				// FillSchema should add the KeyInfo unless MissingSchemaAction
				// is set to Ignore or Error.
				MissingSchemaAction schemaAction = MissingSchemaAction;
				if (!(MissingSchemaAction == MissingSchemaAction.Ignore ||
					MissingSchemaAction == MissingSchemaAction.Error))
					schemaAction = MissingSchemaAction.AddWithKey;

				do {
					tableName = SetupSchema (schemaType, tableName);
					if (tableName != null)
					{
						if (dataSet.Tables.Contains (tableName))
							table = dataSet.Tables [tableName];	
						else
						{
							// Do not create schema if MissingSchemAction is set to Ignore
							if (this.MissingSchemaAction == MissingSchemaAction.Ignore)
								continue;
							table =  dataSet.Tables.Add (tableName);
						}
						
						BuildSchema (reader, table, schemaType, schemaAction,
							MissingMappingAction, TableMappings);
						output.Add (table);
						tableName = String.Format ("{0}{1}", srcTable, ++index);
					}
				}while (reader.NextResult ());
			}
			finally
			{
				reader.Close ();
			}
			return (DataTable[]) output.ToArray (typeof (DataTable));
		}

#if NET_2_0
		[MonoTODO]
		public DataSet GetDataSet ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable GetDataTable ()
		{
			throw new NotImplementedException ();
		}
#endif

		private string SetupSchema (SchemaType schemaType, string sourceTableName)
		{
			DataTableMapping tableMapping = null;

			if (schemaType == SchemaType.Mapped) 
			{
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, sourceTableName, sourceTableName, MissingMappingAction);
				if (tableMapping != null)
					return tableMapping.DataSetTable;
				return null;
			}
			else
				return sourceTableName;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override IDataParameter[] GetFillParameters () 
		{
			IDataParameter[] parameters = new IDataParameter[SelectCommand.Parameters.Count];
			SelectCommand.Parameters.CopyTo (parameters, 0);
			return parameters;
		}
		
		// this method builds the schema for a given datatable. it returns a int array with 
		// "array[ordinal of datatable column] == index of source column in data reader".
		// each column in the datatable has a mapping to a specific column in the datareader,
		// the int array represents this match.
		[MonoTODO ("Test")]
		private int[] BuildSchema (IDataReader reader, DataTable table, SchemaType schemaType)
		{
			return BuildSchema (reader, table, schemaType, MissingSchemaAction,
					    MissingMappingAction, TableMappings);
                }

                /// <summary>
                ///     Creates or Modifies the schema of the given DataTable based on the schema of
                ///     the reader and the arguments passed.
                /// </summary>
                internal static int[] BuildSchema (IDataReader reader,
                                                   DataTable table,
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
				if (ColumnNameCol == null || schemaRow.IsNull(ColumnNameCol) || (string)schemaRow [ColumnNameCol] == String.Empty) {
					sourceColumnName = DefaultSourceColumnName;
					realSourceColumnName = DefaultSourceColumnName + "1";
				}
				else {
					sourceColumnName = (string) schemaRow [ColumnNameCol];
					realSourceColumnName = sourceColumnName;
				}

				for (int i = 1; sourceColumns.Contains (realSourceColumnName); i += 1) 
					realSourceColumnName = String.Format ("{0}{1}", sourceColumnName, i);
				sourceColumns.Add(realSourceColumnName);

				// generate DataSetColumnName from DataTableMapping, if any
				string dsColumnName = realSourceColumnName;
				DataTableMapping tableMapping = null;

				//FIXME : The sourcetable name shud get passed as a parameter.. 
				int index = dtMapping.IndexOfDataSetTable (table.TableName);
				string srcTable = (index != -1 ? dtMapping[index].SourceTable : table.TableName);
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (dtMapping, srcTable, table.TableName, missingMapAction); 
				if (tableMapping != null)
				{
					table.TableName = tableMapping.DataSetTable;
					// check to see if the column mapping exists
					DataColumnMapping columnMapping = DataColumnMappingCollection.GetColumnMappingBySchemaAction(tableMapping.ColumnMappings, realSourceColumnName, missingMapAction);
					if (columnMapping != null)
					{
						Type columnType = (Type)schemaRow[DataTypeCol];
						DataColumn col =
							columnMapping.GetDataColumnBySchemaAction(
												  table ,
												  columnType,
												  missingSchAction);

						if (col != null)
						{
							// if the column is not in the table - add it.
							if (table.Columns.IndexOf(col) == -1)
							{
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

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] dataRows) 
		{
			if (dataRows == null)
				throw new ArgumentNullException("dataRows");
			
			if (dataRows.Length == 0)
				return 0;

			if (dataRows[0] == null)
				throw new ArgumentException("dataRows[0].");

			DataTable table = dataRows[0].Table;
			if (table == null)
				throw new ArgumentException("table is null reference.");
			
			// all rows must be in the same table
			for (int i = 0; i < dataRows.Length; i++)
			{
				if (dataRows[i] == null)
					throw new ArgumentException("dataRows[" + i + "].");
				if (dataRows[i].Table != table)
					throw new ArgumentException(
								    " DataRow["
								    + i
								    + "] is from a different DataTable than DataRow[0].");
			}
			
			// get table mapping for this rows
			DataTableMapping tableMapping = TableMappings.GetByDataSetTable(table.TableName);
			if (tableMapping == null)
			{
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction(
													TableMappings,
													table.TableName,
													table.TableName,
													MissingMappingAction);
				if (tableMapping != null) {
					foreach (DataColumn col in table.Columns) {
						if (tableMapping.ColumnMappings.IndexOf (col.ColumnName) >= 0)
							continue;
						DataColumnMapping columnMapping = DataColumnMappingCollection.GetColumnMappingBySchemaAction (tableMapping.ColumnMappings, col.ColumnName, MissingMappingAction);
						if (columnMapping == null)
							columnMapping = new DataColumnMapping (col.ColumnName, col.ColumnName);
						tableMapping.ColumnMappings.Add (columnMapping);
					}
				} else {
					ArrayList cmc = new ArrayList ();
					foreach (DataColumn col in table.Columns)
						cmc.Add (new DataColumnMapping (col.ColumnName, col.ColumnName));
					tableMapping =
						new DataTableMapping (
								      table.TableName,
								      table.TableName,
								      cmc.ToArray (typeof (DataColumnMapping)) as DataColumnMapping []);
				}
			}

			DataRow[] copy = table.NewRowArray(dataRows.Length);
			Array.Copy(dataRows, 0, copy, 0, dataRows.Length);
			return Update(copy, tableMapping);
		}

		public override int Update (DataSet dataSet) 
		{
			return Update (dataSet, DefaultSourceTableName);
		}

		public int Update (DataTable dataTable) 
		{
			/*
			  int index = TableMappings.IndexOfDataSetTable (dataTable.TableName);
			  if (index < 0)
			  throw new ArgumentException ();
			  return Update (dataTable, TableMappings [index]);
			*/
			DataTableMapping tableMapping = TableMappings.GetByDataSetTable (dataTable.TableName);
			if (tableMapping == null)
			{
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (
													 TableMappings,
													 dataTable.TableName,
													 dataTable.TableName,
													 MissingMappingAction);
				if (tableMapping != null) {
					foreach (DataColumn col in dataTable.Columns) {
						if (tableMapping.ColumnMappings.IndexOf (col.ColumnName) >= 0)
							continue;
						DataColumnMapping columnMapping = DataColumnMappingCollection.GetColumnMappingBySchemaAction (tableMapping.ColumnMappings, col.ColumnName, MissingMappingAction);
						if (columnMapping == null)
							columnMapping = new DataColumnMapping (col.ColumnName, col.ColumnName);
						tableMapping.ColumnMappings.Add (columnMapping);
					}
				} else {
					ArrayList cmc = new ArrayList ();
					foreach (DataColumn col in dataTable.Columns)
						cmc.Add (new DataColumnMapping (col.ColumnName, col.ColumnName));
					tableMapping =
						new DataTableMapping (
								      dataTable.TableName,
								      dataTable.TableName,
								      cmc.ToArray (typeof (DataColumnMapping)) as DataColumnMapping []);
				}
			}
			return Update (dataTable, tableMapping);
		}

		private int Update (DataTable dataTable, DataTableMapping tableMapping)
		{
			DataRow[] rows = dataTable.NewRowArray(dataTable.Rows.Count);
			dataTable.Rows.CopyTo (rows, 0);
			return Update (rows, tableMapping);
		}

		[MonoTODO]
		protected virtual int Update (DataRow[] dataRows, DataTableMapping tableMapping) 
		{
			int updateCount = 0;
			foreach (DataRow row in dataRows) {
				StatementType statementType = StatementType.Update;
				IDbCommand command = null;
				string commandName = String.Empty;

				switch (row.RowState) {
				case DataRowState.Added:
					statementType = StatementType.Insert;
					command = ((IDbDataAdapter) this).InsertCommand;
					commandName = "Insert";
					break;
				case DataRowState.Deleted:
					statementType = StatementType.Delete;
					command = ((IDbDataAdapter) this).DeleteCommand;
					commandName = "Delete";
					break;
				case DataRowState.Modified:
					statementType = StatementType.Update;
					command = ((IDbDataAdapter) this).UpdateCommand;
					commandName = "Update";
					break;
				case DataRowState.Unchanged:
				case DataRowState.Detached:
					continue;
				}

				RowUpdatingEventArgs argsUpdating = CreateRowUpdatingEvent (row, command, statementType, tableMapping);
				row.RowError = null;
				OnRowUpdating(argsUpdating);
				switch(argsUpdating.Status) {
				case UpdateStatus.Continue :
					//continue in update operation
					break;
				case UpdateStatus.ErrorsOccurred :
					if (argsUpdating.Errors == null) {
						argsUpdating.Errors = ExceptionHelper.RowUpdatedError();
					}
					row.RowError += argsUpdating.Errors.Message;
					if (!ContinueUpdateOnError) {
						throw argsUpdating.Errors;
					}
					continue;
				case UpdateStatus.SkipAllRemainingRows :
					return updateCount;
				case UpdateStatus.SkipCurrentRow :
					updateCount++;
					continue;
				default :
					throw ExceptionHelper.InvalidUpdateStatus(argsUpdating.Status);
				}
				command = argsUpdating.Command;					
				try {
					if (command != null) {
						DataColumnMappingCollection columnMappings = tableMapping.ColumnMappings;
						foreach (IDataParameter parameter in command.Parameters) {
							if ((parameter.Direction & ParameterDirection.Input) != 0) {
								string dsColumnName = parameter.SourceColumn;
								if (columnMappings.Contains(parameter.SourceColumn))
									dsColumnName = columnMappings [parameter.SourceColumn].DataSetColumn;
								if (dsColumnName == null || dsColumnName.Length <= 0)
									continue;
								
								DataRowVersion rowVersion = parameter.SourceVersion;
								// Parameter version is ignored for non-update commands
								if (statementType == StatementType.Delete) 
									rowVersion = DataRowVersion.Original;

								parameter.Value = row [dsColumnName, rowVersion];
							}
						}
					}
				}
				catch (Exception e) {
					argsUpdating.Errors = e;
					argsUpdating.Status = UpdateStatus.ErrorsOccurred;
				}

				
				IDataReader reader = null;
				try {								
					if (command == null) {
						throw ExceptionHelper.UpdateRequiresCommand(commandName);
					}				
				
					CommandBehavior commandBehavior = CommandBehavior.Default;
					if (command.Connection.State == ConnectionState.Closed) {
						command.Connection.Open ();
						commandBehavior |= CommandBehavior.CloseConnection;
					}
				
					// use ExecuteReader because we want to use the commandbehavior parameter.
					// so the connection will be closed if needed.
					reader = command.ExecuteReader (commandBehavior);

					// update the current row, if the update command returns any resultset
					// ignore other than the first record.
					DataColumnMappingCollection columnMappings = tableMapping.ColumnMappings;

					if (command.UpdatedRowSource == UpdateRowSource.Both ||
					    command.UpdatedRowSource == UpdateRowSource.FirstReturnedRecord) {
						if (reader.Read ()){
							DataTable retSchema = reader.GetSchemaTable ();
							foreach (DataRow dr in retSchema.Rows) {
								string columnName = dr ["ColumnName"].ToString ();
								string dstColumnName = columnName;
								if (columnMappings != null &&
								    columnMappings.Contains(columnName))
									dstColumnName = columnMappings [dstColumnName].DataSetColumn;
								DataColumn dstColumn = row.Table.Columns [dstColumnName];
								if (dstColumn == null
								    || (dstColumn.Expression != null
									&& dstColumn.Expression.Length > 0))
									continue;
								// info from : http://www.error-bank.com/microsoft.public.dotnet.framework.windowsforms.databinding/
								// _35_hcsyiv0dha.2328@tk2msftngp10.phx.gbl_Thread.aspx
								// disable readonly for non-expression columns.
								bool readOnlyState = dstColumn.ReadOnly;
								dstColumn.ReadOnly = false;
								try {
									row [dstColumnName] = reader [columnName];
								} finally {
									dstColumn.ReadOnly = readOnlyState;
								}                                    
							}
						}
					}
					reader.Close ();

					int tmp = reader.RecordsAffected; // records affected is valid only after closing reader
					// if the execute does not effect any rows we throw an exception.
					if (tmp == 0)
						throw new DBConcurrencyException("Concurrency violation: the " + 
                                                                                 commandName +"Command affected 0 records.");
					updateCount += tmp;
                                        
					if (command.UpdatedRowSource == UpdateRowSource.Both ||
					    command.UpdatedRowSource == UpdateRowSource.OutputParameters) {
						// Update output parameters to row values
						foreach (IDataParameter parameter in command.Parameters) {
							if (parameter.Direction != ParameterDirection.InputOutput
							    && parameter.Direction != ParameterDirection.Output
							    && parameter.Direction != ParameterDirection.ReturnValue)
								continue;

							string dsColumnName = parameter.SourceColumn;
							if (columnMappings != null &&
							    columnMappings.Contains(parameter.SourceColumn))
								dsColumnName = columnMappings [parameter.SourceColumn].DataSetColumn;
							DataColumn dstColumn = row.Table.Columns [dsColumnName];
							if (dstColumn == null
							    || (dstColumn.Expression != null 
								&& dstColumn.Expression.Length > 0))
								continue;
							bool readOnlyState = dstColumn.ReadOnly;
							dstColumn.ReadOnly  = false;
							try {
								row [dsColumnName] = parameter.Value;
							} finally {
								dstColumn.ReadOnly = readOnlyState;
							}
                                    
						}
					}
                    
					RowUpdatedEventArgs updatedArgs = CreateRowUpdatedEvent(row, command, statementType, tableMapping);
					OnRowUpdated(updatedArgs);
					switch(updatedArgs.Status) {
					case UpdateStatus.Continue:
						break;
					case UpdateStatus.ErrorsOccurred:
						if (updatedArgs.Errors == null) {
							updatedArgs.Errors = ExceptionHelper.RowUpdatedError();
						}
						row.RowError += updatedArgs.Errors.Message;
						if (!ContinueUpdateOnError) {
							throw updatedArgs.Errors;
						}
						break;
					case UpdateStatus.SkipCurrentRow:
						continue;
					case UpdateStatus.SkipAllRemainingRows:
						return updateCount;
					}
#if NET_2_0
					if (!AcceptChangesDuringUpdate)
						continue;
#endif
					row.AcceptChanges ();
				} catch(Exception e) {
					row.RowError = e.Message;
					if (!ContinueUpdateOnError) {
						throw e;
					}
				} finally {
					if (reader != null && ! reader.IsClosed) {
						reader.Close ();
					}
				}	
			}		
			return updateCount;
		}

		public int Update (DataSet dataSet, string sourceTable) 
		{
			MissingMappingAction mappingAction = MissingMappingAction;

			if (mappingAction == MissingMappingAction.Ignore)
				mappingAction = MissingMappingAction.Error;

			DataTableMapping tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, sourceTable, sourceTable, mappingAction);

			DataTable dataTable = dataSet.Tables[tableMapping.DataSetTable];
			if (dataTable == null)
				throw new ArgumentException (String.Format ("Missing table {0}",
									    sourceTable));
			return Update (dataTable, tableMapping);
		}

#if ONLY_1_0 || ONLY_1_1
		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			if (FillError != null)
				FillError (this, value);
		}
#endif

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		#endregion // Methods
	}
}
