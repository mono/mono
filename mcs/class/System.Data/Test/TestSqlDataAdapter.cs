//
// TestSqlDataAdapter - tests SqlDataAdapter, DbDataAdapter, DataSet, DataTable,
//                      DataRow, and DataRowCollection by retrieving data
//
// Note: it currently causes an NotImplementException for SqlCommand::NextResult()
//
// Authors:
//      Tim Coleman <tim@timcoleman.com>
//      Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Tim Coleman
// (c)copyright 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace TestSystemDataSqlClient 
{
	public class TestSqlDataAdapter 
	{
		public static void Test() 
		{
			string connectionString;
			string sqlQuery;
			SqlDataAdapter adapter;
			DataSet dataSet;
			DataRow row;

			connectionString =
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			sqlQuery = "select * from pg_tables";

			try {
				System.Console.WriteLine ("new SqlDataAdapter...");
				adapter = new SqlDataAdapter (sqlQuery, 
						connectionString);

				System.Console.WriteLine ("open connection...");
				adapter.SelectCommand.Connection.Open ();
				
				System.Console.WriteLine ("new DataSet...");
				dataSet = new DataSet ();

				System.Console.WriteLine("Fill...");
				adapter.Fill (dataSet);

			}
			catch (NotImplementedException e) {
				Console.WriteLine("Exception Caught: " + e);
			}		
			
			System.Console.WriteLine ("get row...");
			row = dataSet.Tables["Table"].Rows[0];

			Console.WriteLine("tablename: " + row["tablename"]);
			System.Console.WriteLine("Done.");

		}

		public static void Main() 
		{
			Test();
		}
	}
}
