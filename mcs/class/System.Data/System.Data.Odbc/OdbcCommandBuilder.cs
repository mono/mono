//
// System.Data.Odbc.OdbcCommandBuilder
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//   Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (C) Novell Inc, 2004
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

using System.Text;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.Odbc
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>

#if NET_2_0
	public sealed class OdbcCommandBuilder : DbCommandBuilder
#else // 1_1
	public sealed class OdbcCommandBuilder : Component
#endif // NET_2_0
	{
		#region Fields

		private OdbcDataAdapter _adapter;
#if ONLY_1_1
		private string 			_quotePrefix;
		private string 			_quoteSuffix;
#endif

		private DataTable		_schema;
		private string			_tableName;
		private OdbcCommand		_insertCommand;
		private OdbcCommand		_updateCommand;
		private OdbcCommand		_deleteCommand;

		bool _disposed;

		private OdbcRowUpdatingEventHandler rowUpdatingHandler;
		
		#endregion // Fields

		#region Constructors
		
		public OdbcCommandBuilder ()
		{
		}

		public OdbcCommandBuilder (OdbcDataAdapter adapter)
			: this ()
		{
			DataAdapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[OdbcDescriptionAttribute ("The DataAdapter for which to automatically generate OdbcCommands")]
		[DefaultValue (null)]
		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcDataAdapter DataAdapter {
			get {
				return _adapter;
			}
			set {
				if (_adapter == value)
					return;

				if (rowUpdatingHandler != null)
					rowUpdatingHandler = new OdbcRowUpdatingEventHandler (OnRowUpdating);
				
				if (_adapter != null)
					_adapter.RowUpdating -= rowUpdatingHandler;
				_adapter = value;
				if (_adapter != null)
					_adapter.RowUpdating += rowUpdatingHandler;
			}
		}

		private OdbcCommand SelectCommand {
			get {
				if (DataAdapter == null)
					return null;
				return DataAdapter.SelectCommand;
			}
		}

		private DataTable Schema {
			get {
				if (_schema == null)
					RefreshSchema ();
				return _schema;
			}
		}
		
		private string TableName {
			get {
				if (_tableName != string.Empty)
					return _tableName;

				DataRow [] schemaRows = Schema.Select ("BaseTableName is not null and BaseTableName <> ''");
				if (schemaRows.Length > 1) {
					string tableName = (string) schemaRows [0] ["BaseTableName"];
					foreach (DataRow schemaRow in schemaRows) {
						if ( (string) schemaRow ["BaseTableName"] != tableName)
							throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
					}
				}
				if (schemaRows.Length == 0)
					throw new InvalidOperationException ("Cannot determine the base table name. Cannot proceed");
				_tableName = schemaRows [0] ["BaseTableName"].ToString ();
				return _tableName;
			}
		}

#if ONLY_1_1
		[BrowsableAttribute (false)]
		[OdbcDescriptionAttribute ("The prefix string wrapped around sql objects")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public string QuotePrefix {
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

		[BrowsableAttribute (false)]
		[OdbcDescriptionAttribute ("The suffix string wrapped around sql objects")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public string QuoteSuffix {
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
#endif

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (OdbcCommand command)
		{
			throw new NotImplementedException ();
		}

#if ONLY_1_1
		protected override
#else
		new
#endif
		void Dispose (bool disposing)
		{
			if (_disposed)
				return;
			
			if (disposing) {
				// dispose managed resource
				if (_insertCommand != null)
					_insertCommand.Dispose ();
				if (_updateCommand != null)
					_updateCommand.Dispose ();
				if (_deleteCommand != null)
					_deleteCommand.Dispose ();
				if (_schema != null)
					_schema.Dispose ();

				_insertCommand = null;
				_updateCommand = null;
				_deleteCommand = null;
				_schema = null;
			}
			_disposed = true;
		}

		private bool IsUpdatable (DataRow schemaRow)
		{
			if ( (! schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
			     || (! schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
			     || (! schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
			     || (schemaRow.IsNull ("BaseTableName") || ((string) schemaRow ["BaseTableName"]).Length == 0)
			     )
				return false;
			return true;
		}
		
		private string GetColumnName (DataRow schemaRow)
		{
			string columnName = schemaRow.IsNull ("BaseColumnName") ? String.Empty : (string) schemaRow ["BaseColumnName"];
			if (columnName == String.Empty)
				columnName = schemaRow.IsNull ("ColumnName") ? String.Empty : (string) schemaRow ["ColumnName"];
			return columnName;
		}
		
		private OdbcParameter AddParameter (OdbcCommand cmd, string paramName, OdbcType odbcType,
						    int length, string sourceColumnName, DataRowVersion rowVersion)
		{
			OdbcParameter param;
			if (length >= 0 && sourceColumnName != String.Empty)
				param = cmd.Parameters.Add (paramName, odbcType, length, sourceColumnName);
			else
				param = cmd.Parameters.Add (paramName, odbcType);
			param.SourceVersion = rowVersion;
			return param;
		}

		/*
		 * creates where clause for optimistic concurrency
		 */
		private string CreateOptWhereClause (OdbcCommand command, int paramCount)
		{
			string [] whereClause = new string [Schema.Rows.Count];

			int partCount = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form delete command. Column name is missing!");

				bool 	allowNull  = schemaRow.IsNull ("AllowDBNull") || (bool) schemaRow ["AllowDBNull"];
				OdbcType sqlDbType = schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int 	length 	   = schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				if (allowNull) {
					whereClause [partCount++] = String.Format ("((? = 1 AND {0} IS NULL) OR ({0} = ?))",
						GetQuotedString (columnName));
					OdbcParameter nullParam = AddParameter (
						command,
						GetParameterName (++paramCount),
						OdbcType.Int,
						length,
#if NET_2_0
						columnName,
#else
						string.Empty,
#endif
						DataRowVersion.Original);
					nullParam.Value = 1;
					AddParameter (command, GetParameterName (++paramCount),
						sqlDbType, length, columnName,
						DataRowVersion.Original);
				} else {
					whereClause [partCount++] = String.Format ("({0} = ?)",
						GetQuotedString (columnName));
					AddParameter (command, GetParameterName (++paramCount),
						sqlDbType, length, columnName,
						DataRowVersion.Original);
				}
			}

			return String.Join (" AND ", whereClause, 0, partCount);
		}

		private void CreateNewCommand (ref OdbcCommand command)
		{
			OdbcCommand sourceCommand = SelectCommand;
			if (command == null) {
				command = new OdbcCommand ();
				command.Connection = sourceCommand.Connection;
				command.CommandTimeout = sourceCommand.CommandTimeout;
				command.Transaction = sourceCommand.Transaction;
			}
			command.CommandType = CommandType.Text;
			command.UpdatedRowSource = UpdateRowSource.None;
			command.Parameters.Clear ();
		}
		
		private OdbcCommand CreateInsertCommand (bool option)
		{
			CreateNewCommand (ref _insertCommand);

			string query = String.Format ("INSERT INTO {0}", GetQuotedString (TableName));
			string [] columns = new string [Schema.Rows.Count];
			string [] values  = new string [Schema.Rows.Count];

			int count = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form insert command. Column name is missing!");

				// create column string & value string
				columns [count] = GetQuotedString (columnName);
				values [count++] = "?";

				// create parameter and add
				OdbcType sqlDbType = schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int length = schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				AddParameter (_insertCommand, GetParameterName (count),
					sqlDbType, length, columnName, DataRowVersion.Current);
			}

			query = String.Format (
#if NET_2_0
				"{0} ({1}) VALUES ({2})", 
#else
				"{0}( {1} ) VALUES ( {2} )", 
#endif
				query, 
#if NET_2_0
				String.Join (", ", columns, 0, count),
				String.Join (", ", values, 0, count));
#else
				String.Join (" , ", columns, 0, count),
				String.Join (" , ", values, 0, count));
#endif
			_insertCommand.CommandText = query;
			return _insertCommand;
		}

		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcCommand GetInsertCommand ()
		{
			// FIXME: check validity of adapter
			if (_insertCommand != null)
				return _insertCommand;

			if (_schema == null)
				RefreshSchema ();

			return CreateInsertCommand (false);
		}

#if NET_2_0
		public new OdbcCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			// FIXME: check validity of adapter
			if (_insertCommand != null)
				return _insertCommand;

			if (_schema == null)
				RefreshSchema ();

			return CreateInsertCommand (useColumnsForParameterNames);
		}
#endif // NET_2_0

		private OdbcCommand CreateUpdateCommand (bool option)
		{
			CreateNewCommand (ref _updateCommand);

			string query = String.Format ("UPDATE {0} SET", GetQuotedString (TableName));
			string [] setClause = new string [Schema.Rows.Count];

			int count = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form update command. Column name is missing!");

				OdbcType sqlDbType = schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int length = schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				// create column = value string
				setClause [count++] = String.Format ("{0} = ?", GetQuotedString (columnName));
				AddParameter (_updateCommand, GetParameterName (count),
					sqlDbType, length, columnName, DataRowVersion.Current);
			}

			// create where clause. odbc uses positional parameters. so where class
			// is created seperate from the above loop.
			string whereClause = CreateOptWhereClause (_updateCommand, count);
			
			query = String.Format (
#if NET_2_0
				"{0} {1} WHERE ({2})",
#else
				"{0} {1} WHERE ( {2} )",
#endif
				query,
#if NET_2_0
				String.Join (", ", setClause, 0, count),
#else
				String.Join (" , ", setClause, 0, count),
#endif
				whereClause);
			_updateCommand.CommandText = query;
			return _updateCommand;
		}
		
		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcCommand GetUpdateCommand ()
		{
			// FIXME: check validity of adapter
			if (_updateCommand != null)
				return _updateCommand;

			if (_schema == null)
				RefreshSchema ();

			return CreateUpdateCommand (false);
		}

#if NET_2_0
		public new OdbcCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			// FIXME: check validity of adapter
			if (_updateCommand != null)
				return _updateCommand;

			if (_schema == null)
				RefreshSchema ();

			return CreateUpdateCommand (useColumnsForParameterNames);
		}
#endif // NET_2_0

		private OdbcCommand CreateDeleteCommand (bool option)
		{
			CreateNewCommand (ref _deleteCommand);

			string query = String.Format (
#if NET_2_0
				"DELETE FROM {0}",
#else
				"DELETE FROM  {0}",
#endif
				GetQuotedString (TableName));
			string whereClause = CreateOptWhereClause (_deleteCommand, 0);
			
			query = String.Format (
#if NET_2_0
				"{0} WHERE ({1})",
#else
				"{0} WHERE ( {1} )",
#endif
				query,
				whereClause);
			_deleteCommand.CommandText = query;
			return _deleteCommand;
		}

		public
#if NET_2_0
		new
#endif // NET_2_0
		OdbcCommand GetDeleteCommand ()
		{
			// FIXME: check validity of adapter
			if (_deleteCommand != null)
				return _deleteCommand;

			if (_schema == null)
				RefreshSchema ();
			
			return CreateDeleteCommand (false);
		}

#if NET_2_0
		public new OdbcCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			// FIXME: check validity of adapter
			if (_deleteCommand != null)
				return _deleteCommand;

			if (_schema == null)
				RefreshSchema ();

			return CreateDeleteCommand (useColumnsForParameterNames);
		}
#endif // NET_2_0

#if ONLY_1_1
		public
#else
		new
#endif // NET_2_0
		void RefreshSchema ()
		{
			// creates metadata
			if (SelectCommand == null)
				throw new InvalidOperationException ("SelectCommand should be valid");
			if (SelectCommand.Connection == null)
				throw new InvalidOperationException ("SelectCommand's Connection should be valid");
			
			CommandBehavior behavior = CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo;
			if (SelectCommand.Connection.State != ConnectionState.Open) {
				SelectCommand.Connection.Open ();
				behavior |= CommandBehavior.CloseConnection;
			}
			
			OdbcDataReader reader = SelectCommand.ExecuteReader (behavior);
			_schema = reader.GetSchemaTable ();
			reader.Close ();
			
			// force creation of commands
			_insertCommand 	= null;
			_updateCommand 	= null;
			_deleteCommand 	= null;
			_tableName	= String.Empty;
		}

#if NET_2_0
		protected override
#endif
		string GetParameterName (int parameterOrdinal)
		{
#if NET_2_0
			return String.Format ("p{0}", parameterOrdinal);
#else
			return String.Format ("@p{0}", parameterOrdinal);
#endif
		}


#if NET_2_0
		protected override void ApplyParameterInfo (DbParameter parameter,
		                                            DataRow datarow,
		                                            StatementType statementType,
		                                            bool whereClause)
		{
			OdbcParameter odbcParam = (OdbcParameter) parameter;
			odbcParam.Size = int.Parse (datarow ["ColumnSize"].ToString ());
			if (datarow ["NumericPrecision"] != DBNull.Value)
				odbcParam.Precision = byte.Parse (datarow ["NumericPrecision"].ToString ());
			if (datarow ["NumericScale"] != DBNull.Value)
				odbcParam.Scale = byte.Parse (datarow ["NumericScale"].ToString ());
			odbcParam.DbType = (DbType) datarow ["ProviderType"];
		}

		protected override string GetParameterName (string parameterName)
		{
			return String.Format("@{0}", parameterName);
		}

		protected override string GetParameterPlaceholder (int parameterOrdinal)
		{
			return GetParameterName (parameterOrdinal);
		}

		// FIXME: According to MSDN - "if this method is called again with
		// the same DbDataAdapter, the DbCommandBuilder is unregistered for 
		// that DbDataAdapter's RowUpdating event" - this behaviour is yet
		// to be verified
		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
		{
			if (!(adapter is OdbcDataAdapter))
				throw new InvalidOperationException ("Adapter needs to be a SqlDataAdapter");
			if (rowUpdatingHandler == null)
				rowUpdatingHandler = new OdbcRowUpdatingEventHandler (OnRowUpdating);

			((OdbcDataAdapter) adapter).RowUpdating += rowUpdatingHandler;
		}

		public override string QuoteIdentifier (string unquotedIdentifier)
		{
			return QuoteIdentifier (unquotedIdentifier, null);
		}

		public string QuoteIdentifier (string unquotedIdentifier, OdbcConnection connection)
		{
			if (unquotedIdentifier == null)
				throw new ArgumentNullException ("unquotedIdentifier");

			string prefix = QuotePrefix;
			string suffix = QuoteSuffix;

			if (QuotePrefix.Length == 0) {
				if (connection == null)
					throw new InvalidOperationException (
						"An open connection is required if "
						+ "QuotePrefix is not set.");
				prefix = suffix = GetQuoteCharacter (connection);
			}

			if (prefix.Length > 0 && prefix != " ") {
				string escaped;
				if (suffix.Length > 0)
					escaped = unquotedIdentifier.Replace (
						suffix, suffix + suffix);
				else
					escaped = unquotedIdentifier;
				return string.Concat (prefix, escaped, suffix);
			}
			return unquotedIdentifier;
		}

		public string UnquoteIdentifier (string quotedIdentifier, OdbcConnection connection)
		{
			return UnquoteIdentifier (quotedIdentifier);
		}

		public override string UnquoteIdentifier (string quotedIdentifier)
		{
			if (quotedIdentifier == null || quotedIdentifier.Length == 0)
				return quotedIdentifier;
			
			StringBuilder sb = new StringBuilder (quotedIdentifier.Length);
			sb.Append (quotedIdentifier);
			if (quotedIdentifier.StartsWith (QuotePrefix))
				sb.Remove (0,QuotePrefix.Length);
			if (quotedIdentifier.EndsWith (QuoteSuffix))
				sb.Remove (sb.Length - QuoteSuffix.Length, QuoteSuffix.Length );
			return sb.ToString ();
		}
#endif

		private void OnRowUpdating (object sender, OdbcRowUpdatingEventArgs args)
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

		string GetQuotedString (string unquotedIdentifier)
		{
			string prefix = QuotePrefix;
			string suffix = QuoteSuffix;

			if (prefix.Length == 0 && suffix.Length == 0)
				return unquotedIdentifier;

			return String.Format ("{0}{1}{2}", prefix,
				unquotedIdentifier, suffix);
		}

		bool IsCommandGenerated {
			get {
				return (_insertCommand != null || _updateCommand != null || _deleteCommand != null);
			}
		}

#if NET_2_0
		string GetQuoteCharacter (OdbcConnection conn)
		{
			return conn.GetInfo (OdbcInfo.IdentifierQuoteChar);
		}
#endif

		#endregion // Methods
	}
}
