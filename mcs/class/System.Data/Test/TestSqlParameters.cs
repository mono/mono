//
// TestSqlParameters.cs - test parameters for the PostgreSQL .NET Data Provider in Mono
//                        using *Parameter and *ParameterCollection
//
// Note: it currently only tests input parameters.  Output is next on the list.
//       Then output/input and return parameters.
//
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace TestSystemDataSqlClient {

	public class TestParameters {
		public static void Main() {
			Console.WriteLine("** Start Test...");
			
			String connectionString = null;
			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			SqlConnection con;
			Console.WriteLine("** Creating connection...");
			con = new SqlConnection(connectionString);
			Console.WriteLine("** opening connection...");
			con.Open();
		
			string tableName = "pg_type";

			string sql;
			sql = "SELECT * FROM PG_TABLES WHERE TABLENAME = :inTableName";
						
			Console.WriteLine("** Creating command...");
			SqlCommand cmd = new SqlCommand(sql, con);
			
			// add parameter for inTableName
			Console.WriteLine("** Create parameter...");
			SqlParameter parm = new SqlParameter("inTableName", SqlDbType.Text);
			Console.WriteLine("** set dbtype of parameter to string");
			parm.DbType = DbType.String;
			Console.WriteLine("** set direction of parameter to input");
			parm.Direction = ParameterDirection.Input;
			Console.WriteLine("** set value to the tableName string...");
			parm.Value = tableName;
			
			Console.WriteLine("** add parameter to parameters collection in the command...");
			cmd.Parameters.Add(parm);
			
			SqlDataReader rdr;
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

			con.Close();
		}
	}
}
