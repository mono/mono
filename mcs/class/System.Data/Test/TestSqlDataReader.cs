//
// Test/SqlDataRead.cs
//
// Test to do read a simple forward read only record set.
// Using SqlCommand.ExecuteReader() to return a SqlDataReader
// which can be used to Read a row
// and Get a String or Int32.
//
// Author:
//	Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
//

using System;
using System.Data;
using System.Data.SqlClient;

namespace TestSystemDataSqlClient
{
	class TestSqlDataReader
	{

		static void Test() { 
			SqlConnection con = null;
			SqlCommand cmd = null;
			SqlDataReader rdr = null;
			
			String connectionString = null;
			String sql = null;

			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=danmorg;" +
				"password=viewsonic";

			sql = 	"select tid, tdesc, aint4 " + 
				"from sometable";
			
			con = new SqlConnection(connectionString);
			con.Open();

			Console.WriteLine("sql: " +
				     sql);

			cmd = new SqlCommand(sql, con);
			rdr = cmd.ExecuteReader();

                        // get the DataTable that holds
			// the schema
			DataTable dt = rdr.GetSchemaTable();
			
			// number of columns in the table
			Console.WriteLine("dt.Columns.Count: " +
				dt.Columns.Count);

			// display the schema
			for(int c = 0; c < dt.Columns.Count; c++) {
				Console.WriteLine("* Column Name: " + 
					dt.Columns[c].ColumnName);
				Console.WriteLine("         MaxLength: " +
					dt.Columns[c].MaxLength);
				Console.WriteLine("         Type: " +
					dt.Columns[c].DataType);
			}

			// Read and display the rows
			while(rdr.Read()) {
				Console.WriteLine("Row: " +
					rdr["tid"].ToString() + ", " + 
					rdr["tdesc"].ToString() + ", " + 
					rdr["aint4"].ToString()
					);

				Console.WriteLine("1:" + rdr.GetString(0));
				Console.WriteLine("1:" + rdr.GetString(1));
				Console.WriteLine("2:" + rdr.GetInt32(2));
			}
			
			rdr.Close();
			con.Close();
		}

		[STAThread]
		static void Main(string[] args)
		{
			Test();
		}

	}
}
