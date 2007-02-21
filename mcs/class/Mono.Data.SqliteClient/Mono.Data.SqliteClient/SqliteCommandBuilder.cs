//
// Mono.Data.SqliteClient.SqliteCommandBuilder.cs
//
// Author(s): Tim Coleman (tim@timcoleman.com)
//            Marek Habersack (grendello@gmail.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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
#if NET_2_0
using System;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Mono.Data.SqliteClient
{
	public sealed class SqliteCommandBuilder : DbCommandBuilder
	{
		static readonly string clause1 = "({0} = 1 AND {1} IS NULL)";
                static readonly string clause2 = "({0} = {1})";

		DataTable _schemaTable;
		SqliteDataAdapter _dataAdapter;
		SqliteCommand _insertCommand;
		SqliteCommand _updateCommand;
		SqliteCommand _deleteCommand;
		bool _disposed;
		string _quotePrefix = "'";
		string _quoteSuffix = "'";
		string _tableName;
		SqliteRowUpdatingEventHandler rowUpdatingHandler;
		
		public new DbDataAdapter DataAdapter {
			get { return _dataAdapter; }
			set {
				if (_dataAdapter != null)
					_dataAdapter.RowUpdating -= new SqliteRowUpdatingEventHandler (RowUpdatingHandler);
				_dataAdapter = value as SqliteDataAdapter;
				if (_dataAdapter != null)
					_dataAdapter.RowUpdating += new SqliteRowUpdatingEventHandler (RowUpdatingHandler);
			}
		}

		public override string QuotePrefix {
			get { return _quotePrefix; }
			
			set {
				if (_schemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update or Delete commands have been generated.");
				_quotePrefix = value;
			}
		}

		public override string QuoteSuffix {
			get { return _quoteSuffix; }
			
			set {
				if (_schemaTable != null)
					throw new InvalidOperationException ("The QuotePrefix and QuoteSuffix properties cannot be changed once an Insert, Update or Delete commands have been generated.");
				_quoteSuffix = value;
			}
		}

		private SqliteCommand SourceCommand {
			get  {
				if (_dataAdapter != null)
					return _dataAdapter.SelectCommand as SqliteCommand;
				return null;
			}
		}

		private string QuotedTableName {
                        get { return GetQuotedString (_tableName); }
                }

		public new SqliteCommand GetDeleteCommand ()
                {
                        BuildCache (true);
                        if (_deleteCommand == null)
                                return CreateDeleteCommand (false);
                        return _deleteCommand;
                }

		public new SqliteCommand GetInsertCommand ()
		{
			BuildCache (true);
                        if (_insertCommand == null)
                                return CreateInsertCommand (false);
                        return _insertCommand;
		}

		public new SqliteCommand GetUpdateCommand ()
                {
                        BuildCache (true);
                        if (_updateCommand == null)
                                return CreateUpdateCommand (false);
                        return _updateCommand;
                }

		public override void RefreshSchema () 
                {
                        // FIXME: "Figure out what else needs to be cleaned up when we refresh."
                        _tableName = String.Empty;
                        _schemaTable = null;
                        CreateNewCommand (ref _deleteCommand);
                        CreateNewCommand (ref _updateCommand);
                        CreateNewCommand (ref _insertCommand);
                }

		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
                {
                        if (!(adapter is SqliteDataAdapter)) {
                                throw new InvalidOperationException ("Adapter needs to be a SqliteDataAdapter");
                        }
                        rowUpdatingHandler = new SqliteRowUpdatingEventHandler (RowUpdatingHandler);
                        ((SqliteDataAdapter) adapter).RowUpdating += rowUpdatingHandler;
                }
		
                protected override void ApplyParameterInfo (DbParameter dbParameter,
                                                            DataRow row,
                                                            StatementType statementType,
                                                            bool whereClause)
                {
                        // Nothing to do here
                }

		protected override string GetParameterName (int position)
                {
                        return String.Format ("?p{0}", position);
                }

                protected override string GetParameterName (string parameterName)
                {
                        if (String.IsNullOrEmpty (parameterName))
				throw new ArgumentException ("parameterName cannot be null or empty");
			if (parameterName [0] == '?')
				return parameterName;
			return String.Format ("?{0}", parameterName);
                }
                

                protected override string GetParameterPlaceholder (int position)
                {
			return String.Format ("?p{0}", position);
                }
		
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
                                        if (_schemaTable != null)
                                                _schemaTable.Dispose ();
                                }
                                _disposed = true;
                        }
                }

		private void BuildCache (bool closeConnection)
		{
			SqliteCommand sourceCommand = SourceCommand;
                        if (sourceCommand == null)
                                throw new InvalidOperationException ("The DataAdapter.SelectCommand property needs to be initialized.");
                        SqliteConnection connection = sourceCommand.Connection as SqliteConnection;
                        if (connection == null)
                                throw new InvalidOperationException ("The DataAdapter.SelectCommand.Connection property needs to be initialized.");
                                
                        if (_schemaTable == null) {
                                if (connection.State == ConnectionState.Open)
                                        closeConnection = false;        
                                else
                                        connection.Open ();
        
                                SqliteDataReader reader = sourceCommand.ExecuteReader (CommandBehavior.SchemaOnly |
										       CommandBehavior.KeyInfo);
                                _schemaTable = reader.GetSchemaTable ();
                                reader.Close ();
                                if (closeConnection)
                                        connection.Close ();    
                                BuildInformation (_schemaTable);
                        }
		}

		private void BuildInformation (DataTable schemaTable)
                {
                        _tableName = String.Empty;
                        foreach (DataRow schemaRow in schemaTable.Rows) {
                                if (schemaRow.IsNull ("BaseTableName") ||
                                    (string) schemaRow ["BaseTableName"] == String.Empty)
                                        continue;

                                if (_tableName == String.Empty) 
                                        _tableName = (string) schemaRow ["BaseTableName"];
                                else if (_tableName != (string) schemaRow["BaseTableName"])
                                        throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
                        }
                        if (_tableName == String.Empty)
                                throw new InvalidOperationException ("Dynamic SQL generation is not supported with no base table.");
                        _schemaTable = schemaTable;
                }

		private SqliteCommand CreateInsertCommand (bool option)
                {
                        if (QuotedTableName == String.Empty)
                                return null;

                        CreateNewCommand (ref _insertCommand);

                        string command = String.Format ("INSERT INTO {0}", QuotedTableName);
                        string sql;
                        StringBuilder columns = new StringBuilder ();
                        StringBuilder values = new StringBuilder ();

                        int parmIndex = 1;
                        foreach (DataRow schemaRow in _schemaTable.Rows) {
                                if (!IncludedInInsert (schemaRow))
                                        continue;

                                if (parmIndex > 1) {
                                        columns.Append (", ");
                                        values.Append (", ");
                                }

                                SqliteParameter parameter = null;
                                if (option) {
                                        parameter = _insertCommand.Parameters.Add (CreateParameter (schemaRow));
                                } else {
                                        parameter = _insertCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
                                }
                                parameter.SourceVersion = DataRowVersion.Current;
                                columns.Append (GetQuotedString (parameter.SourceColumn));
                                values.Append (parameter.ParameterName);
                        }

                        sql = String.Format ("{0} ({1}) VALUES ({2})", command, columns.ToString (), values.ToString ());
                        _insertCommand.CommandText = sql;
                        return _insertCommand;
                }
		
		private SqliteCommand CreateDeleteCommand (bool option)
                {
                        // If no table was found, then we can't do an delete
                        if (QuotedTableName == String.Empty)
                                return null;

                        CreateNewCommand (ref _deleteCommand);

                        string command = String.Format ("DELETE FROM {0}", QuotedTableName);
                        StringBuilder whereClause = new StringBuilder ();
                        bool keyFound = false;
                        int parmIndex = 1;

                        foreach (DataRow schemaRow in _schemaTable.Rows) {
                                if ((bool)schemaRow["IsExpression"] == true)
                                        continue;
                                if (!IncludedInWhereClause (schemaRow)) 
                                        continue;

                                if (whereClause.Length > 0) 
                                        whereClause.Append (" AND ");

                                bool isKey = (bool) schemaRow ["IsKey"];
                                SqliteParameter parameter = null;

                                if (isKey)
                                        keyFound = true;

                                bool allowNull = (bool) schemaRow ["AllowDBNull"];
                                if (!isKey && allowNull) {
                                        if (option) {
                                                parameter = _deleteCommand.Parameters.Add (
							String.Format ("@{0}", schemaRow ["BaseColumnName"]), DbType.Int32);
                                        } else {
                                                parameter = _deleteCommand.Parameters.Add (
							String.Format ("@p{0}", parmIndex++), DbType.Int32);
                                        }
                                        String sourceColumnName = (string) schemaRow ["BaseColumnName"];
                                        parameter.Value = 1;

                                        whereClause.Append ("(");
					whereClause.Append (String.Format (clause1, parameter.ParameterName, 
									   GetQuotedString (sourceColumnName)));
                                        whereClause.Append (" OR ");
                                }

                                if (option) {
                                        parameter = _deleteCommand.Parameters.Add (CreateParameter (schemaRow));
                                } else {
                                        parameter = _deleteCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
                                }
                                parameter.SourceVersion = DataRowVersion.Original;

                                whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn),
								   parameter.ParameterName));

                                if (!isKey && allowNull)
                                        whereClause.Append (")");
                        }
                        if (!keyFound)
                                throw new InvalidOperationException ("Dynamic SQL generation for the DeleteCommand is not supported against a SelectCommand that does not return any key column information.");

                        string sql = String.Format ("{0} WHERE ({1})", command, whereClause.ToString ());
                        _deleteCommand.CommandText = sql;
                        return _deleteCommand;
                }

		private SqliteCommand CreateUpdateCommand (bool option)
                {
                        if (QuotedTableName == String.Empty)
                                return null;

                        CreateNewCommand (ref _updateCommand);

                        string command = String.Format ("UPDATE {0} SET ", QuotedTableName);
                        StringBuilder columns = new StringBuilder ();
                        StringBuilder whereClause = new StringBuilder ();
                        int parmIndex = 1;
                        bool keyFound = false;

                        foreach (DataRow schemaRow in _schemaTable.Rows) {
                                if (!IncludedInUpdate (schemaRow))
                                        continue;
                                if (columns.Length > 0) 
                                        columns.Append (", ");

                                SqliteParameter parameter = null;
                                if (option) {
                                        parameter = _updateCommand.Parameters.Add (CreateParameter (schemaRow));
                                } else {
                                        parameter = _updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
                                }
                                parameter.SourceVersion = DataRowVersion.Current;

                                columns.Append (String.Format ("{0} = {1}", GetQuotedString (parameter.SourceColumn),
							       parameter.ParameterName));
                        }

                        foreach (DataRow schemaRow in _schemaTable.Rows) {
                                if ((bool)schemaRow["IsExpression"] == true)
                                        continue;

                                if (!IncludedInWhereClause (schemaRow)) 
                                        continue;

                                if (whereClause.Length > 0) 
                                        whereClause.Append (" AND ");

                                bool isKey = (bool) schemaRow ["IsKey"];
				SqliteParameter parameter = null;

                                if (isKey)
                                        keyFound = true;

                                bool allowNull = (bool) schemaRow ["AllowDBNull"];
                                if (!isKey && allowNull) {
                                        if (option) {
                                                parameter = _updateCommand.Parameters.Add (
							String.Format ("@{0}", schemaRow ["BaseColumnName"]), SqlDbType.Int);
                                        } else {
                                                parameter = _updateCommand.Parameters.Add (
							String.Format ("@p{0}", parmIndex++), SqlDbType.Int);
                                        }
                                        parameter.Value = 1;
                                        whereClause.Append ("(");
                                        whereClause.Append (String.Format (clause1, parameter.ParameterName,
									   GetQuotedString ((string) schemaRow ["BaseColumnName"])));
                                        whereClause.Append (" OR ");
                                }

                                if (option) {
                                        parameter = _updateCommand.Parameters.Add (CreateParameter (schemaRow));
                                } else {
                                        parameter = _updateCommand.Parameters.Add (CreateParameter (parmIndex++, schemaRow));
                                }
                                parameter.SourceVersion = DataRowVersion.Original;
                                whereClause.Append (String.Format (clause2, GetQuotedString (parameter.SourceColumn),
								   parameter.ParameterName));

                                if (!isKey && allowNull)
                                        whereClause.Append (")");
                        }
                        if (!keyFound)
                                throw new InvalidOperationException ("Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.");

                        string sql = String.Format ("{0}{1} WHERE ({2})", command, columns.ToString (), whereClause.ToString ());
                        _updateCommand.CommandText = sql;
                        return _updateCommand;
		}
		
		private void CreateNewCommand (ref SqliteCommand command)
                {
                        SqliteCommand sourceCommand = SourceCommand;
                        if (command == null) {
                                command = sourceCommand.Connection.CreateCommand () as SqliteCommand;
                                command.CommandTimeout = sourceCommand.CommandTimeout;
                                command.Transaction = sourceCommand.Transaction;
                        }
                        command.CommandType = CommandType.Text;
                        command.UpdatedRowSource = UpdateRowSource.None;
                        command.Parameters.Clear ();
                }
		
		private bool IncludedInWhereClause (DataRow schemaRow)
                {
                        if ((bool) schemaRow ["IsLong"])
                                return false;
                        return true;
                }

		private bool IncludedInInsert (DataRow schemaRow)
                {
			// not all of the below are supported by Sqlite, but we leave them here anyway, since some day Sqlite may
			// support some of them.
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
			// not all of the below are supported by Sqlite, but we leave them here anyway, since some day Sqlite may
			// support some of them.
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
		
		private SqliteParameter CreateParameter (DataRow schemaRow)
                {
                        string sourceColumn = (string) schemaRow ["BaseColumnName"];
                        string name = String.Format ("@{0}", sourceColumn);
                        DbType dbType = (DbType) schemaRow ["ProviderType"];
                        int size = (int) schemaRow ["ColumnSize"];

                        return new SqliteParameter (name, dbType, size, sourceColumn);
                }

                private SqliteParameter CreateParameter (int parmIndex, DataRow schemaRow)
                {
                        string name = String.Format ("@p{0}", parmIndex);
                        string sourceColumn = (string) schemaRow ["BaseColumnName"];
                        DbType dbType = (DbType) schemaRow ["ProviderType"];
                        int size = (int) schemaRow ["ColumnSize"];

                        return new SqliteParameter (name, dbType, size, sourceColumn);
                }

		private string GetQuotedString (string value)
                {
                        if (value == String.Empty || value == null)
                                return value;
                        if (String.IsNullOrEmpty (_quotePrefix) && String.IsNullOrEmpty (_quoteSuffix))
                                return value;
                        return String.Format ("{0}{1}{2}", _quotePrefix, value, _quoteSuffix);
                }

		private void RowUpdatingHandler (object sender, RowUpdatingEventArgs args)
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
	}
}
#endif
