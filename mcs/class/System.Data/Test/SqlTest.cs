/* SqlTest.cs - based on PostgresTest.cs
 * 
 * Copyright (C) 2002 Gonzalo Paniagua Javier
 * Copyright (C) 2002 Daniel Morgan
 * Copyright (C) 2002 Tim Coleman
 *
 * ORIGINAL AUTHOR:
 *	Gonzalo Paniagua Javier <gonzalo@gnome-db.org>
 * PORTING FROM C TO C# AUTHOR:
 *	Daniel Morgan <danmorg@sc.rr.com>
 * PORTING TO SQL SERVER AUTHOR:
 *	Tim Coleman <tim@timcoleman.com>
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

namespace Test.Mono.Data.SqlClient {

	class SqlTest {

		// execute SQL CREATE TABLE Command using ExecuteNonQuery()
		static void CreateTable (IDbConnection cnc) {
						
			IDbCommand createCommand = cnc.CreateCommand();
	
			createCommand.CommandText = 
				"create table mono_sql_test (" +
				"boolean_value bit, " +
				"byte_value tinyint, " +
				"int2_value smallint, " +
				"int4_value integer, " +
				"float_value real, " + 
				"double_value float, " +
				"numeric_value decimal(15, 3), " +
				"char_value char(50), " +
				"nchar_value nchar(50), " +
				"varchar_value varchar(20), " +
				"nvarchar_value nvarchar(20), " +
				"text_value text, " +
				"ntext_value ntext, " +
				"datetime_value datetime, " +
				"null_boolean_value bit, " +
				"null_byte_value tinyint, " +
				"null_int2_value smallint, " +
				"null_int4_value integer, " +
				"null_float_value real, " + 
				"null_double_value float, " +
				"null_numeric_value decimal(15, 3), " +
				"null_char_value char(50), " +
				"null_nchar_value nchar(50), " +
				"null_varchar_value varchar(20), " +
				"null_nvarchar_value nvarchar(20), " +
				"null_text_value text, " +
				"null_ntext_value ntext, " +
				"null_datetime_value datetime " +
				")";			
	
			createCommand.ExecuteNonQuery ();
		}

		// execute SQL DROP TABLE Command using ExecuteNonQuery
		static void DropTable (IDbConnection cnc) {
				 
			IDbCommand dropCommand = cnc.CreateCommand ();

			dropCommand.CommandText =
				"drop table mono_sql_test";
							
			dropCommand.ExecuteNonQuery ();
		}

		// execute stored procedure using ExecuteScalar()
		static object CallStoredProcedure (IDbConnection cnc) {
				 
			IDbCommand callStoredProcCommand = cnc.CreateCommand ();
			object data;

			callStoredProcCommand.CommandType = 
				CommandType.StoredProcedure;
			callStoredProcCommand.CommandText =
				"sp_server_info";
							
			data = callStoredProcCommand.ExecuteScalar ();

			return data;
		}

		// execute SQL INSERT Command using ExecuteNonQuery()
		static void InsertData (IDbConnection cnc) {		

			IDbCommand insertCommand = cnc.CreateCommand();
		
			insertCommand.CommandText =
				"insert into mono_sql_test (" +
				"boolean_value, " +
				"byte_value, " +
				"int2_value, " +
				"int4_value, " +
				"float_value, " + 
				"double_value, " +
				"numeric_value, " +
				"char_value, " +
				"nchar_value, " +
				"varchar_value, " +
				"nvarchar_value, " +
				"text_value, " +
				"ntext_value, " +
				"datetime_value " +
				") values (" +
				"@p1, " +
				"@p2, " +
				"@p3, " +
				"@p4, " +
				"@p5, " +
				"@p6, " +
				"@p7, " +
				"@p8, " +
				"@p9, " +
				"@p10, " +
				"@p11, " +
				"@p12, " +
				"@p13, " +
				"@p14 " +
				")";

			SqlParameterCollection parameters = ((SqlCommand) insertCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit, 1);
			parameters.Add ("@p2",  SqlDbType.TinyInt, 1);
			parameters.Add ("@p3",  SqlDbType.SmallInt, 2);
			parameters.Add ("@p4",  SqlDbType.Int, 4);
			parameters.Add ("@p5",  SqlDbType.Real, 4);
			parameters.Add ("@p6",  SqlDbType.Float, 8);
			parameters.Add ("@p7",  SqlDbType.Decimal, 12);
			parameters.Add ("@p8",  SqlDbType.Char, 14);
			parameters.Add ("@p9",  SqlDbType.NChar, 16);
			parameters.Add ("@p10", SqlDbType.VarChar, 17);
			parameters.Add ("@p11", SqlDbType.NVarChar, 19);
			parameters.Add ("@p12", SqlDbType.Text, 14);
			parameters.Add ("@p13", SqlDbType.NText, 16);
			parameters.Add ("@p14", SqlDbType.DateTime, 4);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = 15;
			parameters ["@p3"].Value = -22;
			parameters ["@p4"].Value = 1048000;
			parameters ["@p5"].Value = 3.141592;
			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = 123456789012.345;
			parameters ["@p7"].Precision = 15;
			parameters ["@p7"].Scale = 3;
			parameters ["@p8"].Value = "This is a char";
			parameters ["@p9"].Value = "This is an nchar";
			parameters ["@p10"].Value = "This is a varchar";
			parameters ["@p11"].Value = "This is an nvarchar";
			parameters ["@p12"].Value = "This is a text";
			parameters ["@p13"].Value = "This is an ntext";
			parameters ["@p14"].Value = DateTime.Now;

			insertCommand.ExecuteNonQuery ();
		}

		// execute a SQL SELECT Query using ExecuteReader() to retrieve
		// a IDataReader so we retrieve data
		static IDataReader SelectData (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// FIXME: System.Data classes need to handle NULLs
			//        this would be done by System.DBNull ?
			// FIXME: System.Data needs to handle more data types
			/*
			selectCommand.CommandText = 
				"select * " +
				"from mono_postgres_test";
			*/

			selectCommand.CommandText = 
				"select " +				
				"boolean_value, " +
				"byte_value, " +
				"int2_value, " +
				"int4_value, " +
				"float_value, " + 
				"double_value, " +
				"numeric_value, " +
				"char_value, " +
				"nchar_value, " +
				"varchar_value, " +
				"nvarchar_value, " +
				"text_value, " +
				"ntext_value, " +
				"datetime_value, " +
				"null_boolean_value, " +
				"null_byte_value, " +
				"null_int2_value, " +
				"null_int4_value, " +
				"null_float_value, " + 
				"null_double_value, " +
				"null_numeric_value, " +
				"null_char_value, " +
				"null_nchar_value, " +
				"null_varchar_value, " +
				"null_nvarchar_value, " +
				"null_text_value, " +
				"null_ntext_value, " +
				"null_datetime_value " +
				"from mono_sql_test";


			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		static IDataReader SelectDataUsingInsertCommand (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL INSERT Command, not a Query
			selectCommand.CommandText =
				"insert into mono_sql_test (" +
				"boolean_value, " +
				"byte_value, " +
				"int2_value, " +
				"int4_value, " +
				"float_value, " + 
				"double_value, " +
				"numeric_value, " +
				"char_value, " +
				"nchar_value, " +
				"varchar_value, " +
				"nvarchar_value, " +
				"text_value, " +
				"ntext_value, " +
				"datetime_value " +
				") values (" +
				"@p1, " +
				"@p2, " +
				"@p3, " +
				"@p4, " +
				"@p5, " +
				"@p6, " +
				"@p7, " +
				"@p8, " +
				"@p9, " +
				"@p10, " +
				"@p11, " +
				"@p12, " +
				"@p13, " +
				"@p14 " +
				")";

			SqlParameterCollection parameters = ((SqlCommand) selectCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit, 1);
			parameters.Add ("@p2",  SqlDbType.TinyInt, 1);
			parameters.Add ("@p3",  SqlDbType.SmallInt, 2);
			parameters.Add ("@p4",  SqlDbType.Int, 4);
			parameters.Add ("@p5",  SqlDbType.Real, 4);
			parameters.Add ("@p6",  SqlDbType.Float, 8);
			parameters.Add ("@p7",  SqlDbType.Decimal, 12);
			parameters.Add ("@p8",  SqlDbType.Char, 14);
			parameters.Add ("@p9",  SqlDbType.NChar, 16);
			parameters.Add ("@p10", SqlDbType.VarChar, 17);
			parameters.Add ("@p11", SqlDbType.NVarChar, 19);
			parameters.Add ("@p12", SqlDbType.Text, 14);
			parameters.Add ("@p13", SqlDbType.NText, 16);
			parameters.Add ("@p14", SqlDbType.DateTime, 4);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = 15;
			parameters ["@p3"].Value = -22;
			parameters ["@p4"].Value = 1048000;
			parameters ["@p5"].Value = 3.141592;
			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = 123456789012.345;
			parameters ["@p7"].Precision = 15;
			parameters ["@p7"].Scale = 3;
			parameters ["@p8"].Value = "This is a char";
			parameters ["@p9"].Value = "This is an nchar";
			parameters ["@p10"].Value = "This is a varchar";
			parameters ["@p11"].Value = "This is an nvarchar";
			parameters ["@p12"].Value = "This is a text";
			parameters ["@p13"].Value = "This is an ntext";
			parameters ["@p14"].Value = DateTime.Now;

			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command not (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		static IDataReader SelectDataUsingCommand (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL Command, not a Query
			selectCommand.CommandText = 
				"SET FMTONLY OFF";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}


		// execute an SQL UPDATE Command using ExecuteNonQuery()
		static void UpdateData (IDbConnection cnc) {
	
			IDbCommand updateCommand = cnc.CreateCommand();		
		
			updateCommand.CommandText = 
				"update mono_sql_test " +				
				"set " +
				"boolean_value    = @p1, " +
				"byte_value       = @p2, " +
				"int2_value       = @p3, " +
				"int4_value       = @p4, " +
				"char_value       = @p5, " +
				"nchar_value      = @p6, " +
				"varchar_value    = @p7, " +
				"nvarchar_value   = @p8, " +
				"text_value       = @p9, " +
				"ntext_value      = @p10 " +
				"where int2_value = @p11";

			SqlParameterCollection parameters = ((SqlCommand) updateCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit, 1);
			parameters.Add ("@p2",  SqlDbType.TinyInt, 1);
			parameters.Add ("@p3",  SqlDbType.SmallInt, 2);
			parameters.Add ("@p4",  SqlDbType.Int, 4);
			parameters.Add ("@p5",  SqlDbType.Char, 10);
			parameters.Add ("@p6",  SqlDbType.NChar, 10);
			parameters.Add ("@p7",  SqlDbType.VarChar, 14);
			parameters.Add ("@p8",  SqlDbType.NVarChar, 14);
			parameters.Add ("@p9",  SqlDbType.Text, 12);
			parameters.Add ("@p10", SqlDbType.NText, 12);
			parameters.Add ("@p11", SqlDbType.SmallInt, 2);

			parameters ["@p1"].Value = false;
			parameters ["@p2"].Value = 2;
			parameters ["@p3"].Value = 5;
			parameters ["@p4"].Value = 3;
			parameters ["@p5"].Value = "Mono.Data!";
			parameters ["@p6"].Value = "Mono.Data!";
			parameters ["@p7"].Value = "It was not me!";
			parameters ["@p8"].Value = "It was not me!";
			parameters ["@p9"].Value = "We got data!";
			parameters ["@p10"].Value = "We got data!";
			parameters ["@p11"].Value = -22;

			updateCommand.ExecuteNonQuery ();		
		}

		// used to do a min(), max(), count(), sum(), or avg()
		// execute SQL SELECT Query using ExecuteScalar
		static object SelectAggregate (IDbConnection cnc, String agg) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			object data;

			Console.WriteLine("Aggregate: " + agg);

			selectCommand.CommandType = CommandType.Text;
			selectCommand.CommandText = 
				"select " + agg +
				"from mono_sql_test";

			data = selectCommand.ExecuteScalar ();

			Console.WriteLine("Agg Result: " + data);

			return data;
		}

		// used internally by ReadData() to read each result set
		static void ReadResult(IDataReader rdr, DataTable dt) {
                        			
			// number of columns in the table
			Console.WriteLine("   Total Columns: " +
				dt.Rows.Count);

			// display the schema
			foreach (DataRow schemaRow in dt.Rows) {
				foreach (DataColumn schemaCol in dt.Columns)
					Console.WriteLine(schemaCol.ColumnName + 
						" = " + 
						schemaRow[schemaCol]);
				Console.WriteLine();
			}

			int nRows = 0;
			int c = 0;
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
			Console.WriteLine("   Total Rows Retrieved: " + 
				nRows);	
		}

		// Used to read data from IDataReader after calling IDbCommand:ExecuteReader()
		static void ReadData(IDataReader rdr) {

			int results = 0;
			if(rdr == null) {
		
				Console.WriteLine("IDataReader has a Null Reference.");
			}
			else {
				do {
					DataTable dt = rdr.GetSchemaTable();
					if(rdr.RecordsAffected != -1) {
						// Results for 
						// SQL INSERT, UPDATE, DELETE Commands 
						// have RecordsAffected >= 0
						Console.WriteLine("Result is from a SQL Command (INSERT,UPDATE,DELETE).  Records Affected: " + rdr.RecordsAffected);
					}
					else if(dt == null)
						// Results for
						// SQL Commands not INSERT, UPDATE, nor DELETE
						// have RecordsAffected == -1
						// and GetSchemaTable() returns a null reference
						Console.WriteLine("Result is from a SQL Command not (INSERT,UPDATE,DELETE).   Records Affected: " + rdr.RecordsAffected);
					else {
						// Results for
						// SQL SELECT Queries
						// have RecordsAffected = -1
						// and GetSchemaTable() returns a reference to a DataTable
						Console.WriteLine("Result is from a SELECT SQL Query.  Records Affected: " + rdr.RecordsAffected);
		
						results++;
						Console.WriteLine("Result Set " + results + "...");

						ReadResult(rdr, dt);
					}

				} while(rdr.NextResult());
				Console.WriteLine("Total Result sets: " + results);
			
				rdr.Close();
			}
		}
		
		/* Sql provider tests */
		static void DoSqlTest (IDbConnection cnc) {

			IDataReader reader;
			Object oDataValue;

			Console.WriteLine ("\tSql provider specific tests...\n");

			/* Drops the mono_sql_test table. */
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
				// FIXME: still having a problem with avg()
				//        because it returns a decimal.
				//        It may have something to do
				//        with culture not being set
				//        properly.
				//SelectAggregate (cnc, "avg(int4_value)");
				SelectAggregate (cnc, "min(varchar_value)");
				SelectAggregate (cnc, "max(int4_value)");
				SelectAggregate (cnc, "sum(int4_value)");

				/* Select values */
				Console.WriteLine ("\t\tSelect values from the database: ");
				reader = SelectData (cnc);
				ReadData(reader);

				/* SQL Command via ExecuteReader/SqlDataReader */
				/* Command is not INSERT, UPDATE, or DELETE */
				Console.WriteLine("\t\tCall ExecuteReader with a SQL Command. (Not INSERT,UPDATE,DELETE).");
				reader = SelectDataUsingCommand(cnc);
				ReadData(reader);

				/* SQL Command via ExecuteReader/SqlDataReader */
				/* Command is INSERT, UPDATE, or DELETE */
				Console.WriteLine("\t\tCall ExecuteReader with a SQL Command. (Is INSERT,UPDATE,DELETE).");
				reader = SelectDataUsingInsertCommand(cnc);
				ReadData(reader);

				// Call a Stored Procedure named Version()
				Console.WriteLine("\t\tCalling stored procedure sp_server_info()");
				object obj = CallStoredProcedure(cnc);
				Console.WriteLine("Result: " + obj);

				Console.WriteLine("Database Server Version: " + 
					((SqlConnection)cnc).ServerVersion);

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
		static void Main(string[] args) {
			SqlConnection cnc = new SqlConnection ();

			/*
			string connectionString = 
				"host=hostname;" +
				"dbname=database;" +
				"user=userid;" +
				"password=password";
			*/

			string connectionString = 
				"Server=localhost;" + 
				"Database=test;" +
				"User ID=sql;" +
				"Password=";

			cnc.ConnectionString =  connectionString;

			cnc.Open();
			DoSqlTest(cnc);
			cnc.Close();
		}
	}
}
