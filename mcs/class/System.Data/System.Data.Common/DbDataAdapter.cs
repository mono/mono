//
// System.Data.Common.DbDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Sureshkumar T <tsureshkumar@novell.com>
//   Veerapuram Varadhan  <vvaradhan@novell.com>
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
//

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
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Data.Common
{
#if NET_2_0
	public abstract class DbDataAdapter : DataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
#else
	public abstract class DbDataAdapter : DataAdapter, ICloneable
#endif
	{
		#region Fields

		public const string DefaultSourceTableName = "Table";
		const string DefaultSourceColumnName = "Column";
		CommandBehavior _behavior = CommandBehavior.Default;

#if NET_2_0
		IDbCommand _selectCommand;
		IDbCommand _updateCommand;
		IDbCommand _deleteCommand;
		IDbCommand _insertCommand;
#endif

		#endregion // Fields
		
		#region Constructors

		protected DbDataAdapter ()
		{
		}

		protected DbDataAdapter (DbDataAdapter adapter) : base (adapter)
		{
		}

		#endregion // Fields

		#region Properties

#if NET_2_0
		protected internal CommandBehavior FillCommandBehavior {
			get { return _behavior; }
			set { _behavior = value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand {
		    get { return ((DbDataAdapter)this).SelectCommand; }
		    set { ((DbDataAdapter)this).SelectCommand = (DbCommand)value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand{
		    get { return ((DbDataAdapter)this).UpdateCommand; }
		    set { ((DbDataAdapter)this).UpdateCommand = (DbCommand)value; }
		}
		
		IDbCommand IDbDataAdapter.DeleteCommand{
		    get { return ((DbDataAdapter)this).DeleteCommand; }
		    set { ((DbDataAdapter)this).DeleteCommand = (DbCommand)value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand{
		    get { return ((DbDataAdapter)this).InsertCommand; }
		    set { ((DbDataAdapter)this).InsertCommand = (DbCommand)value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DbCommand SelectCommand {
		    get {
					return (DbCommand) _selectCommand;
					//return (DbCommand) ((IDbDataAdapter)this).SelectCommand; 
			}
		    set {
					if (_selectCommand != value) {
						_selectCommand = value;
						((IDbDataAdapter)this).SelectCommand = value; 
					}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DbCommand DeleteCommand {
		    get {
					return (DbCommand) _deleteCommand;
					//return (DbCommand) ((IDbDataAdapter)this).DeleteCommand; 
			}
		    set {
					if (_deleteCommand != value) {
						_deleteCommand = value;
						((IDbDataAdapter)this).DeleteCommand = value; 
					}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DbCommand InsertCommand {
		    get {
					return (DbCommand) _insertCommand;
					//return (DbCommand) ((IDbDataAdapter)this).InsertCommand; 
			}
		    set {
					if (_insertCommand != value) {
						_insertCommand = value;
						((IDbDataAdapter)this).InsertCommand = value; 
					}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DbCommand UpdateCommand {
		    get {
					return (DbCommand) _updateCommand;
					//return (DbCommand) ((IDbDataAdapter)this).DeleteCommand; 
			}
		    set {
					if (_updateCommand != value) {
						_updateCommand = value;
						((IDbDataAdapter)this).UpdateCommand = value; 
					}
			}
		}

		[DefaultValue (1)]
		public virtual int UpdateBatchSize {
			get { return 1; }
			set {
				if (value != 1)
					throw new NotSupportedException ();
			}
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
		protected virtual RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command,
									     StatementType statementType,
									     DataTableMapping tableMapping)
		{
			return new RowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected virtual RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command,
									       StatementType statementType,
									       DataTableMapping tableMapping)
		{
			return new RowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected virtual void OnRowUpdated (RowUpdatedEventArgs value)
		{
			if (Events ["RowUpdated"] != null) {
				Delegate [] rowUpdatedList = Events ["RowUpdated"].GetInvocationList ();
				foreach (Delegate rowUpdated in rowUpdatedList) {
					MethodInfo rowUpdatedMethod = rowUpdated.Method;
					rowUpdatedMethod.Invoke (value, null);
				}
			}
		}

		protected virtual void OnRowUpdating (RowUpdatingEventArgs value)
		{
			if (Events ["RowUpdating"] != null) {
				Delegate [] rowUpdatingList = Events ["RowUpdating"].GetInvocationList ();
				foreach (Delegate rowUpdating in rowUpdatingList) {
					MethodInfo rowUpdatingMethod = rowUpdating.Method;
					rowUpdatingMethod.Invoke (value, null);
				}
			}
		}
#else
		protected abstract RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command,
									     StatementType statementType,
									     DataTableMapping tableMapping);

		protected abstract RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command,
									       StatementType statementType,
									       DataTableMapping tableMapping);

		protected abstract void OnRowUpdated (RowUpdatedEventArgs value);
		protected abstract void OnRowUpdating (RowUpdatingEventArgs value);
#endif

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

		public override int Fill (DataSet dataSet)
		{
			return Fill (dataSet, 0, 0, DefaultSourceTableName, ((IDbDataAdapter) this).SelectCommand, _behavior);
		}

		public int Fill (DataTable dataTable)
		{
			if (dataTable == null)
				throw new ArgumentNullException ("DataTable");

			return Fill (dataTable, ((IDbDataAdapter) this).SelectCommand, _behavior);
		}

		public int Fill (DataSet dataSet, string srcTable)
		{
			return Fill (dataSet, 0, 0, srcTable, ((IDbDataAdapter) this).SelectCommand, _behavior);
		}

#if !NET_2_0
		protected virtual int Fill (DataTable dataTable, IDataReader dataReader)
		{
			return base.FillInternal (dataTable, dataReader);
		}
#endif

		protected virtual int Fill (DataTable dataTable, IDbCommand command, CommandBehavior behavior)
		{
			CommandBehavior commandBehavior = behavior;

			// first see that the connection is not close.
			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				commandBehavior |= CommandBehavior.CloseConnection;
			}
			return Fill (dataTable, command.ExecuteReader (commandBehavior));
		}

		public int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable)
		{
			return this.Fill (dataSet, startRecord, maxRecords, srcTable, ((IDbDataAdapter) this).SelectCommand, _behavior);
		}

#if NET_2_0
		[MonoTODO]
		public int Fill (int startRecord, int maxRecords, params DataTable[] dataTables)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}
#else
		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			return base.FillInternal (dataSet, srcTable, dataReader, startRecord, maxRecords);
		}
#endif

		protected virtual int Fill (DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior)
		{
			if (command.Connection == null)
				throw new InvalidOperationException ("Connection state is closed");

			if (MissingSchemaAction == MissingSchemaAction.AddWithKey)
				behavior |= CommandBehavior.KeyInfo;
			CommandBehavior commandBehavior = behavior;

			if (command.Connection.State == ConnectionState.Closed) {
				command.Connection.Open ();
				commandBehavior |= CommandBehavior.CloseConnection;
			}
			return Fill (dataSet, srcTable, command.ExecuteReader (commandBehavior),
				startRecord, maxRecords);
		}

#if NET_2_0
		/// <summary>
		/// Fills the given datatable using values from reader. if a value 
		/// for a column is  null, that will be filled with default value. 
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
			while (reader.Read () && (length == 0 || counter < length)) {
				for (int i = 0 ; i < mapping.Length; i++)
					values [i] = mapping [i] < 0 ? null : reader [mapping [i]];
				table.BeginLoadData ();
				table.LoadDataRow (values, loadOption);
				table.EndLoadData ();
				counter++;
			}
			return counter;
		}

		internal static int FillFromReader (DataTable table,
                                                    IDataReader reader,
                                                    int start,
                                                    int length,
                                                    int [] mapping,
                                                    LoadOption loadOption,
                                                    FillErrorEventHandler errorHandler)
		{
			if (reader.FieldCount == 0)
				return 0 ;

			for (int i = 0; i < start; i++)
				reader.Read ();

			int counter = 0;
			object [] values = new object [mapping.Length];
			while (reader.Read () && (length == 0 || counter < length)) {
				for (int i = 0 ; i < mapping.Length; i++)
					values [i] = mapping [i] < 0 ? null : reader [mapping [i]];
				table.BeginLoadData ();
				try {
					table.LoadDataRow (values, loadOption);
				} catch (Exception e) {
					FillErrorEventArgs args = new FillErrorEventArgs (table, values);
					args.Errors = e;
					args.Continue = false;
					errorHandler (table, args);
					// if args.Continue is not set to true or if a handler is not set, rethrow the error..
					if(!args.Continue)
						throw e;
				}
				table.EndLoadData ();
				counter++;
			}
			return counter;
		}
#endif // NET_2_0

		public override DataTable [] FillSchema (DataSet dataSet, SchemaType schemaType)
		{
			return FillSchema (dataSet, schemaType, ((IDbDataAdapter) this).SelectCommand, DefaultSourceTableName, _behavior);
		}

		public DataTable FillSchema (DataTable dataTable, SchemaType schemaType)
		{
			return FillSchema (dataTable, schemaType, ((IDbDataAdapter) this).SelectCommand, _behavior);
		}

		public DataTable [] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable)
		{
			return FillSchema (dataSet, schemaType, ((IDbDataAdapter) this).SelectCommand, srcTable, _behavior);
		}

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
			try {
				string tableName =  SetupSchema (schemaType, dataTable.TableName);
				if (tableName != null) {
					// FillSchema should add the KeyInfo unless MissingSchemaAction
					// is set to Ignore or Error.
					MissingSchemaAction schemaAction = MissingSchemaAction;
					if (!(schemaAction == MissingSchemaAction.Ignore ||
						schemaAction == MissingSchemaAction.Error))
						schemaAction = MissingSchemaAction.AddWithKey;

					BuildSchema (reader, dataTable, schemaType, schemaAction,
						MissingMappingAction, TableMappings);
				}
			} finally {
				reader.Close ();
			}
			return dataTable;
		}

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
			try {
				// FillSchema should add the KeyInfo unless MissingSchemaAction
				// is set to Ignore or Error.
				MissingSchemaAction schemaAction = MissingSchemaAction;
				if (!(MissingSchemaAction == MissingSchemaAction.Ignore ||
					MissingSchemaAction == MissingSchemaAction.Error))
					schemaAction = MissingSchemaAction.AddWithKey;

				do {
					tableName = SetupSchema (schemaType, tableName);
					if (tableName != null) {
						if (dataSet.Tables.Contains (tableName))
							table = dataSet.Tables [tableName];
						else {
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
			} finally {
				reader.Close ();
			}
			return (DataTable []) output.ToArray (typeof (DataTable));
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override IDataParameter[] GetFillParameters ()
		{
			IDbCommand selectCmd = ((IDbDataAdapter) this).SelectCommand;
			IDataParameter[] parameters = new IDataParameter [selectCmd.Parameters.Count];
			selectCmd.Parameters.CopyTo (parameters, 0);
			return parameters;
		}
		
		[MonoTODO]
		[Obsolete ("use 'protected DbDataAdapter(DbDataAdapter)' ctor")]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public int Update (DataRow [] dataRows)
		{
			if (dataRows == null)
				throw new ArgumentNullException("dataRows");
			
			if (dataRows.Length == 0)
				return 0;

			if (dataRows [0] == null)
				throw new ArgumentException("dataRows[0].");

			DataTable table = dataRows [0].Table;
			if (table == null)
				throw new ArgumentException("table is null reference.");
			
			// all rows must be in the same table
			for (int i = 0; i < dataRows.Length; i++) {
				if (dataRows [i] == null)
					throw new ArgumentException ("dataRows[" + i + "].");
				if (dataRows [i].Table != table)
					throw new ArgumentException(
								    " DataRow["
								    + i
								    + "] is from a different DataTable than DataRow[0].");
			}
			
			// get table mapping for this rows
			DataTableMapping tableMapping = TableMappings.GetByDataSetTable(table.TableName);
			if (tableMapping == null) {
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

			DataRow[] copy = table.NewRowArray (dataRows.Length);
			Array.Copy (dataRows, 0, copy, 0, dataRows.Length);
			return Update (copy, tableMapping);
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
			if (tableMapping == null) {
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
			DataRow [] rows = dataTable.NewRowArray(dataTable.Rows.Count);
			dataTable.Rows.CopyTo (rows, 0);
			return Update (rows, tableMapping);
		}

		protected virtual int Update (DataRow [] dataRows, DataTableMapping tableMapping)
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
				OnRowUpdating (argsUpdating);
				switch (argsUpdating.Status) {
				case UpdateStatus.Continue :
					//continue in update operation
					break;
				case UpdateStatus.ErrorsOccurred :
					if (argsUpdating.Errors == null)
						argsUpdating.Errors = ExceptionHelper.RowUpdatedError();
					row.RowError += argsUpdating.Errors.Message;
					if (!ContinueUpdateOnError)
						throw argsUpdating.Errors;
					continue;
				case UpdateStatus.SkipAllRemainingRows :
					return updateCount;
				case UpdateStatus.SkipCurrentRow :
					updateCount++;
					continue;
				default :
					throw ExceptionHelper.InvalidUpdateStatus (argsUpdating.Status);
				}
				command = argsUpdating.Command;
				try {
					if (command != null) {
						DataColumnMappingCollection columnMappings = tableMapping.ColumnMappings;
#if ONLY_1_1
						IDataParameter nullCheckParam = null;
#endif
						foreach (IDataParameter parameter in command.Parameters) {
							if ((parameter.Direction & ParameterDirection.Input) == 0)
								continue;

							DataRowVersion rowVersion = parameter.SourceVersion;
							// Parameter version is ignored for non-update commands
							if (statementType == StatementType.Delete)
								rowVersion = DataRowVersion.Original;

							string dsColumnName = parameter.SourceColumn;
#if NET_2_0
							if (columnMappings.Contains(dsColumnName)) {
								dsColumnName = columnMappings [dsColumnName].DataSetColumn;
								parameter.Value = row [dsColumnName, rowVersion];
							} else {
								parameter.Value = null;
							}

							DbParameter nullCheckParam = parameter as DbParameter;
#else
							if (columnMappings.Contains(dsColumnName))
								dsColumnName = columnMappings [dsColumnName].DataSetColumn;
							if (dsColumnName == null || dsColumnName.Length == 0) {
								nullCheckParam = parameter;
								continue;
							}
							parameter.Value = row [dsColumnName, rowVersion];
#endif

#if NET_2_0
							if (nullCheckParam != null && nullCheckParam.SourceColumnNullMapping) {
#else
							if (nullCheckParam != null) {
#endif
								if (parameter.Value != null && parameter.Value != DBNull.Value)
									nullCheckParam.Value = 0;
								else
									nullCheckParam.Value = 1;
								nullCheckParam = null;
							}
						}
					}
				} catch (Exception e) {
					argsUpdating.Errors = e;
					argsUpdating.Status = UpdateStatus.ErrorsOccurred;
				}

				IDataReader reader = null;
				try {
					if (command == null)
						throw ExceptionHelper.UpdateRequiresCommand (commandName);
				
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
							commandName +"Command affected 0 records.", null,
							new DataRow [] { row });
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

					RowUpdatedEventArgs updatedArgs = CreateRowUpdatedEvent (row, command, statementType, tableMapping);
					OnRowUpdated (updatedArgs);
					switch (updatedArgs.Status) {
					case UpdateStatus.Continue:
						break;
					case UpdateStatus.ErrorsOccurred:
						if (updatedArgs.Errors == null)
							updatedArgs.Errors = ExceptionHelper.RowUpdatedError();
						row.RowError += updatedArgs.Errors.Message;
						if (!ContinueUpdateOnError)
							throw updatedArgs.Errors;
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
				} catch (Exception e) {
					row.RowError = e.Message;
					if (!ContinueUpdateOnError)
						throw e;
				} finally {
					if (reader != null && ! reader.IsClosed)
						reader.Close ();
				}
			}
			return updateCount;
		}

		public int Update (DataSet dataSet, string srcTable)
		{
			MissingMappingAction mappingAction = MissingMappingAction;

			if (mappingAction == MissingMappingAction.Ignore)
				mappingAction = MissingMappingAction.Error;

			DataTableMapping tableMapping = DataTableMappingCollection.GetTableMappingBySchemaAction (TableMappings, srcTable, srcTable, mappingAction);

			DataTable dataTable = dataSet.Tables [tableMapping.DataSetTable];
			if (dataTable == null)
				throw new ArgumentException (String.Format ("Missing table {0}",
									    srcTable));

			/** Copied from another Update function **/
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
			/**end insert from another update**/
			return Update (dataTable, tableMapping);
		}

#if NET_2_0
		// All the batch methods, should be implemented, if supported,
		// by individual providers

		protected virtual int AddToBatch (IDbCommand command)
		{
			throw CreateMethodNotSupportedException ();
		}

		protected virtual void ClearBatch ()
		{
			throw CreateMethodNotSupportedException ();
		}

		protected virtual int ExecuteBatch ()
		{
			throw CreateMethodNotSupportedException ();
		}

		protected virtual IDataParameter GetBatchedParameter (int commandIdentifier, int parameterIndex)
		{
			throw CreateMethodNotSupportedException ();
		}

		protected virtual bool GetBatchedRecordsAffected (int commandIdentifier, out int recordsAffected, out Exception error)
		{
			recordsAffected = 1;
			error = null;
			return true;
		}

		protected virtual void InitializeBatching ()
		{
			throw CreateMethodNotSupportedException ();
		}

		protected virtual void TerminateBatching ()
		{
			throw CreateMethodNotSupportedException ();
		}

		Exception CreateMethodNotSupportedException ()
		{
			return new NotSupportedException ("Method is not supported.");
		}
#else
		internal override void OnFillErrorInternal (FillErrorEventArgs value)
		{
			OnFillError (value);
		}

		protected virtual void OnFillError (FillErrorEventArgs value)
		{
			if (FillError != null)
				FillError (this, value);
		}
#endif
		#endregion // Methods
	}
}
