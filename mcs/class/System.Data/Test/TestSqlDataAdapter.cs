//
// TestPgSqlDataAdapter - tests PgSqlDataAdapter, DbDataAdapter, DataSet, DataTable,
//                      DataRow, and DataRowCollection by retrieving data
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
using Mono.Data.PostgreSqlClient;

namespace TestSystemDataPgSqlClient 
{
	public class TestPgSqlDataAdapter 
	{
		public static void Test() 
		{
			string connectionString;
			string sqlQuery;
			PgSqlDataAdapter adapter;
			DataSet dataSet = null;

			connectionString =
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			sqlQuery = "select * from pg_tables";

			System.Console.WriteLine ("new PgSqlDataAdapter...");
			adapter = new PgSqlDataAdapter (sqlQuery, 
					connectionString);

			System.Console.WriteLine ("new DataSet...");
			dataSet = new DataSet ();

			try {
				System.Console.WriteLine("Fill...");
				adapter.Fill (dataSet);

			}
			catch (NotImplementedException e) {
				Console.WriteLine("Exception Caught: " + e);
			}		
			
			System.Console.WriteLine ("get row...");
			if (dataSet != null) {
				foreach (DataRow row in dataSet.Tables["Table"].Rows)
					Console.WriteLine("tablename: " + row["tablename"]);
				System.Console.WriteLine("Done.");
			}

		}

		public static void Main() 
		{
			Test();
		}
	}
}
