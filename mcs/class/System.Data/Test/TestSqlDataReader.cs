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

namespace TestSystemDataSqlClient {
	class TestSqlDataReader {

		static void Test() { 
			SqlConnection con = null;
			SqlCommand cmd = null;
			SqlDataReader rdr = null;
			
			String connectionString = null;
			String sql = null;
			int c;
			int results = 0;

			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
				
			sql = 	"select * from pg_user;" + 
				"select * from pg_tables;" + 
				"select * from pg_database;";
							
			con = new SqlConnection(connectionString);
			con.Open();

			Console.WriteLine("sql: " +
				sql);

			cmd = new SqlCommand(sql, con);
			Console.WriteLine("ExecuteReader...");
			rdr = cmd.ExecuteReader();

			do {
				results++;
				Console.WriteLine("Result Set " + results + "...");

				// get the DataTable that holds
				// the schema
				DataTable dt = rdr.GetSchemaTable();
                        			
				// number of columns in the table
				Console.WriteLine("   Total Columns: " +
					dt.Columns.Count);

				// display the schema
				for(c = 0; c < dt.Columns.Count; c++) {
					Console.WriteLine("   Column Name: " + 
						dt.Columns[c].ColumnName);
					Console.WriteLine("          MaxLength: " +
						dt.Columns[c].MaxLength);
					Console.WriteLine("          Type: " +
						dt.Columns[c].DataType);
				}
				int nRows = 0;

				// Read and display the rows
				while(rdr.Read()) {
					Console.WriteLine("   Row " + nRows + ": ");

					for(c = 0; c < rdr.FieldCount; c++) {
						if(rdr.IsDBNull(c) == true)
							Console.WriteLine("      " + 
								rdr.GetName(c) + " is DBNull");
						else
							Console.WriteLine("      " + 
								rdr.GetName(c) + ": " +
								rdr[c].ToString());
					}
					nRows++;
				}
				Console.WriteLine("   Total Rows: " + 
					nRows);
			} while(rdr.NextResult());
			Console.WriteLine("Total Result sets: " + results);
			
			rdr.Close();
			con.Close();
		}

		[STAThread]
		static void Main(string[] args) {
			Test();
		}
	}
}
