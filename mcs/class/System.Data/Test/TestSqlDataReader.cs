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

			sql = 	"select tid, tdesc " + 
				"from sometable";
			
			con = new SqlConnection(connectionString);
			con.Open();

			Console.WriteLine("sql: " +
				     sql);

			cmd = new SqlCommand(sql, con);
			rdr = cmd.ExecuteReader();
			
			// rdr.GetInt32(0)  
			// rdr.GetString(1)
			while(rdr.Read()) {
				Console.WriteLine(
					rdr["fname"].ToString() + 
					", " + 
					rdr["lname"].ToString());
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
