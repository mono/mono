//
// System.Data.SqlClient.SqlCommandBuilder.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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
using System.Data.Common;
using System.Text;

namespace System.Data.SqlClient
{
#if NET_2_0
	public sealed class SqlCommandBuilder : DbCommandBuilder
#else
	public sealed class SqlCommandBuilder : Component
#endif // NET_2_0
	{
		#region Fields

		bool disposed;

		DataTable dbSchemaTable;
		SqlDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;
		string tableName;
#if NET_2_0
		string _catalogSeperator = ".";
		string _schemaSeperator = ".";
		CatalogLocation _catalogLocation = CatalogLocation.Start;
#endif
	
		SqlCommand deleteCommand;
		SqlCommand insertCommand;
		SqlCommand updateCommand;

		// Used to construct WHERE clauses
		static readonly string clause1 = "({0} = 1 AND {1} IS NULL)";
		static readonly string clause2 = "({0} = {1})";

		private SqlRowUpdatingEventHandler rowUpdatingHandler;

		#endregion // Fields

		#region Constructors

		public SqlCommandBuilder ()
		{
			dbSchemaTable = null;
			adapter = null;
#if NET_2_0
			quoteSuffix = "]";
			quotePrefix = "[";
#else
			quoteSuffix = String.Empty;
			quotePrefix = String.Empty;
#endif
		}

		public SqlCommandBuilder (SqlDataAdapter adapter)
			: this ()
		{
			DataAdapter = adapter;
		}

		#endregion // Constructors

		#region Properties

#if !NET_2_0
		[DataSysDescription ("The DataAdapter for which to automatically generate SqlCommands")]
#endif
		[DefaultValue (null)]
		public new SqlDataAdapter DataAdapter {
			get { return adapter; }
			set { 
				if (adapter != null)
					adapter.RowUpdating -= new SqlRowUpdatingEventHandler (RowUpdatingHandler);

				adapter = value; 
				if (adapter != null)
					adapter.RowUpdating += new SqlRowUpdatingEventHandler (RowUpdatingHandler);
			}
		}

		private string QuotedTableName {
			get { return GetQuotedString (tableName); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if !NET_2_0
		[DataSysDescription ("The character used in a text command as the opening quote for quoting identifiers that contain special characters.")]
#else
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif // NET_2_0
		public
#if NET_2_0
		override
#endif // NET_2_0
		string QuotePrefix {
			get { return quotePrefix; }
			set {
				if (dbSchemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update, or Delete command has been generated.");
				quotePrefix = value; 
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if !NET_2_0
		[DataSysDescription ("The character used in a text command as the closing quote for quoting identifiers that contain special characters. ")]
#else
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif // NET_2_0
		public
#if NET_2_0
		override
#endif // NET_2_0
		string QuoteSuffix {
			get { return quoteSuffix; }
			set {
				if (dbSchemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update, or Delete command has been generated.");
				quoteSuffix = value; 
			}
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if !NET_2_0
		[DefaultValue (".")]
#endif
		public override string CatalogSeparator {
			get { return _catalogSeperator; }
			set { if (value != null) _catalogSeperator = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if !NET_2_0
		[DefaultValue (".")]
#endif
		public override string SchemaSeparator {
			get { return _schemaSeperator; }
			set {  if (value != null) _schemaSeperator = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if !NET_2_0
		[DefaultValue (CatalogLocation.Start)]
#endif
		public override CatalogLocation CatalogLocation {
			get { return _catalogLocation; }
			set { _catalogLocation = value; }
		}

#endif // NET_2_0

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
	
				SqlDataReader reader = sourceCommand.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
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
				if (schemaRow.IsNull ("BaseTableName") ||
				    (string) schemaRow ["BaseTableName"] == String.Empty)
					continue;

				if (tableName == String.Empty) 
					tableName = (string) schemaRow ["BaseTableName"];
				else if (tableName != (string) schemaRow["BaseTableName"])
					throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
			}
			if (tableName == String.Empty)
				throw new InvalidOperationException ("Dynamic SQL generation is not supported with no base table.");
			dbSchemaTable = schemaTable;
		}

		private SqlCommand CreateDeleteCommand (bool option)
		{
			// If no table was found, then we can't do an delete
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref deleteCommand);

			string command = String.Format ("DELETE FROM {0}", QuotedTableName);
			StringBuilder whereClause = new StringBuilder ();
			bool keyFound = false;
			int parmIndex = 1;

			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if ((bool)schemaRow["IsExpression"] == true)
					continue;
				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				SqlParameter parameter = null;

				if (isKey)
					keyFound = true;

				//ms.net 1.1 generates the null check for columns even if AllowDBNull is false
				//while ms.net 2.0 does not. Anyways, since both forms are logically equivalent
				//following the 2.0 approach
				bool allowNull = (bool) schemaRow ["AllowDBNull"];
				if (!isKey && allowNull) {
					if (option) {
						parameter = deleteCommand.Parameters.Add (String.Format ("@{0}",
													 schemaRow ["BaseColumnName"]),
											  SqlDbType.Int);
					} else {
						parameter = deleteCommand.Parameters.Add (String.Format ("@p{0}", parmIndex++),
											  SqlDbType.Int);
					}
					String sourceColumnName = (string) schemaRow ["BaseColumnName"];
					parameter.Value = 1;

					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, parameter.ParameterName, 
									GetQuotedString (sourceColumnName)));
					whereClause.Append (" OR ");
				}

				if (option) {
					parameter = deleteCommand.Parameters.Add (CreateParameter (schemaRow));
				} else {
					parameter = deleteCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
				}
				parameter.SourceVersion = DataRowVersion.Original;

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey && allowNull)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0} WHERE ({1})", command, whereClause.ToString ());
			deleteCommand.CommandText = sql;
			return deleteCommand;
		}

		private SqlCommand CreateInsertCommand (bool option)
		{
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref insertCommand);

			string command = String.Format ("INSERT INTO {0}", QuotedTableName);
			string sql;
			StringBuilder columns = new StringBuilder ();
			StringBuilder values = new StringBuilder ();

			int parmIndex = 1;
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (!IncludedInInsert (schemaRow))
					continue;

				if (parmIndex > 1) {
					columns.Append (", ");
					values.Append (", ");
				}

				SqlParameter parameter = null;
				if (option) {
					parameter = insertCommand.Parameters.Add (CreateParameter (schemaRow));
				} else {
					parameter = insertCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
				}
				parameter.SourceVersion = DataRowVersion.Current;

				columns.Append (GetQuotedString (parameter.SourceColumn));
				values.Append (parameter.ParameterName);
			}

			sql = String.Format ("{0} ({1}) VALUES ({2})", command, columns.ToString (), values.ToString ());
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
			command.Parameters.Clear ();
		}

		private SqlCommand CreateUpdateCommand (bool option)
		{
			// If no table was found, then we can't do an update
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref updateCommand);

			string command = String.Format ("UPDATE {0} SET ", QuotedTableName);
			StringBuilder columns = new StringBuilder ();
			StringBuilder whereClause = new StringBuilder ();
			int parmIndex = 1;
			bool keyFound = false;

			// First, create the X=Y list for UPDATE
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if (!IncludedInUpdate (schemaRow))
					continue;
				if (columns.Length > 0) 
					columns.Append (", ");

				SqlParameter parameter = null;
				if (option) {
					parameter = updateCommand.Parameters.Add (CreateParameter (schemaRow));
				} else {
					parameter = updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
				}
				parameter.SourceVersion = DataRowVersion.Current;

				columns.Append (String.Format ("{0} = {1}", GetQuotedString (parameter.SourceColumn), parameter.ParameterName));
			}

			// Now, create the WHERE clause.  This may be optimizable, but it would be ugly to incorporate
			// into the loop above.  "Premature optimization is the root of all evil." -- Knuth
			foreach (DataRow schemaRow in dbSchemaTable.Rows) {
				if ((bool)schemaRow["IsExpression"] == true)
					continue;

				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				SqlParameter parameter = null;


				if (isKey)
					keyFound = true;

				//ms.net 1.1 generates the null check for columns even if AllowDBNull is false
				//while ms.net 2.0 does not. Anyways, since both forms are logically equivalent
				//following the 2.0 approach
				bool allowNull = (bool) schemaRow ["AllowDBNull"];
				if (!isKey && allowNull) {
					if (option) {
						parameter = updateCommand.Parameters.Add (String.Format ("@{0}",
													 schemaRow ["BaseColumnName"]),
											  SqlDbType.Int);
					} else {
						parameter = updateCommand.Parameters.Add (String.Format ("@p{0}", parmIndex++),
											  SqlDbType.Int);
					}
					parameter.Value = 1;
					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, parameter.ParameterName,
									GetQuotedString ((string) schemaRow ["BaseColumnName"])));
					whereClause.Append (" OR ");
				}

				if (option) {
					parameter = updateCommand.Parameters.Add (CreateParameter (schemaRow));
				} else {
					parameter = updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
				}
				parameter.SourceVersion = DataRowVersion.Original;

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey && allowNull)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0}{1} WHERE ({2})", command, columns.ToString (), whereClause.ToString ());
			updateCommand.CommandText = sql;
			return updateCommand;
		}

		private SqlParameter CreateParameter (DataRow schemaRow)
		{
			string sourceColumn = (string) schemaRow ["BaseColumnName"];
			string name = String.Format ("@{0}", sourceColumn);
			SqlDbType sqlDbType = (SqlDbType) schemaRow ["ProviderType"];
			int size = (int) schemaRow ["ColumnSize"];

			return new SqlParameter (name, sqlDbType, size, sourceColumn);
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
			command.DeriveParameters ();
		}

#if NET_2_0
		new
#else
		protected override
#endif
		void Dispose (bool disposing)
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

		public 
#if NET_2_0
		new
#endif // NET_2_0
		SqlCommand GetDeleteCommand ()
		{
			BuildCache (true);
			if (deleteCommand == null)
				return CreateDeleteCommand (false);
			return deleteCommand;
		}

		public
#if NET_2_0
		new
#endif // NET_2_0
		SqlCommand GetInsertCommand ()
		{
			BuildCache (true);
			if (insertCommand == null)
				return CreateInsertCommand (false);
			return insertCommand;
		}

		private string GetQuotedString (string value)
		{
			if (value == String.Empty || value == null)
				return value;
			if (quotePrefix == String.Empty && quoteSuffix == String.Empty)
				return value;
			return String.Format ("{0}{1}{2}", quotePrefix, value, quoteSuffix);
		}

		public 
#if NET_2_0
		new
#endif // NET_2_0
		SqlCommand GetUpdateCommand ()
		{
			BuildCache (true);
			if (updateCommand == null)
				return CreateUpdateCommand (false);
			return updateCommand;
		}

#if NET_2_0
		public new SqlCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			BuildCache (true);
			if (updateCommand == null)
				return CreateUpdateCommand (useColumnsForParameterNames);
			return updateCommand;
		}

		public new SqlCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			BuildCache (true);
			if (deleteCommand == null)
				return CreateDeleteCommand (useColumnsForParameterNames);
			return deleteCommand;
		}

		public new SqlCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			BuildCache (true);
			if (insertCommand == null)
				return CreateInsertCommand (useColumnsForParameterNames);
			return insertCommand;
		}
		
		public override string QuoteIdentifier (string unquotedIdentifier)
		{
			return base.QuoteIdentifier (unquotedIdentifier);
		}
		
		public override string UnquoteIdentifier (string quotedIdentifier)
		{
			return base.UnquoteIdentifier (quotedIdentifier);
		}
		
#endif // NET_2_0

		private bool IncludedInInsert (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, Expression, RowVersion, ReadOnly

			if (!schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
				return false;
			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
				return false;
			if (!schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"])
				return false;
			if (!schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
				return false;
			if (!schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
				return false;
			return true;
		}

		private bool IncludedInUpdate (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, RowVersion

			if (!schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
				return false;
			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
				return false;
			if (!schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
				return false;
			if (!schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"])
				return false;
			if (!schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
				return false;

			return true;
		}

		private bool IncludedInWhereClause (DataRow schemaRow)
		{
			if ((bool) schemaRow ["IsLong"])
				return false;
			return true;
		}

#if NET_2_0
		new
#else
		public
#endif
		void RefreshSchema () 
		{
			// FIXME: "Figure out what else needs to be cleaned up when we refresh."
			tableName = String.Empty;
			dbSchemaTable = null;
			CreateNewCommand (ref deleteCommand);
			CreateNewCommand (ref updateCommand);
			CreateNewCommand (ref insertCommand);
		}

#if NET_2_0
		protected override void ApplyParameterInfo (DbParameter parameter,
		                                            DataRow datarow,
		                                            StatementType statementType,
		                                            bool whereClause)
		{
			SqlParameter sqlParam = (SqlParameter) parameter;
			sqlParam.Size = int.Parse (datarow ["ColumnSize"].ToString ());
			if (datarow ["NumericPrecision"] != DBNull.Value) {
				sqlParam.Precision = byte.Parse (datarow ["NumericPrecision"].ToString ());
			}
			if (datarow ["NumericScale"] != DBNull.Value) {
				sqlParam.Scale = byte.Parse (datarow ["NumericScale"].ToString ());
			}
			sqlParam.SqlDbType = (SqlDbType) datarow ["ProviderType"];
		}

		protected override string GetParameterName (int parameterOrdinal)
		{
			return String.Format ("@p{0}",  parameterOrdinal);
		}

		protected override string GetParameterName (string parameterName)
		{
			return String.Format ("@{0}", parameterName);
		}

		protected override string GetParameterPlaceholder (int parameterOrdinal)
		{
			return GetParameterName (parameterOrdinal);
		}
#endif // NET_2_0

		#endregion // Methods

		#region Event Handlers

		private void RowUpdatingHandler (object sender, SqlRowUpdatingEventArgs args)
		{
			if (args.Command != null)
				return;
			try {
				switch (args.StatementType) {
				case StatementType.Insert:
					args.Command = GetInsertCommand ();
					break;
				case StatementType.Update:
					args.Command = GetUpdateCommand ();
					break;
				case StatementType.Delete:
					args.Command = GetDeleteCommand ();
					break;
				}
			} catch (Exception e) {
				args.Errors = e;
				args.Status = UpdateStatus.ErrorsOccurred;
			}
		}

#if NET_2_0
		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
		{
			if (!(adapter is SqlDataAdapter)) {
				throw new InvalidOperationException ("Adapter needs to be a SqlDataAdapter");
			}
			rowUpdatingHandler = new SqlRowUpdatingEventHandler (RowUpdatingHandler);
			((SqlDataAdapter) adapter).RowUpdating += rowUpdatingHandler;
		}

		protected override DataTable GetSchemaTable (DbCommand srcCommand)
		{
			using (SqlDataReader rdr = (SqlDataReader) srcCommand.ExecuteReader ())
				return rdr.GetSchemaTable ();
		}

		protected override DbCommand InitializeCommand (DbCommand command)
		{
			if (command == null) {
				command = new SqlCommand ();
			} else {
				command.CommandTimeout = 30;
				command.Transaction = null;
				command.CommandType = CommandType.Text;
				command.UpdatedRowSource = UpdateRowSource.None;
			}
			return command;
		}
#endif // NET_2_0

		#endregion // Event Handlers
	}
}
