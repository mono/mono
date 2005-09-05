/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/overview/*'/>
#if	(NET)
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(FbDataAdapter), "Resources.FbDataAdapter.bmp")]
	[DefaultEvent("RowUpdated")]
	[DesignerAttribute(typeof(Design.FbDataAdapterDesigner), typeof(System.ComponentModel.Design.IDesigner))]
#endif
	public sealed class FbDataAdapter : DbDataAdapter, IDbDataAdapter
	{
		#region Static Fields

		private static readonly object EventRowUpdated = new object();
		private static readonly object EventRowUpdating = new object();

		#endregion

		#region Events

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdated"]/*'/>
		public event FbRowUpdatedEventHandler RowUpdated
		{
			add { base.Events.AddHandler(EventRowUpdated, value); }
			remove { base.Events.RemoveHandler(EventRowUpdated, value); }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/event[@name="RowUpdating"]/*'/>
		public event FbRowUpdatingEventHandler RowUpdating
		{
			add
			{
				base.Events.AddHandler(EventRowUpdating, value);
			}

			remove
			{
				base.Events.RemoveHandler(EventRowUpdating, value);
			}
		}

		#endregion

		#region Fields

		private FbCommand selectCommand;
		private FbCommand insertCommand;
		private FbCommand updateCommand;
		private FbCommand deleteCommand;

		private bool disposed;

		#endregion

		#region Properties

		IDbCommand IDbDataAdapter.SelectCommand
		{
			get { return this.selectCommand; }
			set { this.selectCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/property[@name="SelectCommand"]/*'/>
#if	(!NETCF)
		[Category("Fill"), DefaultValue(null)]
#endif
		public FbCommand SelectCommand
		{
			get { return this.selectCommand; }
			set { this.selectCommand = value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand
		{
			get { return this.insertCommand; }
			set { this.insertCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/property[@name="InsertCommand"]/*'/>
#if	(!NETCF)
		[Category("Update"), DefaultValue(null)]
#endif
		public FbCommand InsertCommand
		{
			get { return this.insertCommand; }
			set { this.insertCommand = value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand
		{
			get { return this.updateCommand; }
			set { this.updateCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/property[@name="UpdateCommand"]/*'/>		
#if	(!NETCF)
		[Category("Update"), DefaultValue(null)]
#endif
		public FbCommand UpdateCommand
		{
			get { return this.updateCommand; }
			set { this.updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand
		{
			get { return this.deleteCommand; }
			set { this.deleteCommand = (FbCommand)value; }
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/property[@name="DeleteCommand"]/*'/>
#if	(!NETCF)
		[Category("Update"), DefaultValue(null)]
#endif
		public FbCommand DeleteCommand
		{
			get { return this.deleteCommand; }
			set { this.deleteCommand = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor"]/*'/>
		public FbDataAdapter() : base()
		{
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(FbCommand)"]/*'/>
		public FbDataAdapter(FbCommand selectCommand) : base()
		{
			this.SelectCommand = selectCommand;
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,FbConnection)"]/*'/>		
		public FbDataAdapter(string selectCommandText, FbConnection selectConnection)
			: base()
		{
			this.SelectCommand = new FbCommand(selectCommandText, selectConnection);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/constructor[@name="ctor(System.String,System.String)"]/*'/>
		public FbDataAdapter(string selectCommandText, string selectConnectionString)
			: base()
		{
			FbConnection connection = new FbConnection(selectConnectionString);
			this.SelectCommand = new FbCommand(selectCommandText, connection);
		}

		#endregion

		#region IDisposable	Methods

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/method[@name="Dispose(System.Boolean)"]/*'/>
		protected override void Dispose(bool disposing)
		{
			lock (this)
			{
				if (!this.disposed)
				{
					try
					{
						// Release any managed resources
						if (disposing)
						{
							if (this.SelectCommand != null)
							{
								this.SelectCommand.Dispose();
							}
							if (this.InsertCommand != null)
							{
								this.InsertCommand.Dispose();
							}
							if (this.UpdateCommand != null)
							{
								this.UpdateCommand.Dispose();
							}
						}

						// release any unmanaged resources

						this.disposed = true;
					}
					finally
					{
						base.Dispose(disposing);
					}
				}
			}
		}

		#endregion

		#region Protected Methods

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatingEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(
			DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/method[@name="CreateRowUpdatedEvent(System.Data.DataRow,System.Data.IDbCommand,System.Data.StatementType,System.Data.Common.DataTableMapping)"]/*'/>
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(
			DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new FbRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdating(System.Data.Common.RowUpdatingEventArgs)"]/*'/>
		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			FbRowUpdatingEventHandler handler = null;

			handler = (FbRowUpdatingEventHandler)base.Events[EventRowUpdating];

			if ((null != handler) &&
				(value is FbRowUpdatingEventArgs) &&
				(value != null))
			{
				handler(this, (FbRowUpdatingEventArgs)value);
			}
		}

		/// <include file='Doc/en_EN/FbDataAdapter.xml'	path='doc/class[@name="FbDataAdapter"]/method[@name="OnRowUpdated(System.Data.Common.RowUpdatedEventArgs)"]/*'/>
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			FbRowUpdatedEventHandler handler = null;

			handler = (FbRowUpdatedEventHandler)base.Events[EventRowUpdated];

			if ((handler != null) &&
				(value is FbRowUpdatedEventArgs) &&
				(value != null))
			{
				handler(this, (FbRowUpdatedEventArgs)value);
			}
		}

		#endregion

		#region Update DataRow Collection

		/// <summary>
		/// Review .NET	Framework documentation.
		/// </summary>
		protected override int Update(DataRow[] dataRows, DataTableMapping tableMapping)
		{
			int						updated			= 0;
			IDbCommand				command			= null;
			StatementType			statementType	= StatementType.Insert;
			ArrayList				connections		= new ArrayList();
			RowUpdatingEventArgs	updatingArgs	= null;
			Exception				updateException = null;

			foreach (DataRow row in dataRows)
			{
				if (row.RowState == DataRowState.Detached ||
					row.RowState == DataRowState.Unchanged)
				{
					continue;
				}

				switch (row.RowState)
				{
					case DataRowState.Added:
						command = this.insertCommand;
						statementType = StatementType.Insert;
						break;

					case DataRowState.Modified:
						command = this.updateCommand;
						statementType = StatementType.Update;
						break;

					case DataRowState.Deleted:
						command = this.deleteCommand;
						statementType = StatementType.Delete;
						break;
				}

				/* The order of	execution can be reviewed in the .NET 1.1 documentation
					*
					* 1. The values in	the	DataRow	are	moved to the parameter values. 
					* 2. The OnRowUpdating	event is raised. 
					* 3. The command executes.	
					* 4. If the command is	set	to FirstReturnedRecord,	then the first returned	result is placed in	the	DataRow. 
					* 5. If there are output parameters, they are placed in the DataRow. 
					* 6. The OnRowUpdated event is	raised.	
					* 7 AcceptChanges is called. 
					*/

				try
				{
					/* 1. Update Parameter values (It's	very similar to	what we	
					 * are doing in	the	FbCommandBuilder class).
					 *
					 * Only	input parameters should	be updated.
					 */
					if (command != null && command.Parameters.Count > 0)
					{
						this.UpdateParameterValues(command, statementType, row, tableMapping);
					}

					// 2. Raise	RowUpdating	event
					updatingArgs = this.CreateRowUpdatingEvent(row, command, statementType, tableMapping);
					this.OnRowUpdating(updatingArgs);

					if (updatingArgs.Status == UpdateStatus.SkipAllRemainingRows)
					{
						break;
					}
					else if (updatingArgs.Status == UpdateStatus.ErrorsOccurred)
					{
						if (updatingArgs.Errors == null)
						{
							throw new InvalidOperationException("RowUpdatingEvent: Errors occurred; no additional is information available.");
						}
						throw updatingArgs.Errors;
					}
					else if (updatingArgs.Status == UpdateStatus.SkipCurrentRow)
					{
					}
					else if (updatingArgs.Status == UpdateStatus.Continue)
					{
						if (command != updatingArgs.Command)
						{
							command = updatingArgs.Command;
						}
						if (command == null)
						{
							/* Samples of exceptions thrown	by DbDataAdapter class
								*
								*	Update requires	a valid	InsertCommand when passed DataRow collection with new rows
								*	Update requires	a valid	UpdateCommand when passed DataRow collection with modified rows.
								*	Update requires	a valid	DeleteCommand when passed DataRow collection with deleted rows.
								*/
							string message = this.CreateExceptionMessage(statementType);
							throw new InvalidOperationException(message);
						}

						/* Validate that the command has a connection */
						if (command.Connection == null)
						{
							throw new InvalidOperationException("Update requires a command with a valid connection.");
						}

						// 3. Execute the command
						if (command.Connection.State == ConnectionState.Closed)
						{
							command.Connection.Open();
							// Track command connection
							connections.Add(command.Connection);
						}

						int rowsAffected = command.ExecuteNonQuery();
						if (rowsAffected == 0)
						{
							throw new DBConcurrencyException("An attempt to execute an INSERT, UPDATE, or DELETE statement resulted in zero records affected.");
						}

						updated++;

						/* 4. If the command is	set	to FirstReturnedRecord,	then the 
							* first returned result is	placed in the DataRow. 
							* 
							* We have nothing to do in	this case as there are no 
							* support for batch commands.
							*/

						/* 5. Check	if we have output parameters and they should 
							* be updated.
							*
							* Only	output paraneters should be	updated
							*/
						if (command.UpdatedRowSource == UpdateRowSource.OutputParameters ||
							command.UpdatedRowSource == UpdateRowSource.Both)
						{
							// Process output parameters
							foreach (IDataParameter parameter in command.Parameters)
							{
								if ((parameter.Direction == ParameterDirection.Output ||
									parameter.Direction == ParameterDirection.ReturnValue ||
									parameter.Direction == ParameterDirection.InputOutput) &&
									parameter.SourceColumn != null &&
									parameter.SourceColumn.Length > 0)
								{
									DataColumn column = null;

									DataColumnMapping columnMapping = tableMapping.GetColumnMappingBySchemaAction(
										parameter.SourceColumn,
										this.MissingMappingAction);

									if (columnMapping != null)
									{
										column = columnMapping.GetDataColumnBySchemaAction(
											row.Table,
											null,
											this.MissingSchemaAction);

										if (column != null)
										{
											row[column] = parameter.Value;
										}
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					row.RowError	= ex.Message;
					updateException = ex;
				}

				if (updatingArgs.Status == UpdateStatus.Continue)
				{
					// 6. Raise	RowUpdated event
					RowUpdatedEventArgs	updatedArgs = this.CreateRowUpdatedEvent(row, command, statementType, tableMapping);
					this.OnRowUpdated(updatedArgs);

					if (updatedArgs.Status == UpdateStatus.SkipAllRemainingRows)
					{
						break;
					}
					else if (updatedArgs.Status == UpdateStatus.ErrorsOccurred)
					{
						if (updatingArgs.Errors == null)
						{
							throw new InvalidOperationException("RowUpdatedEvent: Errors occurred; no additional information available.");
						}
						throw updatedArgs.Errors;
					}
					else if (updatedArgs.Status == UpdateStatus.SkipCurrentRow)
					{
					}
					else if (updatingArgs.Status == UpdateStatus.Continue)
					{
						// If the update result is an exception throw it
						if (!this.ContinueUpdateOnError && updateException != null)
						{
							this.CloseConnections(connections);
							throw updateException;
						}

						// 7. Call AcceptChanges
						row.AcceptChanges();
					}
				}
				else
				{
					// If the update result is an exception throw it
					if (!this.ContinueUpdateOnError && updateException != null)
					{
						this.CloseConnections(connections);
						throw updateException;
					}
				}

				updateException = null;
			}

			this.CloseConnections(connections);

			return updated;
		}

		#endregion

		#region Private	Methods

		private string CreateExceptionMessage(StatementType statementType)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append("Update requires a valid ");
			sb.Append(statementType.ToString());
			sb.Append("Command when passed DataRow collection with ");

			switch (statementType)
			{
				case StatementType.Insert:
					sb.Append("new");
					break;

				case StatementType.Update:
					sb.Append("modified");
					break;

				case StatementType.Delete:
					sb.Append("deleted");
					break;
			}

			sb.Append(" rows.");

			return sb.ToString();
		}

		private void UpdateParameterValues(
			IDbCommand command,
			StatementType statementType,
			DataRow row,
			DataTableMapping tableMapping)
		{
			foreach (IDataParameter parameter in command.Parameters)
			{
				// Process only	input parameters
				if (parameter.Direction != ParameterDirection.Input &&
					parameter.Direction != ParameterDirection.InputOutput)
				{
					continue;
				}

				DataColumn column = null;

				/* Get the DataColumnMapping that matches the given
				 * column name
				 */
				DataColumnMapping columnMapping = tableMapping.GetColumnMappingBySchemaAction(
					parameter.SourceColumn,
					this.MissingMappingAction);

				if (columnMapping != null)
				{
					column = columnMapping.GetDataColumnBySchemaAction(
						row.Table,
						null,
						this.MissingSchemaAction);

					if (column != null)
					{
						DataRowVersion dataRowVersion = DataRowVersion.Default;

						if (statementType == StatementType.Insert)
						{
							dataRowVersion = DataRowVersion.Current;
						}
						else if (statementType == StatementType.Update)
						{
							dataRowVersion = parameter.SourceVersion;
						}
						else if (statementType == StatementType.Delete)
						{
							dataRowVersion = DataRowVersion.Original;
						}

						parameter.Value = row[column, dataRowVersion];
					}
				}
			}
		}

		private void CloseConnections(ArrayList connections)
		{
			foreach (IDbConnection c in connections)
			{
				c.Close();
			}
			connections.Clear();
		}

		#endregion
	}
}