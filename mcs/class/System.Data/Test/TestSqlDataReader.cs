//
// Test/SqlDataReader.cs - to test Mono.Data.PostgreSqlClient/PgSqlDataReader.cs
//
// Test to do read a simple forward read only record set.
// Using PgSqlCommand.ExecuteReader() to return a PgSqlDataReader
// which can be used to Read a row
// and Get a String or Int32.
//
// Author:
//	Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using Mono.Data.PostgreSqlClient;

namespace Test.Mono.Data.PostgreSqlClient {
	class TestPgSqlDataReader {

		static void Test(PgSqlConnection con, string sql, 
				CommandType cmdType, CommandBehavior behavior,
				string testDesc) 
		{ 
			PgSqlCommand cmd = null;
			PgSqlDataReader rdr = null;
			
			int c;
			int results = 0;

			Console.WriteLine("Test: " + testDesc);
			Console.WriteLine("[BEGIN SQL]");
			Console.WriteLine(sql);
			Console.WriteLine("[END SQL]");

			cmd = new PgSqlCommand(sql, con);
			cmd.CommandType = cmdType;
						
			Console.WriteLine("ExecuteReader...");
			rdr = cmd.ExecuteReader(behavior);

			if(rdr == null) {
		
				Console.WriteLine("IDataReader has a Null Reference.");
			}
			else {

				do {
					// get the DataTable that holds
					// the schema
					DataTable dt = rdr.GetSchemaTable();

					if(rdr.RecordsAffected != -1) {
						// Results for 
						// SQL INSERT, UPDATE, DELETE Commands 
						// have RecordsAffected >= 0
						Console.WriteLine("Result is from a SQL Command (INSERT,UPDATE,DELETE).  Records Affected: " + rdr.RecordsAffected);
					}
					else if (dt == null)
						Console.WriteLine("Result is from a SQL Command not (INSERT,UPDATE,DELETE).   Records Affected: " + rdr.RecordsAffected);
					else {
						// Results for
						// SQL not INSERT, UPDATE, nor DELETE
						// have RecordsAffected = -1
						Console.WriteLine("Result is from a SQL SELECT Query.  Records Affected: " + rdr.RecordsAffected);
			
						// Results for a SQL Command (CREATE TABLE, SET, etc)
						// will have a null reference returned from GetSchemaTable()
						// 
						// Results for a SQL SELECT Query
						// will have a DataTable returned from GetSchemaTable()

						results++;
						Console.WriteLine("Result Set " + results + "...");
                        			
						// number of columns in the table
						Console.WriteLine("   Total Columns: " +
							dt.Columns.Count);

						// display the schema
						foreach (DataRow schemaRow in dt.Rows) {
							foreach (DataColumn schemaCol in dt.Columns)
								Console.WriteLine(schemaCol.ColumnName + 
									" = " + 
									schemaRow[schemaCol]);
							Console.WriteLine();
						}

						int nRows = 0;
						string output, metadataValue, dataValue;
						// Read and display the rows
						Console.WriteLine("Gonna do a Read() now...");
						while(rdr.Read()) {
							Console.WriteLine("   Row " + nRows + ": ");
					
							for(c = 0; c < rdr.FieldCount; c++) {
								// column meta data 
								DataRow dr = dt.Rows[c];
								metadataValue = 
									"    Col " + 
									c + ": " + 
									dr["ColumnName"];
						
								// column data
								if(rdr.IsDBNull(c) == true)
									dataValue = " is NULL";
								else
									dataValue = 
										": " + 
										rdr.GetValue(c);
					
								// display column meta data and data
								output = metadataValue + dataValue;					
								Console.WriteLine(output);
							}
							nRows++;
						}
						Console.WriteLine("   Total Rows: " + 
							nRows);
					}	
				} while(rdr.NextResult());
				Console.WriteLine("Total Result sets: " + results);
			
				rdr.Close();
			}
					
		}

		[STAThread]
		static void Main(string[] args) {
			String connectionString = null;
			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			PgSqlConnection con;
			con = new PgSqlConnection(connectionString);
			con.Open();

			string sql;

			// Text - only has one query (single query behavior)
			sql = "select * from pg_tables";
			Test(con, sql, CommandType.Text, 
				CommandBehavior.SingleResult, "Text1");

			// Text - only has one query (default behavior)
			sql = "select * from pg_tables";
			Test(con, sql, CommandType.Text, 
				CommandBehavior.Default, "Text2");
			
			// Text - has three queries
			sql =
				"select * from pg_user;" + 
				"select * from pg_tables;" + 
				"select * from pg_database";
			Test(con, sql, CommandType.Text, 
				CommandBehavior.Default, "Text3Queries");
			
			// Table Direct
			sql = "pg_tables";
			Test(con, sql, CommandType.TableDirect, 
				CommandBehavior.Default, "TableDirect1");

			// Stored Procedure
			sql = "version";
			Test(con, sql, CommandType.StoredProcedure, 
				CommandBehavior.Default, "SP1");

			// Text - test a SQL Command (default behavior)
			// Note: this not a SQL Query
			sql = "SET DATESTYLE TO 'ISO'";
			Test(con, sql, CommandType.Text, 
				CommandBehavior.Default, "TextCmd1");

			con.Close();
		}
	}
}
