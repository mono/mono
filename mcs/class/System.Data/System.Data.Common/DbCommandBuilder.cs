//
// System.Data.Common.DbCommandBuilder
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0 || TARGET_JVM

using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;

namespace System.Data.Common {
	public abstract class DbCommandBuilder : Component
	{
		bool _setAllValues;
		bool _disposed;

		DataTable _dbSchemaTable;
		DbDataAdapter _dbDataAdapter;
		private CatalogLocation _catalogLocation = CatalogLocation.Start;
		private ConflictOption _conflictOption = ConflictOption.CompareAllSearchableValues;

		private string _tableName;
		private string _catalogSeparator;
		private string _quotePrefix;
		private string _quoteSuffix;
		private string _schemaSeparator;
		private DbCommand _dbCommand;

		DbCommand _deleteCommand;
		DbCommand _insertCommand;
		DbCommand _updateCommand;

		static readonly string SEPARATOR_DEFAULT = ".";
		// Used to construct WHERE clauses
		static readonly string clause1 = "({0} = 1 AND {1} IS NULL)";
		static readonly string clause2 = "({0} = {1})";

		protected DbCommandBuilder ()
		{
		}

		private void BuildCache (bool closeConnection)
		{
			DbCommand sourceCommand = SourceCommand;
			if (sourceCommand == null)
				throw new InvalidOperationException ("The DataAdapter.SelectCommand property needs to be initialized.");
			DbConnection connection = sourceCommand.Connection;
			if (connection == null)
				throw new InvalidOperationException ("The DataAdapter.SelectCommand.Connection property needs to be initialized.");

			if (_dbSchemaTable == null) {
				if (connection.State == ConnectionState.Open)
					closeConnection = false;	
				else
					connection.Open ();
	
				DbDataReader reader = sourceCommand.ExecuteReader (CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
				_dbSchemaTable = reader.GetSchemaTable ();
				reader.Close ();
				if (closeConnection)
					connection.Close ();	
				BuildInformation (_dbSchemaTable);
			}
		}
		
		private string QuotedTableName {
			get { return GetQuotedString (_tableName); }
		}

		bool IsCommandGenerated {
			get {
				return (_insertCommand != null || _updateCommand != null || _deleteCommand != null);
			}
		}

		private string GetQuotedString (string value)
		{
			if (value == String.Empty || value == null)
				return value;

			string prefix = QuotePrefix;
			string suffix = QuoteSuffix;

			if (prefix.Length == 0 && suffix.Length == 0)
				return value;
			return String.Format ("{0}{1}{2}", prefix, value, suffix);
		}

		private void BuildInformation (DataTable schemaTable)
		{
			_tableName = String.Empty;
			foreach (DataRow schemaRow in schemaTable.Rows) {
				if (schemaRow.IsNull ("BaseTableName") || (string) schemaRow ["BaseTableName"] == String.Empty)
					continue;

				if (_tableName == String.Empty) 
					_tableName = (string) schemaRow ["BaseTableName"];
				else if (_tableName != (string) schemaRow["BaseTableName"])
					throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
			}
			if (_tableName == String.Empty)
				throw new InvalidOperationException ("Dynamic SQL generation is not supported with no base table.");
			_dbSchemaTable = schemaTable;
		}

		private bool IncludedInInsert (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, Expression, RowVersion, ReadOnly

			if (!schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
				return false;
//			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
//				return false;
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
//			if (!schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
//				return false;
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

		private DbCommand CreateDeleteCommand (bool option)
		{
			// If no table was found, then we can't do an delete
			if (QuotedTableName == String.Empty)
				return null;
			
			CreateNewCommand (ref _deleteCommand);

			string command = String.Format ("DELETE FROM {0}", QuotedTableName);
			StringBuilder whereClause = new StringBuilder ();
			bool keyFound = false;
			int parmIndex = 1;

			foreach (DataRow schemaRow in _dbSchemaTable.Rows) {
				if (!schemaRow.IsNull ("IsExpression") && (bool)schemaRow["IsExpression"] == true)
					continue;
				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				DbParameter parameter = null;
				string sourceColumnName;

				if (isKey)
					keyFound = true;

				//ms.net 1.1 generates the null check for columns even if AllowDBNull is false
				//while ms.net 2.0 does not. Anyways, since both forms are logically equivalent
				//following the 2.0 approach
				bool allowNull = (bool) schemaRow ["AllowDBNull"];
				if (!isKey && allowNull) {
					parameter = _deleteCommand.CreateParameter ();
					if (option) {
						parameter.ParameterName = String.Format ("@IsNull_{0}",
											 schemaRow ["BaseColumnName"]);
					} else {
						parameter.ParameterName = String.Format ("@p{0}", parmIndex++);
					}
					parameter.Value = 1;
					parameter.DbType = DbType.Int32;
					// This should be set for nullcheckparam
					sourceColumnName = (string) schemaRow ["BaseColumnName"];
					parameter.SourceColumn = sourceColumnName;
					parameter.SourceColumnNullMapping = true;
					parameter.SourceVersion = DataRowVersion.Original;
					_deleteCommand.Parameters.Add (parameter);
					
					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, parameter.ParameterName, 
									   GetQuotedString (sourceColumnName)));
					whereClause.Append (" OR ");
				}

				if (option)
					parameter = CreateParameter (_deleteCommand, schemaRow, true);
				else 
					parameter = CreateParameter (_deleteCommand, parmIndex++, schemaRow);
				
				parameter.SourceVersion = DataRowVersion.Original;
				ApplyParameterInfo (parameter, schemaRow, StatementType.Delete, true);
				//parameter.IsNullable = allowNull;

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey && allowNull)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0} WHERE ({1})", command, whereClause.ToString ());
			_deleteCommand.CommandText = sql;
			_dbCommand = _deleteCommand;
			return _deleteCommand;
		}

		private DbCommand CreateInsertCommand (bool option, DataRow row)
		{
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref _insertCommand);

			string command = String.Format ("INSERT INTO {0}", QuotedTableName);
			string sql;
			StringBuilder columns = new StringBuilder ();
			StringBuilder values = new StringBuilder ();

			int parmIndex = 1;
			DbParameter parameter = null;
			foreach (DataRow schemaRow in _dbSchemaTable.Rows) {
				if (!IncludedInInsert (schemaRow))
					continue;

				if (columns.Length > 0) {
					columns.Append (", ");
					values.Append (", ");
				}

				if (option)
					parameter = CreateParameter (_insertCommand, schemaRow, false);
				else 
					parameter = CreateParameter (_insertCommand, parmIndex++, schemaRow);			
				parameter.SourceVersion = DataRowVersion.Current;
				ApplyParameterInfo (parameter, schemaRow, StatementType.Insert, false);
				
				columns.Append (GetQuotedString (parameter.SourceColumn));

				// Workaround for columns that may have a default/bound value and for now, 
				// the framework, don't provide a mechanism to read these values yet
				// AllowDBNull and DataRow is used to workaround #385028 by using DEFAULT 
				string colName = schemaRow ["ColumnName"] as string;
				bool allowDBNull = !schemaRow.IsNull ("AllowDBNull") & (bool) schemaRow ["AllowDBNull"];
				if (!allowDBNull && row != null &&
				    (row [colName] == DBNull.Value || row [colName] == null)) {
					values.Append ("DEFAULT");
				} else {
					values.Append (parameter.ParameterName);
				}
			}

			sql = String.Format ("{0} ({1}) VALUES ({2})", command, columns.ToString (), values.ToString ());
			_insertCommand.CommandText = sql;
			_dbCommand = _insertCommand;
			return _insertCommand;
		}

		private void CreateNewCommand (ref DbCommand command)
		{
			DbCommand sourceCommand = SourceCommand;
			if (command == null) {
				command = sourceCommand.Connection.CreateCommand ();
				command.CommandTimeout = sourceCommand.CommandTimeout;
				command.Transaction = sourceCommand.Transaction;
			}
			command.CommandType = CommandType.Text;
			command.UpdatedRowSource = UpdateRowSource.None;
			command.Parameters.Clear ();
		}

		private DbCommand CreateUpdateCommand (bool option)
		{
			// If no table was found, then we can't do an update
			if (QuotedTableName == String.Empty)
				return null;

			CreateNewCommand (ref _updateCommand);

			string command = String.Format ("UPDATE {0} SET ", QuotedTableName);
			StringBuilder columns = new StringBuilder ();
			StringBuilder whereClause = new StringBuilder ();
			int parmIndex = 1;
			bool keyFound = false;
			DbParameter parameter = null;

			// First, create the X=Y list for UPDATE
			foreach (DataRow schemaRow in _dbSchemaTable.Rows) {
				if (!IncludedInUpdate (schemaRow))
					continue;
				if (columns.Length > 0) 
					columns.Append (", ");

				if (option)
					parameter = CreateParameter (_updateCommand, schemaRow, false);
				else 
					parameter = CreateParameter (_updateCommand, parmIndex++, schemaRow);
				parameter.SourceVersion = DataRowVersion.Current;
				ApplyParameterInfo (parameter, schemaRow, StatementType.Update, false);
				//parameter.IsNullable = (bool) schemaRow ["AllowDBNull"];
				columns.Append (String.Format ("{0} = {1}", GetQuotedString (parameter.SourceColumn), parameter.ParameterName));
			}

			// Now, create the WHERE clause.  This may be optimizable, but it would be ugly to incorporate
			// into the loop above.  "Premature optimization is the root of all evil." -- Knuth
			foreach (DataRow schemaRow in _dbSchemaTable.Rows) {
				if (!schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"] == true)
					continue;

				if (!IncludedInWhereClause (schemaRow)) 
					continue;

				if (whereClause.Length > 0) 
					whereClause.Append (" AND ");

				bool isKey = (bool) schemaRow ["IsKey"];
				if (isKey)
					keyFound = true;

				//ms.net 1.1 generates the null check for columns even if AllowDBNull is false
				//while ms.net 2.0 does not. Anyways, since both forms are logically equivalent
				//following the 2.0 approach
				bool allowNull = (bool) schemaRow ["AllowDBNull"];
				if (!isKey && allowNull) {
					parameter = _updateCommand.CreateParameter ();
					if (option) {
						parameter.ParameterName = String.Format ("@IsNull_{0}",
											 schemaRow ["BaseColumnName"]);
					} else {
						parameter.ParameterName = String.Format ("@p{0}", parmIndex++);
					}
					parameter.DbType = DbType.Int32;
					parameter.Value = 1;
					parameter.SourceColumn = (string) schemaRow ["BaseColumnName"];
					parameter.SourceColumnNullMapping = true;
					parameter.SourceVersion = DataRowVersion.Original;
					whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, parameter.ParameterName,
									   GetQuotedString ((string) schemaRow ["BaseColumnName"])));
					whereClause.Append (" OR ");
					_updateCommand.Parameters.Add (parameter);
				}

				if (option)
					parameter = CreateParameter (_updateCommand, schemaRow, true);
				else 
					parameter = CreateParameter (_updateCommand, parmIndex++, schemaRow);
				parameter.SourceVersion = DataRowVersion.Original;
				//parameter.IsNullable = allowNull;
				ApplyParameterInfo (parameter, schemaRow, StatementType.Update, true);

				whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn), parameter.ParameterName));

				if (!isKey && allowNull)
					whereClause.Append (")");
			}
			if (!keyFound)
				throw new InvalidOperationException ("Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.");

			// We're all done, so bring it on home
			string sql = String.Format ("{0}{1} WHERE ({2})", command, columns.ToString (), whereClause.ToString ());
			_updateCommand.CommandText = sql;
			_dbCommand = _updateCommand;
			return _updateCommand;
		}

		private DbParameter CreateParameter (DbCommand _dbCommand, DataRow schemaRow, bool whereClause)
		{
			string sourceColumn = (string) schemaRow ["BaseColumnName"];
			DbParameter parameter = _dbCommand.CreateParameter ();
			if (whereClause)
				parameter.ParameterName = GetParameterName ("Original_" + sourceColumn);
			else
				parameter.ParameterName = GetParameterName (sourceColumn);
			parameter.SourceColumn = sourceColumn;
			//parameter.Size = (int) schemaRow ["ColumnSize"];
			_dbCommand.Parameters.Add (parameter);
			return parameter;
		}

		private DbParameter CreateParameter (DbCommand _dbCommand, int paramIndex, DataRow schemaRow)
		{
			string sourceColumn = (string) schemaRow ["BaseColumnName"];
			DbParameter parameter = _dbCommand.CreateParameter ();
			parameter.ParameterName = GetParameterName (paramIndex);
			parameter.SourceColumn = sourceColumn;
			//parameter.Size = (int) schemaRow ["ColumnSize"];
			_dbCommand.Parameters.Add (parameter);
			return parameter;
		}
		
		[DefaultValue (CatalogLocation.Start)]
		public virtual CatalogLocation CatalogLocation {
			get { return _catalogLocation; }
			set {
				CheckEnumValue (typeof (CatalogLocation),
					(int) value);
				_catalogLocation = value;
			}
		}

		[DefaultValue (".")]
		public virtual string CatalogSeparator {
			get {
				if (_catalogSeparator == null || _catalogSeparator.Length == 0)
					return SEPARATOR_DEFAULT;
				return _catalogSeparator;
			}
			set { _catalogSeparator = value; }
		}

		[DefaultValue (ConflictOption.CompareAllSearchableValues)]
		public virtual ConflictOption ConflictOption {
			get { return _conflictOption; }
			set {
				CheckEnumValue (typeof (ConflictOption),
					(int) value);
				_conflictOption = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public DbDataAdapter DataAdapter {
			get { return _dbDataAdapter; }
			set {  if (value != null) 
				SetRowUpdatingHandler (value);
				_dbDataAdapter = value; 
			}
		}

		[DefaultValue ("")]
		public virtual string QuotePrefix {
			get {
				if (_quotePrefix == null)
					return string.Empty;
				return _quotePrefix;
			}
			set {
				if (IsCommandGenerated)
					throw new InvalidOperationException (
						"QuotePrefix cannot be set after " +
						"an Insert, Update or Delete command " +
						"has been generated.");
				_quotePrefix = value;
			}
		}

		[DefaultValue ("")]
		public virtual string QuoteSuffix {
			get {
				if (_quoteSuffix == null)
					return string.Empty;
				return _quoteSuffix;
			}
			set {
				if (IsCommandGenerated)
					throw new InvalidOperationException (
						"QuoteSuffix cannot be set after " +
						"an Insert, Update or Delete command " +
						"has been generated.");
				_quoteSuffix = value;
			}
		}

		[DefaultValue (".")]
		public virtual string SchemaSeparator {
			get {
				if (_schemaSeparator == null || _schemaSeparator.Length == 0)
					return SEPARATOR_DEFAULT;
				return _schemaSeparator;
			}
			set { _schemaSeparator = value; }
		}

		[DefaultValue (false)]
		public bool SetAllValues {
			get { return _setAllValues; }
			set { _setAllValues = value; }
		}

		private DbCommand SourceCommand {
			get {
				if (_dbDataAdapter != null)
					return _dbDataAdapter.SelectCommand;
				return null;
			}
		}

		protected abstract void ApplyParameterInfo (DbParameter parameter, 
							    DataRow row, 
							    StatementType statementType, 
							    bool whereClause);

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					if (_insertCommand != null)
						_insertCommand.Dispose ();
					if (_deleteCommand != null)
						_deleteCommand.Dispose ();
					if (_updateCommand != null)
						_updateCommand.Dispose ();
					if (_dbSchemaTable != null)
						_dbSchemaTable.Dispose ();
				}
				_disposed = true;
			}
		}

		public DbCommand GetDeleteCommand ()
		{
			return GetDeleteCommand (false);
		}

		public DbCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			BuildCache (true);
			if (_deleteCommand == null || useColumnsForParameterNames)
				return CreateDeleteCommand (useColumnsForParameterNames);
			return _deleteCommand;
		}

		public DbCommand GetInsertCommand ()
		{
			return GetInsertCommand (false, null);
		}

		public DbCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			return GetInsertCommand (useColumnsForParameterNames, null);
		}

		internal DbCommand GetInsertCommand (bool useColumnsForParameterNames, DataRow row)
		{
			BuildCache (true);
			if (_insertCommand == null || useColumnsForParameterNames)
				return CreateInsertCommand (useColumnsForParameterNames, row);
			return _insertCommand;
		}

		public DbCommand GetUpdateCommand ()
		{
			return GetUpdateCommand (false);
		}

		public DbCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			BuildCache (true);
			if (_updateCommand == null || useColumnsForParameterNames)
				return CreateUpdateCommand (useColumnsForParameterNames);
			return _updateCommand;
		}

		protected virtual DbCommand InitializeCommand (DbCommand command)
		{
			if (_dbCommand == null) {
				_dbCommand = SourceCommand;
			} else {
				_dbCommand.CommandTimeout = 30;
				_dbCommand.Transaction = null;
				_dbCommand.CommandType = CommandType.Text;
				_dbCommand.UpdatedRowSource = UpdateRowSource.None;
			}
			return _dbCommand;

		}

		public virtual string QuoteIdentifier (string unquotedIdentifier)
		{
			throw new NotSupportedException ();
		}

		public virtual string UnquoteIdentifier (string quotedIdentifier)
		{
			if (quotedIdentifier == null) {
				throw new ArgumentNullException ("Quoted identifier parameter cannot be null");
			}
			string unquotedIdentifier = quotedIdentifier.Trim ();
			if (unquotedIdentifier.StartsWith (this.QuotePrefix)) {
				unquotedIdentifier = unquotedIdentifier.Remove (0, 1);
			}
			if (unquotedIdentifier.EndsWith (this.QuoteSuffix)) {
				unquotedIdentifier = unquotedIdentifier.Remove (unquotedIdentifier.Length - 1, 1);
			}
			return unquotedIdentifier;
		}

		public virtual void RefreshSchema ()
		{
			_tableName = String.Empty;
			_dbSchemaTable = null;
			_deleteCommand = null;
			_updateCommand = null;
			_insertCommand = null;
		}

		protected void RowUpdatingHandler (RowUpdatingEventArgs rowUpdatingEvent)
		{
			if (rowUpdatingEvent.Command != null)
				return;
			try {
				switch (rowUpdatingEvent.StatementType) {
				case StatementType.Insert:
					rowUpdatingEvent.Command = GetInsertCommand (false, rowUpdatingEvent.Row);
					break;
				case StatementType.Update:
					rowUpdatingEvent.Command = GetUpdateCommand ();
					break;
				case StatementType.Delete:
					rowUpdatingEvent.Command = GetDeleteCommand ();
					break;
				}
			} catch (Exception e) {
				rowUpdatingEvent.Errors = e;
				rowUpdatingEvent.Status = UpdateStatus.ErrorsOccurred;
			}
		}

		protected abstract string GetParameterName (int parameterOrdinal);
		protected abstract string GetParameterName (String parameterName);
		protected abstract string GetParameterPlaceholder (int parameterOrdinal);

		protected abstract void SetRowUpdatingHandler (DbDataAdapter adapter);

		protected virtual DataTable GetSchemaTable (DbCommand sourceCommand)
		{
			using (DbDataReader rdr = sourceCommand.ExecuteReader ())
				return rdr.GetSchemaTable ();
		}

		static void CheckEnumValue (Type type, int value)
		{
			if (Enum.IsDefined (type, value))
				return;

			string typename = type.Name;
			string msg = string.Format (CultureInfo.CurrentCulture,
				"Value {0} is not valid for {1}.", value,
				typename);
			throw new ArgumentOutOfRangeException (typename, msg);
		}
	}
}

#endif
