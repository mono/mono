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
using System.ComponentModel;

namespace Npgsql {

	public sealed class NpgsqlCommandBuilder : Component {
		
		bool disposed = false;

		private NpgsqlDataAdapter data_adapter;
		private NpgsqlCommand insert_command;
		private NpgsqlCommand update_command;
		private NpgsqlCommand delete_command;

		private string table_name = String.Empty;

		public NpgsqlCommandBuilder () {
		}

		public NpgsqlCommandBuilder (NpgsqlDataAdapter adapter) {
			DataAdapter = adapter;
		}

		public NpgsqlDataAdapter DataAdapter {
			get {
				return data_adapter;
			}
			set {
				if (data_adapter != null) {
					throw new Exception ("DataAdapter is already set");
				}
				data_adapter = value;
				string select_text = data_adapter.SelectCommand.CommandText;
				string[] words = select_text.Split(new char [] {' '});
				bool from_found = false;
				for (int i = 0; i < words.Length; i++) {
					if (from_found && (words[i] != String.Empty)) {
						table_name = words[i];
						break;
					}
					if (words[i].ToLower() == "from") {
						from_found = true;
					}
				}
			}
		}

		public string QuotePrefix {
			get { return ""; }
			set { }
		}

		public string QuoteSuffix {
			get { return ""; }
			set { }
		}

		public static void DeriveParameters (NpgsqlCommand command) {
		}

		public NpgsqlCommand GetInsertCommand (DataRow row) {
			if (insert_command == null) {
				string fields = "";
				string values = "";
				for (int i = 0; i < row.Table.Columns.Count; i++) {
					DataColumn column = row.Table.Columns[i];
					if (i != 0) {
						fields += ", ";
						values += ", ";
					}
					fields += column.ColumnName;
					values += ":param_" + column.ColumnName;
				}
				if (table_name == String.Empty) {
					table_name = row.Table.TableName;
				}
				NpgsqlCommand cmdaux = new NpgsqlCommand("insert into " + table_name + " (" + fields + ") values (" + values + ")", data_adapter.SelectCommand.Connection);
				foreach (DataColumn column in row.Table.Columns) {
					NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, row[column]);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					cmdaux.Parameters.Add(aux);
				}
				insert_command = cmdaux;
			}
			return insert_command;
		}

		public NpgsqlCommand GetUpdateCommand (DataRow row) {
			if (update_command == null) {
				string sets = "";
				string wheres = "";
				for (int i = 0; i < row.Table.Columns.Count; i++) {
					if (i != 0) {
						sets += ", ";
						wheres += " and ";
					}
					DataColumn column = row.Table.Columns[i];
					sets += String.Format("{0} = :s_param_{0}", column.ColumnName);
					wheres += String.Format("(({0} is null) or ({0} = :w_param_{0}))", column.ColumnName);
				}
				if (table_name == String.Empty) {
					table_name = row.Table.TableName;
				}
				NpgsqlCommand cmdaux = new NpgsqlCommand("update " + table_name + " set " + sets + " where ( " + wheres + " )", data_adapter.SelectCommand.Connection);
				foreach (DataColumn column in row.Table.Columns) {
					NpgsqlParameter aux = new NpgsqlParameter("s_param_" + column.ColumnName, row[column]);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					aux.SourceVersion = DataRowVersion.Current;
					cmdaux.Parameters.Add(aux);
				}
				foreach (DataColumn column in row.Table.Columns) {
					NpgsqlParameter aux = new NpgsqlParameter("w_param_" + column.ColumnName, row[column]);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					aux.SourceVersion = DataRowVersion.Original;
					cmdaux.Parameters.Add(aux);
				}
				update_command = cmdaux;

			}
			return update_command;
		}

		public NpgsqlCommand GetDeleteCommand (DataRow row) {
			if (delete_command == null) {
				string wheres = "";
				for (int i = 0; i < row.Table.Columns.Count; i++) {
					DataColumn column = row.Table.Columns[i];
					if (i != 0) {
						wheres += " and ";
					}
					wheres += String.Format("(({0} is null) or ({0} = :param_{0}))", column.ColumnName);
				}
				if (table_name == String.Empty) {
					table_name = row.Table.TableName;
				}
				NpgsqlCommand cmdaux = new NpgsqlCommand("delete from " + table_name + " where ( " + wheres + " )", data_adapter.SelectCommand.Connection);
				foreach (DataColumn column in row.Table.Columns) {
					NpgsqlParameter aux = new NpgsqlParameter("param_" + column.ColumnName, row[column,DataRowVersion.Original]);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					aux.SourceVersion = DataRowVersion.Original;
					cmdaux.Parameters.Add(aux);
				}
				delete_command = cmdaux;
			}
			return delete_command;
		}

		public void RefreshSchema () {
			insert_command = null;
			update_command = null;
			delete_command = null;
		}

		protected override void Dispose (bool disposing) {
			if (!disposed) {
				if (disposing) {
					if (insert_command != null) {
						insert_command.Dispose();
					}
					if (update_command != null) {
						update_command.Dispose();
					}
					if (delete_command != null) {
						delete_command.Dispose();
					}
				}
			}
		}

		~NpgsqlCommandBuilder () {
			Dispose(false);
		}

	}

}

