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

        bool disposed = false;


        private NpgsqlDataAdapter data_adapter;
        private NpgsqlCommand insert_command;
        private NpgsqlCommand update_command;
        private NpgsqlCommand delete_command;

        private string table_name = String.Empty;

        private string quotePrefix = "\"";
        private string quoteSuffix = "\"";

        public NpgsqlCommandBuilder ()
        {}

        public NpgsqlCommandBuilder (NpgsqlDataAdapter adapter)
        {
            DataAdapter = adapter;
            adapter.RowUpdating += new NpgsqlRowUpdatingEventHandler(OnRowUpdating);
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
                string select_text = data_adapter.SelectCommand.CommandText;
                string[] words = select_text.Split(new char [] {' '});
                bool from_found = false;
                for (int i = 0; i < words.Length; i++)
                {
                    if (from_found && (words[i] != String.Empty))
                    {
                        table_name = words[i];
                        break;
                    }
                    if (words[i].ToLower() == "from")
                    {
                        from_found = true;
                    }
                }
            }
        }

        private void OnRowUpdating(Object sender, NpgsqlRowUpdatingEventArgs value)
        {
            switch (value.StatementType)
            {
            case StatementType.Insert:
                value.Command = GetInsertCommand(value.Row);
                break;
            case StatementType.Update:
                value.Command = GetUpdateCommand(value.Row);
                break;
            case StatementType.Delete:
                value.Command = GetDeleteCommand(value.Row);
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
            value.Row.AcceptChanges ();
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
            String query = "select proargtypes from pg_proc where proname = :procname";

            NpgsqlCommand c = new NpgsqlCommand(query, command.Connection);
            c.Parameters.Add(new NpgsqlParameter("procname", NpgsqlDbType.Text));
            c.Parameters[0].Value = command.CommandText;

            String types = (String) c.ExecuteScalar();

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
            if (insert_command == null)
            {
                string fields = "";
                string values = "";
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    DataColumn column = row.Table.Columns[i];
                    if (i != 0)
                    {
                        fields += ", ";
                        values += ", ";
                    }
                    fields += GetQuotedName(column.ColumnName);
                    values += ":param_" + column.ColumnName;
                }
                if (table_name == String.Empty)
                {
                    table_name = row.Table.TableName;
                }
                NpgsqlCommand cmdaux = new NpgsqlCommand("insert into " + GetQuotedName(table_name) + " (" + fields + ") values (" + values + ")", data_adapter.SelectCommand.Connection);
                foreach (DataColumn column in row.Table.Columns)
                {
                    NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, row[column], NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                    aux.Direction = ParameterDirection.Input;
                    aux.SourceColumn = column.ColumnName;
                    cmdaux.Parameters.Add(aux);
                }
                insert_command = cmdaux;
            }
            return insert_command;
        }

        public NpgsqlCommand GetUpdateCommand (DataRow row)
        {
            if (update_command == null)
            {
                string sets = "";
                string wheres = "";
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    if (i != 0)
                    {
                        sets += ", ";
                        wheres += " and ";
                    }
                    DataColumn column = row.Table.Columns[i];
                    sets += String.Format(GetQuotedName("{0}") + " = :s_param_{0}", column.ColumnName);
                    wheres += String.Format("((" + GetQuotedName("{0}") + " is null) or (" + GetQuotedName("{0}") + " = :w_param_{0}))", column.ColumnName);
                }
                if (table_name == String.Empty)
                {
                    table_name = row.Table.TableName;
                }
                NpgsqlCommand cmdaux = new NpgsqlCommand("update " + GetQuotedName(table_name) + " set " + sets + " where ( " + wheres + " )", data_adapter.SelectCommand.Connection);
                foreach (DataColumn column in row.Table.Columns)
                {
                    NpgsqlParameter aux = new NpgsqlParameter("s_param_" + column.ColumnName, row[column], NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                    aux.Direction = ParameterDirection.Input;
                    aux.SourceColumn = column.ColumnName;
                    aux.SourceVersion = DataRowVersion.Current;
                    cmdaux.Parameters.Add(aux);
                }
                foreach (DataColumn column in row.Table.Columns)
                {
                    NpgsqlParameter aux = new NpgsqlParameter("w_param_" + column.ColumnName, row[column], NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                    aux.Direction = ParameterDirection.Input;
                    aux.SourceColumn = column.ColumnName;
                    aux.SourceVersion = DataRowVersion.Original;
                    cmdaux.Parameters.Add(aux);
                }
                update_command = cmdaux;

            }
            return update_command;
        }

        public NpgsqlCommand GetDeleteCommand (DataRow row)
        {
            if (delete_command == null)
            {
                string wheres = "";
                for (int i = 0; i < row.Table.Columns.Count; i++)
                {
                    DataColumn column = row.Table.Columns[i];
                    if (i != 0)
                    {
                        wheres += " and ";
                    }
                    wheres += String.Format("((" + GetQuotedName("{0}") + " is null) or (" + GetQuotedName("{0}") + " = :param_{0}))", column.ColumnName);
                }
                if (table_name == String.Empty)
                {
                    table_name = row.Table.TableName;
                }
                NpgsqlCommand cmdaux = new NpgsqlCommand("delete from " + GetQuotedName(table_name) + " where ( " + wheres + " )", data_adapter.SelectCommand.Connection);
                foreach (DataColumn column in row.Table.Columns)
                {
                    NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, row[column,DataRowVersion.Original], NpgsqlTypesHelper.GetNativeTypeInfo(column.DataType));
                    aux.Direction = ParameterDirection.Input;
                    aux.SourceColumn = column.ColumnName;
                    aux.SourceVersion = DataRowVersion.Original;
                    cmdaux.Parameters.Add(aux);
                }
                delete_command = cmdaux;
            }
            return delete_command;
        }

        public void RefreshSchema ()
        {
            insert_command = null;
            update_command = null;
            delete_command = null;
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
                }
            }
        }

        /*~NpgsqlCommandBuilder ()
        {
            Dispose(false);
        }*/

    }

}
