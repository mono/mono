// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace ByteFX.Data.MySQLClient
{
	/// <summary>
	/// Summary description for CommandBuilder.
	/// </summary>
	[ToolboxItem(false)]
	public sealed class MySQLCommandBuilder : Component
	{
		private MySQLDataAdapter	_adapter;
		private string				_QuotePrefix;
		private string				_QuoteSuffix;
		private DataTable			_schema;
		private string				_tableName;

		private	MySQLCommand		_updateCmd;
		private MySQLCommand		_insertCmd;
		private MySQLCommand		_deleteCmd;

		#region Constructors
		public MySQLCommandBuilder()
		{
		}

		public MySQLCommandBuilder( MySQLDataAdapter adapter )
		{
			_adapter = adapter;
			_adapter.RowUpdating += new MySQLRowUpdatingEventHandler( OnRowUpdating );
		}
		#endregion

		#region Properties
		public MySQLDataAdapter DataAdapter 
		{
			get { return _adapter; }
			set 
			{ 
				if (_adapter != null) 
				{
					_adapter.RowUpdating -= new MySQLRowUpdatingEventHandler( OnRowUpdating );
				}
				_adapter = value; 
			}
		}

		public string QuotePrefix 
		{
			get { return _QuotePrefix; }
			set { _QuotePrefix = value; }
		}

		public string QuoteSuffix
		{
			get { return _QuoteSuffix; }
			set { _QuoteSuffix = value; }
		}

		#endregion

		#region Public Methods
		public static void DeriveParameters(MySQLCommand command)
		{
			throw new MySQLException("DeriveParameters is not supported (due to MySQL not supporting SP)");
		}

		public MySQLCommand GetDeleteCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateDeleteCommand();
		}

		public MySQLCommand GetInsertCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateInsertCommand();
		}

		public MySQLCommand GetUpdateCommand() 
		{
			if (_schema == null)
				GenerateSchema();
			return CreateUpdateCommand();
		}

		public void RefreshSchema()
		{
			_schema = null;
			_insertCmd = null;
			_deleteCmd = null;
			_updateCmd = null;
		}
		#endregion

		#region Private Methods

		private void GenerateSchema()
		{
			if (_adapter == null)
				throw new MySQLException("Improper MySQLCommandBuilder state: adapter is null");
			if (_adapter.SelectCommand == null)
				throw new MySQLException("Improper MySQLCommandBuilder state: adapter's SelectCommand is null");

			MySQLDataReader dr = _adapter.SelectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
			_schema = dr.GetSchemaTable();
			dr.Close();

			// make sure we got at least one unique or key field and count base table names
			bool   hasKeyOrUnique=false;

			foreach (DataRow row in _schema.Rows)
			{
				if (true == (bool)row["IsKey"] || true == (bool)row["IsUnique"])
					hasKeyOrUnique=true;
				if (_tableName == null)
					_tableName = (string)row["BaseTableName"];
				else if (_tableName != (string)row["BaseTableName"])
					throw new InvalidOperationException("MySQLCommandBuilder does not support multi-table statements");
			}
			if (! hasKeyOrUnique)
				throw new InvalidOperationException("MySQLCommandBuilder cannot operate on tables with no unique or key columns");
		}

		private string Quote(string table_or_column)
		{
			if (_QuotePrefix == null || _QuoteSuffix == null)
				return table_or_column;
			return _QuotePrefix + table_or_column + _QuoteSuffix;
		}

		private MySQLParameter CreateParameter(DataRow row, bool Original)
		{
			MySQLParameter p;
			if (Original)
				p = new MySQLParameter( "@Original_" + (string)row["ColumnName"], (MySQLDbType)row["ProviderType"],
					ParameterDirection.Input, (string)row["ColumnName"], DataRowVersion.Original, null );
			else
				p = new MySQLParameter( "@" + (string)row["ColumnName"], (MySQLDbType)row["ProviderType"],
					(string)row["ColumnName"]);
			return p;
		}

		private MySQLCommand CreateBaseCommand()
		{
			MySQLCommand cmd = new MySQLCommand();
			cmd.Connection = _adapter.SelectCommand.Connection;
			cmd.CommandTimeout = _adapter.SelectCommand.CommandTimeout;
			cmd.Transaction = _adapter.SelectCommand.Transaction;
			return cmd;
		}

		private MySQLCommand CreateDeleteCommand()
		{
			if (_deleteCmd != null) return _deleteCmd;

			MySQLCommand cmd = CreateBaseCommand();

			cmd.CommandText = "DELETE FROM " + Quote(_tableName) + 
				" WHERE " + CreateOriginalWhere(cmd);

			_deleteCmd = cmd;
			return cmd;
		}

		private string CreateFinalSelect(bool forinsert)
		{
			StringBuilder sel = new StringBuilder();
			StringBuilder where = new StringBuilder();

			foreach (DataRow row in _schema.Rows)
			{
				string colname = (string)row["ColumnName"];
				if (sel.Length > 0)
					sel.Append(", ");
				sel.Append( colname );
				if ((bool)row["IsKey"] == false) continue;
				if (where.Length > 0)
					where.Append(" AND ");
				where.Append( "(" + colname + "=" );
				if ((bool)row["IsAutoIncrement"] && forinsert)
					where.Append("last_insert_id()");
				else
					where.Append("@Original_" + colname);
				where.Append(")");
			}
			return "SELECT " + sel.ToString() + " FROM " + Quote(_tableName) +
				   " WHERE " + where.ToString();
		}

		private string CreateOriginalWhere(MySQLCommand cmd)
		{
			StringBuilder wherestr = new StringBuilder();

			foreach (DataRow row in _schema.Rows)
			{
				if (! IncludedInWhereClause(row)) continue;

				// first update the where clause since it will contain all parameters
				if (wherestr.Length > 0)
					wherestr.Append(" AND ");
				string colname = Quote((string)row["ColumnName"]);

				MySQLParameter op = CreateParameter(row, true);
				cmd.Parameters.Add(op);

				wherestr.Append( "(" + colname + "=" + op.ParameterName);
				if ((bool)row["AllowDBNull"] == true) 
					wherestr.Append( " or " + colname + " is null and " + op.ParameterName + " is null");
				wherestr.Append(")");
			}
			return wherestr.ToString();
		}

		private MySQLCommand CreateUpdateCommand()
		{
			if (_updateCmd != null) return _updateCmd; 

			MySQLCommand cmd = CreateBaseCommand();

			StringBuilder setstr = new StringBuilder();
		
			foreach (DataRow schemaRow in _schema.Rows)
			{
				string colname = Quote((string)schemaRow["ColumnName"]);

				if (! IncludedInUpdate(schemaRow)) continue;

				if (setstr.Length > 0) 
					setstr.Append(", ");

				MySQLParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname + "=" + p.ParameterName );
			}

			cmd.CommandText = "UPDATE " + Quote(_tableName) + " SET " + setstr.ToString() + 
							  " WHERE " + CreateOriginalWhere(cmd);
			cmd.CommandText += "; " + CreateFinalSelect(false);

			_updateCmd = cmd;
			return cmd;
		}

		private MySQLCommand CreateInsertCommand()
		{
			if (_insertCmd != null) return _insertCmd;

			MySQLCommand cmd = CreateBaseCommand();

			StringBuilder setstr = new StringBuilder();
			StringBuilder valstr = new StringBuilder();
			foreach (DataRow schemaRow in _schema.Rows)
			{
				string colname = Quote((string)schemaRow["ColumnName"]);

				if (!IncludedInInsert(schemaRow)) continue;

				if (setstr.Length > 0) 
				{
					setstr.Append(", ");
					valstr.Append(", ");
				}

				MySQLParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname );
				valstr.Append( p.ParameterName );
			}

			cmd.CommandText = "INSERT INTO " + Quote(_tableName) + " (" + setstr.ToString() + ") " +
				" VALUES (" + valstr.ToString() + ")";
			cmd.CommandText += "; " + CreateFinalSelect(true);

			_insertCmd = cmd;
			return cmd;
		}

		private bool IncludedInInsert (DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:
			// AutoIncrement, Hidden, Expression, RowVersion, ReadOnly

			if ((bool) schemaRow ["IsAutoIncrement"])
				return false;
/*			if ((bool) schemaRow ["IsHidden"])
				return false;
			if ((bool) schemaRow ["IsExpression"])
				return false;*/
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
//			if ((bool) schemaRow ["IsHidden"])
//				return false;
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

		private void SetParameterValues(MySQLCommand cmd, DataRow dataRow)
		{
			foreach (MySQLParameter p in cmd.Parameters)
			{
				if (p.ParameterName.Length >= 9 && p.ParameterName.Substring(0, 9).Equals("@Original"))
					p.Value = dataRow[ p.SourceColumn, DataRowVersion.Original ];
				else
					p.Value = dataRow[ p.SourceColumn, DataRowVersion.Current ];
			}
		}

		private void OnRowUpdating(object sender, MySQLRowUpdatingEventArgs args)
		{
			// make sure we are still to proceed
			if (args.Status != UpdateStatus.Continue) return;

			if (_schema == null)
				GenerateSchema();

			if (StatementType.Delete == args.StatementType)
				args.Command = CreateDeleteCommand();
			else if (StatementType.Update == args.StatementType)
				args.Command = CreateUpdateCommand();
			else if (StatementType.Insert == args.StatementType)
				args.Command = CreateInsertCommand();
			else if (StatementType.Select == args.StatementType)
				return;

			SetParameterValues(args.Command, args.Row);
		}
		#endregion

	}
}
