/* PostgresTest.cs - based on the postgres-test.c in libgda
 * 
 * Copyright (C) 2002 Gonzalo Paniagua Javier
 * Copyright (C) 2002 Daniel Morgan
 *
 * ORIGINAL AUTHOR:
 *	Gonzalo Paniagua Javier <gonzalo@gnome-db.org>
 * PORTING FROM C TO C# AUTHOR:
 *	Daniel Morgan <danmorg@sc.rr.com>
 *
 * Permission was given from the original author, Gonzalo Paniagua Javier,
 * to port and include his original work in Mono.
 * 
 * The original work falls under the LGPL, but the port to C# falls
 * under the X11 license.
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as
 * published by the Free Software Foundation; either version 2 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; see the file COPYING.  If not,
 * write to the Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

using System;
using System.Data;
using System.Data.SqlClient;

namespace TestSystemDataSqlClient {

	class PostgresTest {

		static void CreateTable (IDbConnection cnc) {
						
			IDbCommand createCommand = cnc.CreateCommand();
	
			createCommand.CommandText = 
				"create table mono_postgres_test (" +
				"boolean_value boolean, " +
				"int2_value smallint, " +
				"int4_value integer, " +
				"bigint_value bigint, " +
				"float_value real, " + 
				"double_value double precision, " +
				"numeric_value numeric(15, 3), " +
				"char_value char(50), " +
				"varchar_value varchar(20), " +
				"text_value text, " +
				"point_value point, " +
				"time_value time, " +
				"date_value date, " +
				"timestamp_value timestamp, " +
				"null_boolean_value boolean, " +
				"null_int2_value smallint, " +
				"null_int4_value integer, " +
				"null_bigint_value bigint, " +
				"null_float_value real, " + 
				"null_double_value double precision, " +
				"null_numeric_value numeric(15, 3), " +
				"null_char_value char(50), " +
				"null_varchar_value varchar(20), " +
				"null_text_value text, " +
				"null_point_value point, " +
				"null_time_value time, " +
				"null_date_value date, " +
				"null_timestamp_value timestamp " +
				")";			
	
			createCommand.ExecuteNonQuery ();
		}

		static void DropTable (IDbConnection cnc) {
				 
			IDbCommand dropCommand = cnc.CreateCommand ();

			dropCommand.CommandText =
				"drop table mono_postgres_test";
							
			dropCommand.ExecuteNonQuery ();
		}

		static void InsertData (IDbConnection cnc) {		

			IDbCommand insertCommand = cnc.CreateCommand();
		
			insertCommand.CommandText =
				"insert into mono_postgres_test (" +
				"boolean_value, " +
				"int2_value, " +
				"int4_value, " +
				"bigint_value, " +
				"float_value, " +
				"double_value, " +
				"numeric_value, " +
				"char_value, " +
				"varchar_value, " +
				"text_value, " +
				"time_value, " +
				"date_value, " +
				"timestamp_value, " +
				"point_value " +
				") values (" +
				"'T', " +
				"-22, " +
				"1048000, " +
				"123456789012345, " +
				"3.141592, " +
				"3.1415926969696, " +
				"123456789012.345, " +
				"'This is a char', " +
				"'This is a varchar', " +
				"'This is a text', " +
				"'21:13:14', " +
				"'2000-02-29', " +
				"'2004-02-29 14:00:11.31', " +
				"'(1,0)' " +
				")";

			insertCommand.ExecuteNonQuery ();
		}

		static IDataReader SelectData (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// FIXME: System.Data classes need to handle NULLs
			// FIXME: System.Data needs to handle more data types
			/*
			selectCommand.CommandText = 
				"select * " +
				"from mono_postgres_test";
			*/

			selectCommand.CommandText = 
				"select " +				
				"boolean_value, " +
				"int2_value, " +
				"int4_value, " +
				"bigint_value, " +
				"float_value, " + 
				"double_value, " +
				"numeric_value, " +
				"char_value, " +
				"varchar_value, " +
				"text_value, " +
				"point_value, " +
				"time_value, " +
				"date_value, " +
				"timestamp_value, " +
				"null_boolean_value, " +
				"null_int2_value, " +
				"null_int4_value, " +
				"null_bigint_value, " +
				"null_float_value, " + 
				"null_double_value, " +
				"null_numeric_value, " +
				"null_char_value, " +
				"null_varchar_value, " +
				"null_text_value, " +
				"null_point_value, " +
				"null_time_value, " +
				"null_date_value, " +
				"null_timestamp_value " +
				"from mono_postgres_test";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		static void UpdateData (IDbConnection cnc) {
	
			IDbCommand updateCommand = cnc.CreateCommand();		
		
			updateCommand.CommandText = 
				"update mono_postgres_test " +				
				"set " +
					"boolean_value = 'F', " +
					"int2_value    = 5, " +
					"int4_value    = 3, " +
					"bigint_value  = 9, " +
					"char_value    = 'Mono.Data!'   , " +
					"varchar_value = 'It was not me!', " +
					"text_value    = 'We got data!'   " +
				"where int2_value = -22";

			updateCommand.ExecuteNonQuery ();		
		}

		// used to do a min(), max(), count(), sum(), or avg()
		static object SelectAggregate (IDbConnection cnc, String agg) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			object data;

			Console.WriteLine("Aggregate: " + agg);

			selectCommand.CommandText = 
				"select " + agg +
				"from mono_postgres_test";

			data = selectCommand.ExecuteScalar ();

			Console.WriteLine("Agg Result: " + data);

			return data;
		}

		/* Postgres provider tests */
		static void DoPostgresTest (IDbConnection cnc) {

			IDataReader reader;
			Object oDataValue;

			Console.WriteLine ("\tPostgres provider specific tests...\n");

			/* Drops the gda_postgres_test table. */
			Console.WriteLine ("\t\tDrop table: ");
			try {
				DropTable (cnc);
				Console.WriteLine ("OK");
			}
			catch (SqlException e) {
				Console.WriteLine("Error (don't worry about this one)" + e);
			}
			
			try {
				/* Creates a table with all supported data types */
				Console.WriteLine ("\t\tCreate table with all supported types: ");
				CreateTable (cnc);
				Console.WriteLine ("OK");
				
				/* Inserts values */
				Console.WriteLine ("\t\tInsert values for all known types: ");
				InsertData (cnc);
				Console.WriteLine ("OK");

				/* Update values */
				Console.WriteLine ("\t\tUpdate values: ");
				UpdateData (cnc);
				Console.WriteLine ("OK");

				/* Inserts values */
				Console.WriteLine ("\t\tInsert values for all known types: ");
				InsertData (cnc);
				Console.WriteLine ("OK");			

				/* Select aggregates */
				SelectAggregate (cnc, "count(*)");
				//SelectAggregate (cnc, "avg(int4_value)");
				SelectAggregate (cnc, "min(text_value)");
				SelectAggregate (cnc, "max(int4_value)");
				SelectAggregate (cnc, "sum(int4_value)");

				/* Select values */
				Console.WriteLine ("\t\tSelect values from the database: ");
				reader = SelectData (cnc);

                                // get the DataTable that holds
				// the schema
				Console.WriteLine("\t\tGet Schema.");
				DataTable dt = reader.GetSchemaTable();
			
				// number of columns in the table
				Console.WriteLine("dt.Columns.Count: " +
					dt.Columns.Count);

				int c;
				// display the schema
				for(c = 0; c < dt.Columns.Count; c++) {
					Console.WriteLine("* Column Name: " + 
						dt.Columns[c].ColumnName);
					Console.WriteLine("         MaxLength: " +
						dt.Columns[c].MaxLength);
					Console.WriteLine("         Type: " +
						dt.Columns[c].DataType);
				}

				int nRows = 0;
				string metadataValue;
				string dataValue;
				string output;
				// Read and display the rows
				while(reader.Read()) {
					Console.WriteLine ("Row " + nRows + ":");
					for(c = 0; c < reader.FieldCount; c++) {
						
						// column meta data 
						metadataValue = 
							"    Col " + 
							c + ": " + 
							dt.Columns[c].ColumnName;
						
						// column data
						if(reader.IsDBNull(c) == true)
							dataValue = " is NULL";
						else
							dataValue = 
								": " + 
								reader.GetValue(c);
					
						// display column meta data and data
						output = metadataValue + dataValue;					
						Console.WriteLine(output);
					}
	
					nRows++;
				}
				reader.Close();
				Console.WriteLine ("Rows: " + nRows);		

				/* Clean up */
				Console.WriteLine ("Clean up...");
				Console.WriteLine ("\t\tDrop table...");
				DropTable (cnc);
				Console.WriteLine("OK");
			}
			catch(Exception e) {
				Console.WriteLine("Exception caught: " + e);
			}
		}

		[STAThread]
		static void Main(string[] args)
		{
			SqlConnection cnc = new SqlConnection ();

			/*
			string connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=userid;" +
				"password=password";
			*/

			string connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres;";

			cnc.ConnectionString =  connectionString;

			cnc.Open();
			DoPostgresTest(cnc);

			cnc.Close();
		}
	}
}
