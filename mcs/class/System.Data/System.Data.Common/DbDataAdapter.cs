//
// System.Data.Common.DbDataAdapter.cs
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.Common {
	public abstract class DbDataAdapter : DataAdapter, ICloneable
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

		IDbCommand DeleteCommand {
			get { return ((IDbDataAdapter) this).DeleteCommand; }
		}

#if NET_2_0
		protected internal CommandBehavior FillCommandBehavior {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		IDbCommand InsertCommand {
			get { return ((IDbDataAdapter) this).InsertCommand; }
		}

#if NET_2_0
		[MonoTODO]
		protected virtual IDbCommand this [[Optional] StatementType statementType] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		protected virtual DbProviderFactory ProviderFactory {
			get { throw new NotImplementedException (); }
		}
#endif

		IDbCommand SelectCommand {
			get { return ((IDbDataAdapter) this).SelectCommand; }
		}

#if NET_2_0
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
#endif

		IDbCommand UpdateCommand {
			get { return ((IDbDataAdapter) this).UpdateCommand; }
		}

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
			return Fill (dataSet, 0, 0, DefaultSourceTableName, SelectCommand, CommandBehavior.Default);
		}

		public int Fill (DataTable dataTable) 
		{
			if (dataTable == null)
				throw new NullReferenceException ();

			return Fill (dataTable, SelectCommand, CommandBehavior.Default);
		}

		public int Fill (DataSet dataSet, string srcTable) 
		{
			return Fill (dataSet, 0, 0, srcTable, SelectCommand, CommandBehavior.Default);
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
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, SelectCommand, CommandBehavior.Default);
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
			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

			DataTable dataTable;
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
								dataTable = new DataTable(tableName);
								dataSet.Tables.Add (dataTable);
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
			if (dataReader.FieldCount == 0) {
				return false;
			}

			int counterStart = counter;
			
			int[] mapping = BuildSchema (dataReader, dataTable, SchemaType.Mapped);

				for (int i = 0; i < startRecord; i++) {
					dataReader.Read ();
				}

			while (dataReader.Read () && (maxRecords == 0 || (counter - counterStart) < maxRecords)) {
				try {
					dataTable.BeginLoadData ();
					dataTable.LoadDataRow (dataReader, mapping, AcceptChangesDuringFill);
					dataTable.EndLoadData ();
					counter++;
				} 
				catch (Exception e) {
					object[] readerArray = new object[dataReader.FieldCount];
					object[] tableArray = new object[mapping.Length];
					// we get the values from the datareader
					dataReader.GetValues (readerArray);
					// copy from datareader columns to table columns according to given mapping
					for (int i = 0; i < mapping.Length; i++) {
						tableArray[i] = readerArray[mapping[i]];
					}
					FillErrorEventArgs args = CreateFillErrorEvent (dataTable, tableArray, e);
					OnFillError (args);
					if(!args.Continue) {
						return false;
					}
				}
			}
			return true;
		}

		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType) 
		{
			return FillSchema (dataSet, schemaType, SelectCommand, DefaultSourceTableName, CommandBehavior.Default);
		}

		public DataTable FillSchema (DataTable dataTable, SchemaType schemaType) 
		{
			return FillSchema (dataTable, schemaType, SelectCommand, CommandBehavior.Default);
		}

		public DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable) 
		{
			return FillSchema (dataSet, schemaType, SelectCommand, srcTable, CommandBehavior.Default);
		}

		[MonoTODO ("Verify")]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior) 
		{
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
					BuildSchema (reader, dataTable, schemaType);
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
				tableName = SetupSchema (schemaType, tableName);
				if (tableName != null)
				{
					if (dataSet.Tables.Contains (tableName))
						table = dataSet.Tables [tableName];	
					else
					{
						table = new DataTable(tableName);
						dataSet.Tables.Add (table);
					}
					BuildSchema (reader, table, schemaType);
					output.Add (table);
					tableName = String.Format ("{0}{1}", srcTable, ++index);
				}
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
			int readerIndex = 0;
			int[] mapping = new int[reader.FieldCount]; // mapping the reader indexes to the datatable indexes
			ArrayList primaryKey = new ArrayList ();
			ArrayList sourceColumns = new ArrayList ();

			foreach (DataRow schemaRow in reader.GetSchemaTable ().Rows) {
				// generate a unique column name in the source table.
				string sourceColumnName;
				if (schemaRow.IsNull("ColumnName"))
					sourceColumnName = DefaultSourceColumnName;
				else 
					sourceColumnName = (string) schemaRow ["ColumnName"];

				string realSourceColumnName = sourceColumnName;

				for (int i = 1; sourceColumns.Contains (realSourceColumnName); i += 1) 
					realSourceColumnName = String.Format ("{0}{1}", sourceColumnName, i);
				sourceColumns.Add(realSourceColumnName);

				// generate DataSetColumnName from DataTableMapping, if any
				string dsColumnName = realSourceColumnName;
				DataTableMapping tableMapping = null;
				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, table.TableName, table.TableName, MissingMappingAction); 
				if (tableMapping != null) 
				{
					
					table.TableName = tableMapping.DataSetTable;
					// check to see if the column mapping exists
					DataColumnMapping columnMapping = DataColumnMappingCollection.GetColumnMappingBySchemaAction(tableMapping.ColumnMappings, realSourceColumnName, MissingMappingAction);
					if (columnMapping != null)
					{
						DataColumn col =
							columnMapping.GetDataColumnBySchemaAction(
							table ,
							(Type)schemaRow["DataType"],
							MissingSchemaAction);

						if (col != null)
						{
							// if the column is not in the table - add it.
							if (table.Columns.IndexOf(col) == -1)
							{
								if (MissingSchemaAction == MissingSchemaAction.Add || MissingSchemaAction == MissingSchemaAction.AddWithKey)
									table.Columns.Add(col);
							}

							if (!schemaRow["IsKey"].Equals (DBNull.Value))
								if ((bool) (schemaRow ["IsKey"]))
									primaryKey.Add (col);
							
							// add the ordinal of the column as a key and the index of the column in the datareader as a value.
							mapping[col.Ordinal] = readerIndex;
						}
					}
				}
				readerIndex++;
			}
			if (primaryKey.Count > 0)
				table.PrimaryKey = (DataColumn[])(primaryKey.ToArray(typeof (DataColumn)));

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

			DataRow[] copy = new DataRow [dataRows.Length];
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
			DataRow[] rows = new DataRow [dataTable.Rows.Count];
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
				bool useCommandBuilder = false;

				switch (row.RowState) {
				case DataRowState.Added:
					statementType = StatementType.Insert;
					command = InsertCommand;
					commandName = "Insert";
					break;
				case DataRowState.Deleted:
					statementType = StatementType.Delete;
					command = DeleteCommand;
					commandName = "Delete";
					break;
				case DataRowState.Modified:
					statementType = StatementType.Update;
					command = UpdateCommand;
					commandName = "Update";
					break;
				case DataRowState.Unchanged:
					continue;
				case DataRowState.Detached:
					throw new NotImplementedException ();
				}

				if (command == null)
					useCommandBuilder = true;

				RowUpdatingEventArgs args = CreateRowUpdatingEvent (row, command, statementType, tableMapping);
				OnRowUpdating (args);

				if (args.Status == UpdateStatus.ErrorsOccurred)
					throw (args.Errors);

				if (command == null && args.Command != null)
					command = args.Command;
				else if (command == null)
					throw new InvalidOperationException (String.Format ("Update requires a valid {0}Command when passed a DataRow collection with modified rows.", commandName));

				if (!useCommandBuilder) {
					DataColumnMappingCollection columnMappings = tableMapping.ColumnMappings;

					foreach (IDataParameter parameter in command.Parameters) {
						string dsColumnName = parameter.SourceColumn;
						if (columnMappings.Contains(parameter.SourceColumn))
							dsColumnName = columnMappings [parameter.SourceColumn].DataSetColumn;
						DataRowVersion rowVersion = DataRowVersion.Default;

						// Parameter version is ignored for non-update commands
						if (statementType == StatementType.Update) 
							rowVersion = parameter.SourceVersion;
						if (statementType == StatementType.Delete) 
							rowVersion = DataRowVersion.Original;

						parameter.Value = row [dsColumnName, rowVersion];
					}
				}
				
				CommandBehavior commandBehavior = CommandBehavior.Default;
				if (command.Connection.State == ConnectionState.Closed) 
				{
					command.Connection.Open ();
					commandBehavior |= CommandBehavior.CloseConnection;
				}
				
				IDataReader reader = null;
				try
				{
					// use ExecuteReader because we want to use the commandbehavior parameter.
					// so the connection will be closed if needed.
					reader = command.ExecuteReader (commandBehavior);
					int tmp = reader.RecordsAffected;
					// if the execute does not effect any rows we throw an exception.
					if (tmp == 0)
						throw new DBConcurrencyException("Concurrency violation: the " + commandName +"Command affected 0 records.");
					updateCount += tmp;
					OnRowUpdated (CreateRowUpdatedEvent (row, command, statementType, tableMapping));
					row.AcceptChanges ();
				}
				catch (Exception e)
				{
					if (ContinueUpdateOnError)
						row.RowError = e.Message;// do somthing with the error
					else
						throw e;
				}
				finally
				{
					if (reader != null)
						reader.Close ();
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
			    throw new ArgumentException ("sourceTable");

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
