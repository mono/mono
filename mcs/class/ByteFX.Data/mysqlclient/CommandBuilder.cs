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

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Automatically generates single-table commands used to reconcile changes made to a DataSet with the associated MySQL database. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlCommandBuilder.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	[ToolboxItem(false)]
	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlCommandBuilder : Component
	{
		private MySqlDataAdapter	_adapter;
		private string				_QuotePrefix;
		private string				_QuoteSuffix;
		private DataTable			_schema;
		private string				_tableName;

		private	MySqlCommand		_updateCmd;
		private MySqlCommand		_insertCmd;
		private MySqlCommand		_deleteCmd;

		#region Constructors
		/// <summary>
		/// Overloaded. Initializes a new instance of the SqlCommandBuilder class.
		/// </summary>
		public MySqlCommandBuilder()
		{
		}

		/// <summary>
		/// Overloaded. Initializes a new instance of the SqlCommandBuilder class.
		/// </summary>
		public MySqlCommandBuilder( MySqlDataAdapter adapter )
		{
			_adapter = adapter;
			_adapter.RowUpdating += new MySqlRowUpdatingEventHandler( OnRowUpdating );
		}
		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets a MySqlDataAdapter object for which SQL statements are automatically generated.
		/// </summary>
		public MySqlDataAdapter DataAdapter 
		{
			get { return _adapter; }
			set 
			{ 
				if (_adapter != null) 
				{
					_adapter.RowUpdating -= new MySqlRowUpdatingEventHandler( OnRowUpdating );
				}
				_adapter = value; 
			}
		}

		/// <summary>
		/// Gets or sets the beginning character or characters to use when specifying MySql database objects (for example, tables or columns) whose names contain characters such as spaces or reserved tokens.
		/// </summary>
		public string QuotePrefix 
		{
			get { return _QuotePrefix; }
			set { _QuotePrefix = value; }
		}

		/// <summary>
		/// Gets or sets the ending character or characters to use when specifying MySql database objects (for example, tables or columns) whose names contain characters such as spaces or reserved tokens.
		/// </summary>
		public string QuoteSuffix
		{
			get { return _QuoteSuffix; }
			set { _QuoteSuffix = value; }
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Retrieves parameter information from the stored procedure specified in the MySqlCommand and populates the Parameters collection of the specified MySqlCommand object.
		/// This method is not currently supported since stored procedures are not available in MySql.
		/// </summary>
		/// <param name="command">The MySqlCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the MySqlCommand.</param>
		/// <exception cref="InvalidOperationException">The command text is not a valid stored procedure name.</exception>
		public static void DeriveParameters(MySqlCommand command)
		{
			throw new MySqlException("DeriveParameters is not supported (due to MySql not supporting SP)");
		}

		/// <summary>
		/// Gets the automatically generated MySqlCommand object required to perform deletions on the database.
		/// </summary>
		/// <returns></returns>
		public MySqlCommand GetDeleteCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateDeleteCommand();
		}

		/// <summary>
		/// Gets the automatically generated MySqlCommand object required to perform insertions on the database.
		/// </summary>
		/// <returns></returns>
		public MySqlCommand GetInsertCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateInsertCommand();
		}

		/// <summary>
		/// Gets the automatically generated MySqlCommand object required to perform updates on the database.
		/// </summary>
		/// <returns></returns>
		public MySqlCommand GetUpdateCommand() 
		{
			if (_schema == null)
				GenerateSchema();
			return CreateUpdateCommand();
		}

		/// <summary>
		/// Refreshes the database schema information used to generate INSERT, UPDATE, or DELETE statements.
		/// </summary>
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
				throw new MySqlException("Improper MySqlCommandBuilder state: adapter is null");
			if (_adapter.SelectCommand == null)
				throw new MySqlException("Improper MySqlCommandBuilder state: adapter's SelectCommand is null");

			MySqlDataReader dr = _adapter.SelectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
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
					throw new InvalidOperationException("MySqlCommandBuilder does not support multi-table statements");
			}
			if (! hasKeyOrUnique)
				throw new InvalidOperationException("MySqlCommandBuilder cannot operate on tables with no unique or key columns");
		}

		private string Quote(string table_or_column)
		{
			if (_QuotePrefix == null || _QuoteSuffix == null)
				return table_or_column;
			return _QuotePrefix + table_or_column + _QuoteSuffix;
		}

		private MySqlParameter CreateParameter(DataRow row, bool Original)
		{
			MySqlParameter p;
			if (Original)
				p = new MySqlParameter( "@Original_" + (string)row["ColumnName"], (MySqlDbType)row["ProviderType"],
					ParameterDirection.Input, (string)row["ColumnName"], DataRowVersion.Original, DBNull.Value );
			else
				p = new MySqlParameter( "@" + (string)row["ColumnName"], (MySqlDbType)row["ProviderType"],
					ParameterDirection.Input, (string)row["ColumnName"], DataRowVersion.Current, DBNull.Value );
			return p;
		}

		private MySqlCommand CreateBaseCommand()
		{
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = _adapter.SelectCommand.Connection;
			cmd.CommandTimeout = _adapter.SelectCommand.CommandTimeout;
			cmd.Transaction = _adapter.SelectCommand.Transaction;
			return cmd;
		}

		private MySqlCommand CreateDeleteCommand()
		{
			if (_deleteCmd != null) return _deleteCmd;

			MySqlCommand cmd = CreateBaseCommand();

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
				if (forinsert) 
				{
					if ((bool)row["IsAutoIncrement"])
						where.Append("last_insert_id()");
					else if ((bool)row["IsKey"])
						where.Append("@" + colname);
				}
				else 
				{
					where.Append("@Original_" + colname);
				}
				where.Append(")");
			}
			return "SELECT " + sel.ToString() + " FROM " + Quote(_tableName) +
				   " WHERE " + where.ToString();
		}

		private string CreateOriginalWhere(MySqlCommand cmd)
		{
			StringBuilder wherestr = new StringBuilder();

			foreach (DataRow row in _schema.Rows)
			{
				if (! IncludedInWhereClause(row)) continue;

				// first update the where clause since it will contain all parameters
				if (wherestr.Length > 0)
					wherestr.Append(" AND ");
				string colname = Quote((string)row["ColumnName"]);

				MySqlParameter op = CreateParameter(row, true);
				cmd.Parameters.Add(op);

				wherestr.Append( "(" + colname + "=@" + op.ParameterName);
				if ((bool)row["AllowDBNull"] == true) 
					wherestr.Append( " or (" + colname + " IS NULL and @" + op.ParameterName + " IS NULL)");
				wherestr.Append(")");
			}
			return wherestr.ToString();
		}

		private MySqlCommand CreateUpdateCommand()
		{
			if (_updateCmd != null) return _updateCmd; 

			MySqlCommand cmd = CreateBaseCommand();

			StringBuilder setstr = new StringBuilder();
		
			foreach (DataRow schemaRow in _schema.Rows)
			{
				string colname = Quote((string)schemaRow["ColumnName"]);

				if (! IncludedInUpdate(schemaRow)) continue;

				if (setstr.Length > 0) 
					setstr.Append(", ");

				MySqlParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname + "=@" + p.ParameterName );
			}

			cmd.CommandText = "UPDATE " + Quote(_tableName) + " SET " + setstr.ToString() + 
							  " WHERE " + CreateOriginalWhere(cmd);
			cmd.CommandText += "; " + CreateFinalSelect(false);

			_updateCmd = cmd;
			return cmd;
		}

		private MySqlCommand CreateInsertCommand()
		{
			if (_insertCmd != null) return _insertCmd;

			MySqlCommand cmd = CreateBaseCommand();

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

				MySqlParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname );
				valstr.Append( "@" + p.ParameterName );
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
//			if ((bool) schemaRow ["IsLong"])
//				return false;
			return true;
		}

		private void SetParameterValues(MySqlCommand cmd, DataRow dataRow)
		{
			foreach (MySqlParameter p in cmd.Parameters)
			{
				if (p.ParameterName.Length >= 8 && p.ParameterName.Substring(0, 8).Equals("Original"))
					p.Value = dataRow[ p.SourceColumn, DataRowVersion.Original ];
				else
					p.Value = dataRow[ p.SourceColumn, DataRowVersion.Current ];
			}
		}

		private void OnRowUpdating(object sender, MySqlRowUpdatingEventArgs args)
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
