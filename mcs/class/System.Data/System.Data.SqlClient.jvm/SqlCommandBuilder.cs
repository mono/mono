//
// System.Data.SqlClient.SqlCommandBuilder.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Text;
using System.Data.ProviderBase;

namespace System.Data.SqlClient {
	public sealed class SqlCommandBuilder : AbstractDbCommandBuilder
	{
		#region Fields

		bool disposed = false;

		DataTable dbSchemaTable;
		SqlDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;
		string[] columnNames;
		string tableName;
	
		SqlCommand deleteCommand;
		SqlCommand insertCommand;
		SqlCommand updateCommand;

		// Used to construct WHERE clauses
		static readonly string clause1 = "({0} IS NULL AND {1} IS NULL)";
		static readonly string clause2 = "({0} = {1})";

		#endregion // Fields

		#region Constructors

		public SqlCommandBuilder () 
		{
			dbSchemaTable = null;
			adapter = null;
			quoteSuffix = String.Empty;
			quotePrefix = String.Empty;
		}

		public SqlCommandBuilder (SqlDataAdapter adapter)
			: this ()
		{
			DataAdapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("The DataAdapter for which to automatically generate SqlCommands")]
		[DefaultValue (null)]
		public SqlDataAdapter DataAdapter {
			get { return adapter; }
			set { 
				adapter = value; 
				if (adapter != null)
					adapter.RowUpdating += new SqlRowUpdatingEventHandler (RowUpdatingHandler);
			}
		}

		private string QuotedTableName {
			get { return GetQuotedString (tableName); }
		}

		[Browsable (false)]
		[DataSysDescription ("The character used in a text command as the opening quote for quoting identifiers that contain special characters.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string QuotePrefix {
			get { return quotePrefix; }
			set { 
				if (dbSchemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update, or Delete command has been generated.");
				quotePrefix = value; 
			}
		}

		[Browsable (false)]
		[DataSysDescription ("The character used in a text command as the closing quote for quoting identifiers that contain special characters.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string QuoteSuffix {
			get { return quoteSuffix; }
			set {
				if (dbSchemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update, or Delete command has been generated.");
				quoteSuffix = value; 
			}
		}

		private SqlCommand SourceCommand {
			get {
				if (adapter != null)
					return adapter.SelectCommand;
				return null;
			}
		}

		#endregion // Properties

		#region Methods

		private void BuildCache (bool closeConnection)
		{
			SqlCommand sourceCommand = SourceCommand;
			if (sourceCommand == null)
				throw new InvalidOperationException ("The DataAdapter.SelectCommand property needs to be initialized.");
			SqlConnection connection = sourceCommand.Connection;
			if (connection == null)
				throw new InvalidOperationException ("The DataAdapter.SelectCommand.Connection property needs to be initialized.");
				
			if (dbSchemaTable == null) {
				if (connection.State == ConnectionState.Open)
					closeConnection = false;	
				else
					connection.Open ();
	
				SqlDataReader reader = (SqlDataReader)sourceCommand.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				dbSchemaTable = reader.GetSchemaTable ();
				reader.Close ();
				if (closeConnection)
					connection.Close ();	
				BuildInformation (dbSchemaTable);
			}
		}
		
		private void BuildInformation (DataTable schemaTable)
		{
			tableName = String.Empty;
			foreach (DataRow schemaRow in schemaTable.Rows) {
				if (tableName == String.Empty) 
					tableName = (string) schemaRow ["BaseTableName"];
				if (tableName != (string) schemaRow["BaseTableName"])
					throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
			}
			dbSchemaTable = schemaTable;
		}

		private SqlCommand CreateDeleteCommand (DataRow row, DataTableMapping tableMapping) 
		{
			// If no table was found, then we can't do an delete
			if (QuotedTableName == String.Empty)
				return null;


			CreateNewCommand (ref deleteCommand);

			string command = String.Format ("DELETE FROM {0} ", QuotedTableName);
			StringBuilder columns = new StringBuilder ();
			StringBuilder whereClause = new StringBuilder ();
			string dsColumnName = String.Empty;
			bool keyFound = false;
			int parmIndex = 1;

			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				SqlParameter parameter = null;

				if (!isKey) {
					parameter = deleteCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

					dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
					if (row != null)
						parameter.Value = row [dsColumnName, DataRowVersion.Current];
					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));
					whereClause.Append (" OR ");
				}
				else
					keyFound = true;
					
				parameter = deleteCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

				dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
				if (row != null)
					parameter.Value = row [dsColumnName, DataRowVersion.Current];

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0} WHERE ( {1} )", command, whereClause.ToString ());
			deleteCommand.CommandText = sql;
			return deleteCommand;
		}

		private SqlCommand CreateInsertCommand (DataRow row, DataTableMapping tableMapping) 
		{
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref insertCommand);

			string command = String.Format ("INSERT INTO {0}", QuotedTableName);
			string sql;
			StringBuilder columns = new StringBuilder ();
			StringBuilder values = new StringBuilder ();
			string dsColumnName = String.Empty;

			int parmIndex = 1;
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (!IncludedInInsert (schemaRow))
					continue;

				if (parmIndex > 1) {
					columns.Append (" , ");
					values.Append (" , ");
				}

				SqlParameter parameter = insertCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

				dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
				if (row != null)
					parameter.Value = row [dsColumnName];

				columns.Append (GetQuotedString (parameter.SourceColumn));
				values.Append (parameter.ParameterName);
			}

			sql = String.Format ("{0}( {1} ) VALUES ( {2} )", command, columns.ToString (), values.ToString ());
			insertCommand.CommandText = sql;
			return insertCommand;
		}

		private void CreateNewCommand (ref SqlCommand command)
		{
			SqlCommand sourceCommand = SourceCommand;
			if (command == null) {
				command = sourceCommand.Connection.CreateCommand ();
				command.CommandTimeout = sourceCommand.CommandTimeout;
				command.Transaction = sourceCommand.Transaction;
			}
			command.CommandType = CommandType.Text;
			command.UpdatedRowSource = UpdateRowSource.None;
		}

		private SqlCommand CreateUpdateCommand (DataRow row, DataTableMapping tableMapping) 
		{
			// If no table was found, then we can't do an update
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref updateCommand);

			string command = String.Format ("UPDATE {0} SET ", QuotedTableName);
			StringBuilder columns = new StringBuilder ();
			StringBuilder whereClause = new StringBuilder ();
			int parmIndex = 1;
			string dsColumnName = String.Empty;
			bool keyFound = false;

			// First, create the X=Y list for UPDATE
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (columns.Length > 0) 
					columns.Append (" , ");

				SqlParameter parameter = updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

				dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
				if (row != null)
					parameter.Value = row [dsColumnName, DataRowVersion.Proposed];

				columns.Append (String.Format ("{0} = {1}", GetQuotedString (parameter.SourceColumn), parameter.ParameterName));
			}

			// Now, create the WHERE clause.  This may be optimizable, but it would be ugly to incorporate
			// into the loop above.  "Premature optimization is the root of all evil." -- Knuth
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				SqlParameter parameter = null;


				if (!isKey) {
					parameter = updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

					dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
					if (row != null)
						parameter.Value = row [dsColumnName];

					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));
					whereClause.Append (" OR ");
				}
				else
					keyFound = true;
					
				parameter = updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));

				dsColumnName = tableMapping.ColumnMappings [parameter.SourceColumn].DataSetColumn;
				if (row != null)
					parameter.Value = row [dsColumnName];

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0}{1} WHERE ( {2} )", command, columns.ToString (), whereClause.ToString ());
			updateCommand.CommandText = sql;
			return updateCommand;
		}

		private SqlParameter CreateParameter (int parmIndex, DataRow schemaRow)
		{
			string name = String.Format ("@p{0}", parmIndex);
			string sourceColumn = (string) schemaRow ["BaseColumnName"];
			SqlDbType sqlDbType = (SqlDbType) schemaRow ["ProviderType"];
			int size = (int) schemaRow ["ColumnSize"];

			return new SqlParameter (name, sqlDbType, size, sourceColumn);
		}

		public static void DeriveParameters (SqlCommand command)
		{
			DeriveParameters((AbstractDbCommand)command);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					if (insertCommand != null)
						insertCommand.Dispose ();
					if (deleteCommand != null)
						deleteCommand.Dispose ();
					if (updateCommand != null)
						updateCommand.Dispose ();
					if (dbSchemaTable != null)
						dbSchemaTable.Dispose ();
				}
				disposed = true;
			}
		}

		public SqlCommand GetDeleteCommand ()
		{
			BuildCache (true);
			return CreateDeleteCommand (null, null);
		}

		public SqlCommand GetInsertCommand ()
		{
			BuildCache (true);
			return CreateInsertCommand (null, null);
		}

		private string GetQuotedString (string value)
		{
			if (value == String.Empty || value == null)
				return value;
			if (quotePrefix == String.Empty && quoteSuffix == String.Empty)
				return value;
			return String.Format ("{0}{1}{2}", quotePrefix, value, quoteSuffix);
		}

		public SqlCommand GetUpdateCommand ()
		{
			BuildCache (true);
			return CreateUpdateCommand (null, null);
		}

		private bool IncludedInInsert (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, Expression, RowVersion, ReadOnly

			if ((bool) schemaRow ["IsAutoIncrement"])
				return false;
			if ((bool) schemaRow ["IsHidden"])
				return false;
			if ((bool) schemaRow ["IsExpression"])
				return false;
			if ((bool) schemaRow ["IsRowVersion"])
				return false;
			if ((bool) schemaRow ["IsReadOnly"])
				return false;
			return true;
		}

		private bool IncludedInUpdate (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, RowVersion

			if ((bool) schemaRow ["IsAutoIncrement"])
				return false;
			if ((bool) schemaRow ["IsHidden"])
				return false;
			if ((bool) schemaRow ["IsRowVersion"])
				return false;
			return true;
		}

		private bool IncludedInWhereClause (DataRow schemaRow)
		{
			if ((bool) schemaRow ["IsLong"])
				return false;
			return true;
		}

		[MonoTODO ("Figure out what else needs to be cleaned up when we refresh.")]
		public void RefreshSchema () 
		{
			tableName = String.Empty;
			dbSchemaTable = null;
		}

		#endregion // Methods

		#region Event Handlers

		private void RowUpdatingHandler (object sender, SqlRowUpdatingEventArgs e)
		{
			if (e.Status != UpdateStatus.Continue)
				return;

			switch (e.StatementType) {
			case StatementType.Delete:
				deleteCommand = e.Command;
				break;
			case StatementType.Insert:
				insertCommand = e.Command;
				break;
			case StatementType.Update:
				updateCommand = e.Command;
				break;
			default:
				return;
			}

			try {
				BuildCache (false);

				switch (e.StatementType) {
				case StatementType.Delete:
					e.Command = CreateDeleteCommand (e.Row, e.TableMapping);
					e.Status = UpdateStatus.Continue;
					break;
				case StatementType.Insert:
					e.Command = CreateInsertCommand (e.Row, e.TableMapping);
					e.Status = UpdateStatus.Continue;
					break;
				case StatementType.Update:
					e.Command = CreateUpdateCommand (e.Row, e.TableMapping);
					e.Status = UpdateStatus.Continue;
					break;
				}

				if (e.Command != null && e.Row != null) {
					e.Row.AcceptChanges ();
					e.Status = UpdateStatus.SkipCurrentRow;
				}
			}
			catch (Exception exception) {
				e.Errors = exception;
				e.Status = UpdateStatus.ErrorsOccurred;
			}
		}

		#endregion // Event Handlers
	}
}

