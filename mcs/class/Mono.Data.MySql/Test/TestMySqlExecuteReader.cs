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

namespace TestSystemDataSqlClient {
	using System;
	using System.Data;
	using Mono.Data.MySql;

	class TestSqlDataReader {

		[STAThread]
		static void Main(string[] args) {
			Console.WriteLine("Started.");

			String connectionString = null;
			connectionString = 
				"dbname=mysql";
						
			MySqlConnection con;
			Console.WriteLine("Create MySQL Connection...");
			con = new MySqlConnection(connectionString);
			Console.WriteLine("Open the connection...");
			con.Open();

			string sql;
			sql = "select * from db";

			Console.WriteLine("Create command...");
			MySqlCommand cmd;
			cmd = con.CreateCommand();

			cmd.CommandText = sql;

			MySqlDataReader reader;
			Console.WriteLine("ExecuteReader...");
			reader = cmd.ExecuteReader();

			int row = 0;
			Console.WriteLine("Reading data...");
			while(reader.Read()){
				row++;
				Console.WriteLine("Row: " + row);
				for(int col = 0; col < reader.FieldCount; col++) {
					Console.WriteLine("  Field: " + col);
					
					Console.WriteLine("      Name: " + 
						reader.GetName(col));
					Console.WriteLine("      Value: " + 
						reader.GetValue(col));
				}
			}
			Console.WriteLine("Clean up...");

			reader.Close();
			reader = null;
			cmd.Dispose();
			cmd = null;
			con.Close();
			con = null;

			Console.WriteLine("Done.");
		}
	}
}
