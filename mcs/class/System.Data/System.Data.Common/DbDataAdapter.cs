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
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Aids implementation of the IDbDataAdapter interface. Inheritors of DbDataAdapter  implement a set of functions to provide strong typing, but inherit most of the functionality needed to fully implement a DataAdapter.
	/// </summary>
	public abstract class DbDataAdapter : DataAdapter, ICloneable
	{
		#region Fields

		public const string DefaultSourceTableName = "Table";
		const string DefaultSourceColumnName = "Column";
		static readonly object EventFillError = new object ();

		#endregion
		
		#region Constructors

		protected DbDataAdapter() 
		{
		}

		#endregion

		#region Properties

		IDbCommand DeleteCommand {
			get { return ((IDbDataAdapter)this).DeleteCommand; }
		}

		IDbCommand InsertCommand {
			get { return ((IDbDataAdapter)this).InsertCommand; }
		}

		IDbCommand SelectCommand {
			get { return ((IDbDataAdapter)this).SelectCommand; }
		}


		IDbCommand UpdateCommand {
			get { return ((IDbDataAdapter)this).UpdateCommand; }
		}

		#endregion

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

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
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

		[MonoTODO ("Support filling after we have already filled.")]
		protected virtual int Fill (DataTable dataTable, IDataReader dataReader) 
		{
			int count = 0;
			bool doContinue = true;

			object[] itemArray = new object [dataReader.FieldCount];
			GetSchema (dataReader, dataTable);

			while (doContinue && dataReader.Read ()) {
				dataReader.GetValues (itemArray);
				try {
					dataTable.BeginLoadData ();
					dataTable.LoadDataRow (itemArray, AcceptChangesDuringFill);
					dataTable.EndLoadData ();
					count += 1;
				}
				catch (Exception e) {
					FillErrorEventArgs args = CreateFillErrorEvent (dataTable, itemArray, e);
					OnFillError (args);
					doContinue = args.Continue;
				}
			}
			dataReader.Close ();

			return count;
		}

		protected virtual int Fill (DataTable dataTable, IDbCommand command, CommandBehavior behavior) 
		{
			return Fill (dataTable, command.ExecuteReader (behavior));
		}

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable) 
		{
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, SelectCommand, CommandBehavior.Default);
		}

		[MonoTODO ("Support filling after we have already filled.")]
		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords) 
		{
			if (startRecord < 0)
				throw new ArgumentException ("The startRecord parameter was less than 0.");
			if (maxRecords < 0)
				throw new ArgumentException ("The maxRecords parameter was less than 0.");

                        DataTable dataTable;
                        int resultIndex = 0;
                        int count = 0;
			bool doContinue = true;

			string tableName = srcTable;
			object[] itemArray = new object [dataReader.FieldCount];

                       	do {
				if (dataSet.Tables.Contains (tableName))
					dataTable = dataSet.Tables[tableName];
				else
					dataTable = new DataTable (tableName);
				GetSchema (dataReader, dataTable);

				for (int k = 0; k < startRecord; k += 1)
					dataReader.Read ();

				while (doContinue && dataReader.Read () && !(maxRecords > 0 && count >= maxRecords))
				{
					dataReader.GetValues (itemArray);
					try {
						dataTable.BeginLoadData ();
						dataTable.LoadDataRow (itemArray, AcceptChangesDuringFill);
						dataTable.EndLoadData ();
						count += 1;
					}
					catch (Exception e) {
						FillErrorEventArgs args = CreateFillErrorEvent (dataTable, itemArray, e);
						OnFillError (args);
						doContinue = args.Continue;
					}
				}

				if (dataTable.Rows.Count > 0) {
					dataSet.Tables.Add (dataTable);
                               		tableName = String.Format ("{0}{1}", srcTable, ++resultIndex);
				}

				startRecord = 0;
				maxRecords = 0;

                       	} while (doContinue && dataReader.NextResult ());
                        dataReader.Close ();

                        return count;
		}


		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) 
		{
			CommandBehavior commandBehavior = behavior;
			if (command.Connection.State == ConnectionState.Closed)
			{
				command.Connection.Open ();
				commandBehavior = behavior | CommandBehavior.CloseConnection;
			}

			return Fill (dataSet, srcTable, command.ExecuteReader (commandBehavior), startRecord, maxRecords);
		}

		[MonoTODO]
		public override DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable FillSchema (DataTable dataTable, SchemaType schemaType) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		public override IDataParameter[] GetFillParameters () 
		{
			object[] parameters = new object [SelectCommand.Parameters.Count];
			SelectCommand.Parameters.CopyTo (parameters, 0);
			return (IDataParameter[]) parameters;
		}

		private void GetSchema (IDataReader reader, DataTable table)
		{
			string sourceColumnName;
			string sourceTableName;
			string dsColumnName;

			ArrayList primaryKey = new ArrayList (); 	
			DataTableMapping tableMapping;

			foreach (DataRow schemaRow in reader.GetSchemaTable ().Rows)
			{
				// generate a unique column name in the dataset table.
				if (schemaRow ["BaseColumnName"].Equals (DBNull.Value))
					sourceColumnName = DefaultSourceColumnName;
				else 
					sourceColumnName = (string) schemaRow ["BaseColumnName"];

				dsColumnName = sourceColumnName;

				for (int i = 1; table.Columns.Contains (dsColumnName); i += 1) 
					dsColumnName = String.Format ("{0}{1}", sourceColumnName, i);

				if (schemaRow ["BaseTableName"].Equals (DBNull.Value))
					sourceTableName = DefaultSourceTableName;
				else
					sourceTableName = (string) schemaRow ["BaseTableName"];

				tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, sourceTableName, table.TableName, MissingMappingAction);

				// check to see if the column mapping exists
				if (tableMapping.ColumnMappings.IndexOfDataSetColumn (dsColumnName) < 0)
				{
					if (MissingSchemaAction == MissingSchemaAction.Error)
						throw new SystemException ();

					table.Columns.Add (dsColumnName, (Type) schemaRow ["DataType"]);
					tableMapping.ColumnMappings.Add (dsColumnName, sourceColumnName);
				}
			
				if (!TableMappings.Contains (tableMapping))
					TableMappings.Add (tableMapping);

				if (!schemaRow["IsKey"].Equals (DBNull.Value))
					if ((bool) (schemaRow ["IsKey"]))
						primaryKey.Add (table.Columns [dsColumnName]);	
			}
			if (MissingSchemaAction == MissingSchemaAction.AddWithKey && primaryKey.Count > 0)
				table.PrimaryKey = (DataColumn[])(primaryKey.ToArray());
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (DataRow[] dataRows) 
		{
			throw new NotImplementedException (); // FIXME: Which mapping?
		}

		public override int Update (DataSet dataSet) 
		{
			int result = 0;
			foreach (DataTable table in dataSet.Tables)
				result += Update (table);	
			return result;
		}

		public int Update (DataTable dataTable) 
		{
			int index = TableMappings.IndexOfDataSetTable (dataTable.TableName);
			if (index < 0)
				throw new ArgumentException ();
			return Update ((DataRow[]) dataTable.Rows.List.ToArray (typeof (DataRow)), TableMappings[index]);
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
						string dsColumnName = columnMappings [parameter.SourceColumn].DataSetColumn;
						DataRowVersion rowVersion = DataRowVersion.Proposed;

						// Parameter version is ignored for non-update commands
						if (statementType == StatementType.Update) 
							rowVersion = parameter.SourceVersion;

						parameter.Value = row [dsColumnName, rowVersion];
					}
					row.AcceptChanges ();
				}
				updateCount += command.ExecuteNonQuery ();

				OnRowUpdated (CreateRowUpdatedEvent (row, command, statementType, tableMapping));
			}
			return updateCount;
		}

		public int Update (DataSet dataSet, string sourceTable) 
		{
			int result = 0;
			DataTableMapping tableMapping = TableMappings [sourceTable];
			foreach (DataTable table in dataSet.Tables)
				result += Update ((DataRow[]) table.Rows.List.ToArray (typeof (DataRow)), tableMapping);
			return result;
		}

		protected virtual void OnFillError (FillErrorEventArgs value) 
		{
			FillErrorEventHandler handler = (FillErrorEventHandler) Events [EventFillError];
			if (handler != null)
				handler (this, value);
		}

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
		
		#endregion
		
		#region Events

		public event FillErrorEventHandler FillError {
			add { Events.AddHandler (EventFillError, value); }
			remove { Events.RemoveHandler (EventFillError, value); }
		}

		#endregion
	}
}
