//
// TestSqlParameters.cs - test parameters for the PostgreSQL .NET Data Provider in Mono
//                        using PgSqlParameter and PgSqlParameterCollection
//
// Note: it currently only tests input parameters.  Output is next on the list.
//       Then output/input and return parameters.
//
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
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
using System.Collections;
using System.Data;
using Mono.Data.PostgreSqlClient;

namespace TestSystemDataPgSqlClient {

	public class TestParameters {
		public static void Main() {
			Console.WriteLine("** Start Test...");
			
			String connectionString = null;
			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			PgSqlConnection con;
			Console.WriteLine("** Creating connection...");
			con = new PgSqlConnection(connectionString);
			Console.WriteLine("** opening connection...");
			con.Open();
		
			string tableName = "pg_type";

			string sql;
			sql = "SELECT * FROM PG_TABLES WHERE TABLENAME = :inTableName";
						
			Console.WriteLine("** Creating command...");
			PgSqlCommand cmd = new PgSqlCommand(sql, con);
			
			// add parameter for inTableName
			Console.WriteLine("** Create parameter...");
			PgSqlParameter parm = new PgSqlParameter("inTableName", DbType.String);
			
			Console.WriteLine("** set dbtype of parameter to string");
			parm.DbType = DbType.String;
			
			Console.WriteLine("** set direction of parameter to input");
			parm.Direction = ParameterDirection.Input;
			
			Console.WriteLine("** set value to the tableName string...");
			parm.Value = tableName;
			
			Console.WriteLine("** add parameter to parameters collection in the command...");
			cmd.Parameters.Add(parm);
			
			PgSqlDataReader rdr;
			Console.WriteLine("** ExecuteReader()...");
			
			rdr = cmd.ExecuteReader();
			
			Console.WriteLine("[][] And now we are going to our results [][]...");
			int c;
			int results = 0;
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
				foreach (DataRow schemaRow in dt.Rows) {
					foreach (DataColumn schemaCol in dt.Columns)
						Console.WriteLine(schemaCol.ColumnName + 
							" = " + 
							schemaRow[schemaCol]);
					Console.WriteLine();
				}

				string output, metadataValue, dataValue;
				int nRows = 0;

				// Read and display the rows
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
			} while(rdr.NextResult());
			Console.WriteLine("Total Result sets: " + results);

			con.Close();
		}
	}
}
