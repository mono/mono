
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
using System.Data;
using System.ComponentModel;

namespace IBM.Data.DB2
{

	public sealed class DB2CommandBuilder : Component
	{

		bool disposed = false;

		private DB2DataAdapter dataAdapter;
		private DB2Command insertCommand;
		private DB2Command updateCommand;
		private DB2Command deleteCommand;

		private string tableName = String.Empty;

		public DB2CommandBuilder ()
		{}

		public DB2CommandBuilder (DB2DataAdapter adapter)
		{
			DataAdapter = adapter;
		}

		public DB2DataAdapter DataAdapter 
		{
			get
			{
				return dataAdapter;
			}
			set
			{
				if (dataAdapter != null)
				{
					throw new Exception ("DataAdapter is already set");
				}
				dataAdapter = value;
				string select_text = dataAdapter.SelectCommand.CommandText;
				string[] words = select_text.Split(new char [] {' '});
				bool from_found = false;
				for (int i = 0; i < words.Length; i++)
				{
					if (from_found && (words[i] != String.Empty))
					{
						tableName = words[i];
						break;
					}
					if (words[i].ToLower() == "from")
					{
						from_found = true;
					}
				}
			}
		}

		public string QuotePrefix 
		{
			get
			{
				return "";
			}
			set
			{ }
		}

		public string QuoteSuffix 
		{
			get
			{
				return "";
			}
			set
			{ }
		}

		public static void DeriveParameters (DB2Command command)
		{}

		public DB2Command GetInsertCommand ()
		{
			DataTable dt = GetSchema();
			if (insertCommand == null)
			{
				string fields = "";
				string values = "";
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					//DataColumn column = dt.Columns[i];
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
				
					if (fields.Length != 0 && !((bool)dr["IsAutoIncrement"]))
					{
						fields += ", ";
						values += ", ";
					}

					if(!((bool)dr["IsAutoIncrement"]))
					{
						fields += column.ColumnName;
						//values += ":v_" + column.ColumnName;
						values += "?";
					}
				}
				if (tableName == String.Empty)
				{
					tableName = dt.TableName;
				}
				DB2Command cmdaux = new DB2Command("insert into " + tableName + " (" + fields + ") values (" + values + ")", dataAdapter.SelectCommand.Connection);
				for (int i = 0;i < dt.Rows.Count;i++)
				{
					//DataColumn column = dt.Columns[i];
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					if (!((bool)dr["IsAutoIncrement"]))
					{
						DB2Parameter aux = new DB2Parameter("v_" + column.ColumnName,  column.DataType);
						aux.Direction = ParameterDirection.Input;
						aux.SourceColumn = column.ColumnName;
						cmdaux.Parameters.Add(aux);
					}
				}
				insertCommand = cmdaux;
			}
			return insertCommand;
		}

		public DB2Command GetUpdateCommand ()
		{
			DataTable dt = GetSchema();
			if (updateCommand == null)
			{
				string sets = "";
				string wheres = "";
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					if (sets.Length != 0 && !((bool)dt.Rows[i]["IsAutoIncrement"]))
					{
						sets += ", ";
					}
					if (i != 0)
					{
						wheres += " and ";
					}
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					if(!((bool)dr["IsAutoIncrement"])){sets += String.Format("{0} = ? ", column.ColumnName);}
					wheres += String.Format("(({0} is null) or ({0} = ?))", column.ColumnName);
				}
				if (tableName == String.Empty)
				{
					tableName = (string)dt.Rows[0]["BaseTableName"];
				}
				DB2Command cmdaux = new DB2Command("update " + tableName + " set " + sets + " where ( " + wheres + " )", dataAdapter.SelectCommand.Connection);
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					if (!((bool)dr["IsAutoIncrement"]))
					{
						DB2Parameter aux = new DB2Parameter("s_" + column.ColumnName, column.DataType);
						aux.Direction = ParameterDirection.Input;
						aux.SourceColumn = column.ColumnName;
						aux.SourceVersion = DataRowVersion.Current;
						cmdaux.Parameters.Add(aux);
					}
				}
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					DB2Parameter aux = new DB2Parameter("w_" + column.ColumnName, column.DataType);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					aux.SourceVersion = DataRowVersion.Original;
					cmdaux.Parameters.Add(aux);
				}
				updateCommand = cmdaux;

			}
			return updateCommand;
		}

		public DB2Command GetDeleteCommand ()
		{
			DataTable dt = GetSchema();
			if (deleteCommand == null)
			{
				string wheres = "";
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					//DataColumn column = row.Table.Columns[i];
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					if (i != 0)
					{
						wheres += " and ";
					}
					//wheres += String.Format("(({0} is null) or ({0} = v_{0}))", column.ColumnName);
					wheres += String.Format("(({0} is null) or ({0} = ?))", column.ColumnName);
				}
				if (tableName == String.Empty)
				{
					tableName = (string)dt.Rows[0]["BaseTableName"];
				}
				DB2Command cmdaux = new DB2Command("delete from " + tableName + " where ( " + wheres + " )", dataAdapter.SelectCommand.Connection);
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					DataRow dr = dt.Rows[i];
					DataColumn column = new DataColumn((string)dr["ColumnName"], DB2TypeConverter.GetManagedType((int)dr["ProviderType"]));
					
					DB2Parameter aux = new DB2Parameter("v_" + column.ColumnName, column.DataType);
					aux.Direction = ParameterDirection.Input;
					aux.SourceColumn = column.ColumnName;
					aux.SourceVersion = DataRowVersion.Original;
					cmdaux.Parameters.Add(aux);
				}
				deleteCommand = cmdaux;
			}
			return deleteCommand;
		}

		public void RefreshSchema ()
		{
			insertCommand = null;
			updateCommand = null;
			deleteCommand = null;
		}

		private DataTable GetSchema()
		{
			dataAdapter.SelectCommand.Connection.Open();
			DB2Command cmd = new DB2Command(dataAdapter.SelectCommand.CommandText, dataAdapter.SelectCommand.Connection);
			DB2DataReader fake = cmd.ExecuteReader(CommandBehavior.KeyInfo);
			
			DataTable dt = fake.GetSchemaTable();
			fake.Close();
			dataAdapter.SelectCommand.Connection.Close();

			return dt;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (insertCommand != null)
					{
						insertCommand.Dispose();
					}
					if (updateCommand != null)
					{
						updateCommand.Dispose();
					}
					if (deleteCommand != null)
					{
						deleteCommand.Dispose();
					}
				}
			}
		}

	}

}
