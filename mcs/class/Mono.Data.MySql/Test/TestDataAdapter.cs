
namespace TestSystemDataSqlClient {
	using System;
	using System.Collections;
	using System.Data;
	using System.Data.Common;
	using Mono.Data.MySql;

	public class TestSqlDataAdapter {
		public static void Test() {
			string connectionString;
			string sqlQuery;
			
			MySqlDataAdapter adapter = null;
			DataSet dataSet = null;

			connectionString =
				"dbname=test;";
				
						
			sqlQuery = "select * from mono_mysql_test";

			System.Console.WriteLine ("new MySqlDataAdapter...");
			adapter = new MySqlDataAdapter (sqlQuery, 
				connectionString);

			System.Console.WriteLine ("new DataSet...");
			dataSet = new DataSet ();

			System.Console.WriteLine("Fill...");
			adapter.Fill (dataSet, "Table1");
			
			System.Console.WriteLine ("Get Each Row in DataTable...");
			if (dataSet != null) {
				
				foreach (DataRow row in dataSet.Tables["Table1"].Rows)
					Console.WriteLine ("int_value: " + 
						row["int_value"]);
				
				string filename = "DataSetTest.xml";
				Console.WriteLine ("Write DataSet to XML file: " + 
					filename);
				dataSet.WriteXml (filename);
			}
			Console.WriteLine ("Done.");

		}

		public static void Main() {
			Test();
		}
	}
}
