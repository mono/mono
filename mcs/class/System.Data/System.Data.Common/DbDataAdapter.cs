//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;

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

		#endregion // Fields

		#region Properties

		IDbCommand DeleteCommand {
			get { return ((IDbDataAdapter) this).DeleteCommand; }
		}

		IDbCommand InsertCommand {
			get { return ((IDbDataAdapter) this).InsertCommand; }
		}

		IDbCommand SelectCommand {
			get { return ((IDbDataAdapter) this).SelectCommand; }
		}


		IDbCommand UpdateCommand {
			get { return ((IDbDataAdapter) this).UpdateCommand; }
		}

	 	#endregion // Properties
		
		#region Events

		[DataCategory ("Fill")]
		[DataSysDescription ("Event triggered when a recoverable error occurs during Fill.")]
		public event FillErrorEventHandler FillError;

		#endregion // Events

		#region Methods

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
				((IDbDataAdapter) this).SelectCommand = null;
				((IDbDataAdapter) this).InsertCommand = null;
				((IDbDataAdapter) this).UpdateCommand = null;
				((IDbDataAdapter) this).DeleteCommand = null;
			}
		}

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

		protected virtual int Fill (DataTable dataTable, IDataReader dataReader) 
		{
			int count = 0;
			bool doContinue = true;

			if (dataReader.FieldCount == 0) {
				dataReader.Close ();
				return 0;
			}
			
			try
			{
				object[] itemArray = new object [dataReader.FieldCount];
				string tableName = SetupSchema (SchemaType.Mapped, dataTable.TableName);
				if (tableName != null)
				{
					dataTable.TableName = tableName;
					Hashtable mapping = BuildSchema (dataReader, dataTable, SchemaType.Mapped);

					while (doContinue && dataReader.Read ()) 
					{
						// we get the values from the datareader
						dataReader.GetValues (itemArray);

						// we only need the values that has a mapping to the table.
						object[] tableArray = new object[mapping.Count];
						for (int i = 0; i < tableArray.Length; i++)
							tableArray[i] = mapping[i]; // get the value for each column

						try 
						{
							dataTable.BeginLoadData ();
							dataTable.LoadDataRow (itemArray, AcceptChangesDuringFill);
							dataTable.EndLoadData ();
							count += 1;
						}
						catch (Exception e) 
						{
							FillErrorEventArgs args = CreateFillErrorEvent (dataTable, itemArray, e);
							OnFillError (args);
							doContinue = args.Continue;
						}
					}
				}
			}
			finally
			{
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

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable) 
		{
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, SelectCommand, CommandBehavior.Default);
		}

		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
		{
			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

			if (dataReader.FieldCount == 0) {
				dataReader.Close ();
				return 0;
			}

                        DataTable dataTable;
                        int resultIndex = 0;
                        int count = 0;
			bool doContinue = true;
			
			try
			{
				string tableName = srcTable;
				object[] itemArray;

				do 
				{
					tableName = SetupSchema (SchemaType.Mapped, tableName);
					if (tableName != null)
					{
						// check if the table exists in the dataset
						if (dataSet.Tables.Contains (tableName)) 
							// get the table from the dataset
							dataTable = dataSet.Tables [tableName];
						else
						{
							dataTable = new DataTable(tableName);
							dataSet.Tables.Add (dataTable);
						}
						Hashtable mapping = BuildSchema (dataReader, dataTable, SchemaType.Mapped);

						for (int i = 0; i < startRecord; i += 1)
							dataReader.Read ();
						
						itemArray = new object [dataReader.FieldCount];

						while (doContinue && dataReader.Read () && !(maxRecords > 0 && count >= maxRecords)) 
						{
							// we get the values from the datareader
							dataReader.GetValues (itemArray);
							
							// we only need the values that has a mapping to the table.
							object[] tableArray = new object[mapping.Count];
							for (int i = 0; i < tableArray.Length; i++)
								tableArray[i] = itemArray[(int)mapping[i]]; // get the value for each column
							
							try 
							{
								dataTable.BeginLoadData ();
								//dataTable.LoadDataRow (itemArray, AcceptChangesDuringFill);
								dataTable.LoadDataRow (tableArray, AcceptChangesDuringFill);
								dataTable.EndLoadData ();
								count += 1;
							}
							catch (Exception e) 
							{
								FillErrorEventArgs args = CreateFillErrorEvent (dataTable, itemArray, e);
								OnFillError (args);
								doContinue = args.Continue;
							}
						}

						tableName = String.Format ("{0}{1}", srcTable, ++resultIndex);

						startRecord = 0;
						maxRecords = 0;
					}

				} while (doContinue && dataReader.NextResult ());
			}
			finally
			{
				dataReader.Close ();
			}

                        return count;
		}

		
		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) 
		{
			CommandBehavior commandBehavior = behavior;
			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				commandBehavior |= CommandBehavior.CloseConnection;
			}
			return Fill (dataSet, srcTable, command.ExecuteReader (commandBehavior), startRecord, maxRecords);
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
		
		// this method bulds the schema for a given datatable
		// returns a hashtable that his keys are the ordinal of the datatable columns, and his values
		// are the indexes of the source columns in the data reader.
		// each column in the datatable has a mapping to a specific column in the datareader
		// the hashtable represents this match.
		[MonoTODO ("Test")]
		private Hashtable BuildSchema (IDataReader reader, DataTable table, SchemaType schemaType)
		{
			int readerIndex = 0;
			Hashtable mapping = new Hashtable(); // hashing the reader indexes with the datatable indexes
			ArrayList primaryKey = new ArrayList ();
			ArrayList sourceColumns = new ArrayList ();

			foreach (DataRow schemaRow in reader.GetSchemaTable ().Rows) {
				// generate a unique column name in the source table.
				string sourceColumnName;
				if (schemaRow ["ColumnName"].Equals (DBNull.Value))
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
				if (schemaType == SchemaType.Mapped)
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
							mapping.Add(col.Ordinal, readerIndex);
						}
					}
				}
				readerIndex++;
			}
			if (MissingSchemaAction == MissingSchemaAction.AddWithKey && primaryKey.Count > 0)
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
				if (tableMapping == null)
					tableMapping =
						new DataTableMapping(
						table.TableName,
						table.TableName);
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
			int index = TableMappings.IndexOfDataSetTable (dataTable.TableName);
			if (index < 0)
				throw new ArgumentException ();
			return Update (dataTable, TableMappings [index]);
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
						DataColumnMapping mapping = columnMappings [parameter.SourceColumn];
						if (mapping != null) dsColumnName = mapping.DataSetColumn;
						DataRowVersion rowVersion = DataRowVersion.Default;

						// Parameter version is ignored for non-update commands
						if (statementType == StatementType.Update) 
							rowVersion = parameter.SourceVersion;
						if (statementType == StatementType.Delete) 
							rowVersion = DataRowVersion.Original;

						parameter.Value = row [dsColumnName, rowVersion];
					}
					row.AcceptChanges ();
				}

				if (command.Connection.State == ConnectionState.Closed) 
					command.Connection.Open ();
				
				try
				{
					int tmp = command.ExecuteNonQuery ();
					// if the execute does not effect any rows we throw an exception.
					if (tmp == 0)
						throw new DBConcurrencyException("Concurrency violation: the " + commandName +"Command affected 0 records.");
					updateCount += tmp;
					OnRowUpdated (CreateRowUpdatedEvent (row, command, statementType, tableMapping));
				}
				catch (Exception e)
				{
					if (ContinueUpdateOnError)
						row.RowError = e.Message;// do somthing with the error
					else
						throw e;
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

		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			if (FillError != null)
				FillError (this, value);
		}

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		#endregion // Methods
	}
}
