// NpgsqlCommandBuilder.cs
//
// Author:
//   Pedro Martínez Juliá (yoros@wanadoo.es)
//
// Copyright (C) 2003 Pedro Martínez Juliá
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


using System;
using System.Resources;
using System.Data;
using System.Data.Common;
using System.ComponentModel;
using NpgsqlTypes;

namespace Npgsql
{

    ///<summary>
    /// This class is responsible to create database commands for automatic insert, update and delete operations.
    ///</summary>
    public sealed class NpgsqlCommandBuilder : Component
    {

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlCommandBuilder";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlCommandBuilder));
        
        bool disposed = false;


        private NpgsqlDataAdapter data_adapter;
        private NpgsqlCommand insert_command;
        private NpgsqlCommand update_command;
        private NpgsqlCommand delete_command;

        private string quotePrefix = "\"";
        private string quoteSuffix = "\"";
		private DataTable select_schema;

        public NpgsqlCommandBuilder ()
        {}

        public NpgsqlCommandBuilder (NpgsqlDataAdapter adapter)
        {
            DataAdapter = adapter;
        }

        public NpgsqlDataAdapter DataAdapter {
            get
            {
                return data_adapter;
            }
            set
            {
                if (data_adapter != null)
                {
                    throw new InvalidOperationException ("DataAdapter is already set");
                }
                data_adapter = value;
                data_adapter.RowUpdating += new NpgsqlRowUpdatingEventHandler(OnRowUpdating);
            }
        }

        private void OnRowUpdating(Object sender, NpgsqlRowUpdatingEventArgs value) {
            switch (value.StatementType)
            {
                case StatementType.Insert:
                    value.Command = GetInsertCommand(value.Row, false);
                    break;
                case StatementType.Update:
                    value.Command = GetUpdateCommand(value.Row, false);
                    break;
                case StatementType.Delete:
                    value.Command = GetDeleteCommand(value.Row, false);
                    break;
            }

            DataColumnMappingCollection columnMappings = value.TableMapping.ColumnMappings;
            foreach (IDataParameter parameter in value.Command.Parameters)
            {

                string dsColumnName = parameter.SourceColumn;
                if (columnMappings.Contains(parameter.SourceColumn))
                {
                    DataColumnMapping mapping = columnMappings[parameter.SourceColumn];
                    if (mapping != null)
                    {
                        dsColumnName = mapping.DataSetColumn;
                    }
                }

                DataRowVersion rowVersion = DataRowVersion.Default;
                if (value.StatementType == StatementType.Update)
                    rowVersion = parameter.SourceVersion;
                if (value.StatementType == StatementType.Delete)
                    rowVersion = DataRowVersion.Original;
                parameter.Value = value.Row [dsColumnName, rowVersion];
            }
        }

        public string QuotePrefix {
            get
            {
                return quotePrefix;
            }
            set
            {
				quotePrefix = value;
			}
        }

        public string QuoteSuffix {
            get
            {
                return quoteSuffix;
            }
            set
            {
				quoteSuffix = value;
			}
        }

	///<summary>
	///
	/// This method is reponsible to derive the command parameter list with values obtained from function definition. 
	/// It clears the Parameters collection of command. Also, if there is any parameter type which is not supported by Npgsql, an InvalidOperationException will be thrown.
	/// Parameters name will be parameter1, parameter2, ...
	/// For while, only parameter name and NpgsqlDbType are obtained.
	///</summary>
	/// <param name="command">NpgsqlCommand whose function parameters will be obtained.</param>
        public static void DeriveParameters (NpgsqlCommand command)
        {

            // Updated after 0.99.3 to support the optional existence of a name qualifying schema and case insensitivity when the schema ror procedure name do not contain a quote.
            // This fixed an incompatibility with NpgsqlCommand.CheckFunctionReturn(String ReturnType)
            String query = null;
            string procedureName = null;
            string schemaName = null;
            string[] fullName = command.CommandText.Split('.');
            if (fullName.Length > 1 && fullName[0].Length > 0)
            {
                query = "select proargtypes from pg_proc p left join pg_namespace n on p.pronamespace = n.oid where proname=:proname and n.nspname=:nspname";
                schemaName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
                procedureName = (fullName[1].IndexOf("\"") != -1) ? fullName[1] : fullName[1].ToLower();
            }
            else
            {
                query = "select proargtypes from pg_proc where proname = :proname";
                procedureName = (fullName[0].IndexOf("\"") != -1) ? fullName[0] : fullName[0].ToLower();
            }

            NpgsqlCommand c = new NpgsqlCommand(query, command.Connection);
            c.Parameters.Add(new NpgsqlParameter("proname", NpgsqlDbType.Text));

            
            c.Parameters[0].Value = procedureName.Replace("\"", "").Trim();

            if (fullName.Length > 1 && schemaName.Length > 0)
            {
                NpgsqlParameter prm = c.Parameters.Add(new NpgsqlParameter("nspname", NpgsqlDbType.Text));
                prm.Value = schemaName.Replace("\"", "").Trim();
            }
    
            String types = (String) c.ExecuteScalar();

            if (types == null)
                throw new InvalidOperationException (String.Format(resman.GetString("Exception_InvalidFunctionName"), command.CommandText));
    
            command.Parameters.Clear();
            Int32 i = 1;
            
            foreach(String s in types.Split())
            {
                if (!c.Connector.OidToNameMapping.ContainsOID(Int32.Parse(s)))
                {
                    command.Parameters.Clear();
                    throw new InvalidOperationException(String.Format("Invalid parameter type: {0}", s));
                }
                command.Parameters.Add(new NpgsqlParameter("parameter" + i++, c.Connector.OidToNameMapping[Int32.Parse(s)].NpgsqlDbType));
            }
	    	
        }
 
        private string GetQuotedName(string str)
        {
            string result = str;
            if ((QuotePrefix != string.Empty) && !str.StartsWith(QuotePrefix))
            {
                result = QuotePrefix + result;
            }
            if ((QuoteSuffix != string.Empty) && !str.EndsWith(QuoteSuffix))
            {
                result = result + QuoteSuffix;
            }
            return result;
        }


        public NpgsqlCommand GetInsertCommand (DataRow row)
        {
            return GetInsertCommand(row, true);
        }

        private NpgsqlCommand GetInsertCommand(DataRow row, bool setParameterValues)
        {
            if (insert_command == null)
            {
                string fields = "";
                string values = "";
				bool first = true;
				if (select_schema == null)
				{
					BuildSchema();
				}
                string schema_name = string.Empty;
				string table_name = string.Empty;
                string quotedName;
                NpgsqlCommand cmdaux = new NpgsqlCommand();
				foreach(DataRow schemaRow in select_schema.Rows)
				{
					if (!(bool)schemaRow["IsAutoIncrement"])
					{
						if (!first)
						{
							fields += ", ";
							values += ", ";
						}
						else
						{
                            schema_name = (string)schemaRow["BaseSchemaName"];
							table_name = (string)schemaRow["BaseTableName"];
							if (table_name == null || table_name.Length == 0)
							{
								table_name = row.Table.TableName;
							}
						}
                        quotedName = GetQuotedName((string)schemaRow["BaseColumnName"]);
                        DataColumn column = row.Table.Columns[(string)schemaRow["ColumnName"]];

                        fields += quotedName;
                        values += ":param_" + column.ColumnName;
						first = false;

                        NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                        aux.Direction = ParameterDirection.Input;
                        aux.SourceColumn = column.ColumnName;
                        cmdaux.Parameters.Add(aux);
					}
				}
                cmdaux.CommandText = "insert into " + QualifiedTableName(schema_name, table_name) + " (" + fields + ") values (" + values + ")";
                cmdaux.Connection = data_adapter.SelectCommand.Connection;
                insert_command = cmdaux;
            }
            if (setParameterValues)
            {
                SetParameterValuesFromRow(insert_command, row);
            }
            return insert_command;
        }

        public NpgsqlCommand GetUpdateCommand (DataRow row)
        {
            return GetUpdateCommand(row, true);
        }

        private NpgsqlCommand GetUpdateCommand(DataRow row, bool setParameterValues)
        {
            if (update_command == null)
            {
                string sets = "";
				string wheres = "";
				bool first = true;
				if (select_schema == null)
				{
					BuildSchema();
				}
                string schema_name = string.Empty;
                string table_name = string.Empty;
                string quotedName;
                NpgsqlCommand cmdaux = new NpgsqlCommand();
				foreach(DataRow schemaRow in select_schema.Rows)
				{
					if (!first)
					{
						sets += ", ";
						wheres += " and ";
					}
					else
					{
                        schema_name = (string)schemaRow["BaseSchemaName"];
						table_name = (string)schemaRow["BaseTableName"];
						if (table_name == null || table_name.Length == 0)
						{
							table_name = row.Table.TableName;
						}
					}
                    quotedName = GetQuotedName((string)schemaRow["BaseColumnName"]);
                    DataColumn column = row.Table.Columns[(string)schemaRow["ColumnName"]];
                    sets += String.Format("{0} = :s_param_{1}", quotedName, column.ColumnName);
                    wheres += String.Format("(({0} is null) or ({0} = :w_param_{1}))", quotedName, column.ColumnName);
					first = false;

                    NpgsqlNativeTypeInfo typeInfo = NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType);
                    NpgsqlParameter aux_set = new NpgsqlParameter("s_param_" + column.ColumnName, typeInfo);
                    aux_set.Direction = ParameterDirection.Input;
                    aux_set.SourceColumn = column.ColumnName;
                    aux_set.SourceVersion = DataRowVersion.Current;
                    cmdaux.Parameters.Add(aux_set);

                    NpgsqlParameter aux_where = new NpgsqlParameter("w_param_" + column.ColumnName, typeInfo);
                    aux_where.Direction = ParameterDirection.Input;
                    aux_where.SourceColumn = column.ColumnName;
                    aux_where.SourceVersion = DataRowVersion.Original;
                    cmdaux.Parameters.Add(aux_where);
				}
                cmdaux.CommandText = "update " + QualifiedTableName(schema_name, table_name) + " set " + sets + " where ( " + wheres + " )";
                cmdaux.Connection = data_adapter.SelectCommand.Connection;
                update_command = cmdaux;

            }
            if (setParameterValues)
            {
                SetParameterValuesFromRow(update_command, row);
            }
            return update_command;
        }

        public NpgsqlCommand GetDeleteCommand (DataRow row)
        {
            return GetDeleteCommand(row, true);
        }

        private NpgsqlCommand GetDeleteCommand(DataRow row, bool setParameterValues)
        {
            if (delete_command == null)
            {
				string wheres = "";
				bool first = true;
				if (select_schema == null)
				{
					BuildSchema();
				}
                string schema_name = string.Empty;
                string table_name = string.Empty;
                string quotedName;
                NpgsqlCommand cmdaux = new NpgsqlCommand();
				foreach(DataRow schemaRow in select_schema.Rows)
				{
					if (!first)
					{
						wheres += " and ";
					}
					else
					{
                        schema_name = (string)schemaRow["BaseSchemaName"];
						table_name = (string)schemaRow["BaseTableName"];
						if (table_name == null || table_name.Length == 0)
						{
							table_name = row.Table.TableName;
						}
                    }

                    quotedName = GetQuotedName((string)schemaRow["BaseColumnName"]);
                    DataColumn column = row.Table.Columns[(string)schemaRow["ColumnName"]];

					wheres += String.Format("(({0} is null) or ({0} = :param_{1}))", quotedName , column.ColumnName);
                    first = false;

                    NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                    aux.Direction = ParameterDirection.Input;
                    aux.SourceColumn = column.ColumnName;
                    aux.SourceVersion = DataRowVersion.Original;
                    cmdaux.Parameters.Add(aux);
				}
                cmdaux.CommandText = "delete from " + QualifiedTableName(schema_name, table_name) + " where ( " + wheres + " )";
                cmdaux.Connection = data_adapter.SelectCommand.Connection;
                delete_command = cmdaux;
            }
            if (setParameterValues)
            {
                SetParameterValuesFromRow(delete_command, row);
            }
            return delete_command;
        }

        public void RefreshSchema ()
        {
            insert_command = null;
            update_command = null;
            delete_command = null;
			select_schema = null;
        }

        protected override void Dispose (bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (insert_command != null)
                    {
                        insert_command.Dispose();
                    }
                    if (update_command != null)
                    {
                        update_command.Dispose();
                    }
                    if (delete_command != null)
                    {
                        delete_command.Dispose();
                    }

                    data_adapter.RowUpdating -= new NpgsqlRowUpdatingEventHandler(OnRowUpdating);
                }
            }
            base.Dispose(disposing);
        }

		private void BuildSchema()
		{
			if (select_schema == null)
			{
				bool openedConnection = false;
				try
				{
					if ((data_adapter.SelectCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
					{
						data_adapter.SelectCommand.Connection.Open();
						openedConnection = true;
					}
					using (NpgsqlDataReader reader = data_adapter.SelectCommand.ExecuteReader(CommandBehavior.SchemaOnly|CommandBehavior.KeyInfo))
					{
						select_schema = reader.GetSchemaTable();
					}
				}
				finally
				{
					if (openedConnection)
					{
						data_adapter.SelectCommand.Connection.Close();
					}
				}
			}
		}

        /*~NpgsqlCommandBuilder ()
        {
            Dispose(false);
        }*/

        private string QualifiedTableName(string schema, string tableName)
        {
            if (schema == null || schema.Length == 0)
            {
                return GetQuotedName(tableName);
            }
            else
            {
                return GetQuotedName(schema) + "." + GetQuotedName(tableName);
            }
        }

        private static void SetParameterValuesFromRow(NpgsqlCommand command, DataRow row)
        {
            foreach (NpgsqlParameter parameter in command.Parameters)
            {
                parameter.Value = row[parameter.SourceColumn, parameter.SourceVersion];
            }
        }
    }

}
