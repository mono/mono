/* MySqlTest.cs - based on PostgresTest.cs which is based 
 *                on postgres-test.c in libgda
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

//
// To compile on Windows using Cygwin, you will need to do:
// mono C:/cygwin/home/danmorg/mono/install/bin/mcs.exe MySqlTest.cs -r System.Data.dll -r Mono.Data.MySql.dll
//

using System;
using System.Data;
using System.Data.Common;
using Mono.Data.MySql;

namespace Test.Mono.Data.MySql {

	public class MySqlTest {

		// execute SQL CREATE TABLE Command using ExecuteNonQuery()
		public static void CreateTable (IDbConnection cnc) {
						
			IDbCommand createCommand = cnc.CreateCommand();
	
			createCommand.CommandText = 
				"CREATE TABLE mono_mysql_test (" +
				"tinyint_value TINYINT," +
				"smallint_value SMALLINT," +
				"mediumint_value MEDIUMINT," +
				"int_value INT," +
				"integer_value INTEGER," +
				"bigint_value BIGINT," + 
				"real_value REAL," + 
				"double_value DOUBLE," + 
				"float_value FLOAT," +
				"decimal_value DECIMAL(8,2)," + 
				"numeric_value NUMERIC(15,2)," + 
				"char_value CHAR(2)," + 
				"varchar_value VARCHAR(5)," + 
				"date_value DATE," + 
				"time_value TIME," +
				"timestamp_value TIMESTAMP," +
				"datetime_value DATETIME," +
				"tinyblob_value TINYBLOB," +
				"blob_value BLOB," +
				"mediumblob_value MEDIUMBLOB," +
				"longblob_value LONGBLOB," +
				"tinytext_value TINYTEXT," +
				"text_value TEXT," +
				"mediumtext_value MEDIUMTEXT," +
				"longtext_value LONGTEXT," +
				"enum_value ENUM('dog','cat','bird','fish')," +
				"set_value SET('value1','value2','value3','value4'), " +
				"null_tinyint_value TINYINT," +
				"null_smallint_value SMALLINT," +
				"null_mediumint_value MEDIUMINT," +
				"null_int_value INT," +
				"null_integer_value INTEGER," +
				"null_bigint_value BIGINT," + 
				"null_real_value REAL," + 
				"null_double_value DOUBLE," + 
				"null_float_value FLOAT," +
				"null_decimal_value DECIMAL(8,2)," + 
				"null_numeric_value NUMERIC(15,2)," + 
				"null_char_value CHAR(2)," + 
				"null_varchar_value VARCHAR(5)," + 
				"null_date_value DATE," + 
				"null_time_value TIME," +
				"null_timestamp_value TIMESTAMP," +
				"null_datetime_value DATETIME," +
				"null_tinyblob_value TINYBLOB," +
				"null_blob_value BLOB," +
				"null_mediumblob_value MEDIUMBLOB," +
				"null_longblob_value LONGBLOB," +
				"null_tinytext_value TINYTEXT," +
				"null_text_value TEXT," +
				"null_mediumtext_value MEDIUMTEXT," +
				"null_longtext_value LONGTEXT," +
				"null_enum_value ENUM('dog','cat','bird','fish')," +
				"null_set_value SET('value1','value2','value3','value4') " +
				") ";
	
			int rowsAffected;
			rowsAffected = createCommand.ExecuteNonQuery ();
			Console.WriteLine("Rows Affected: " + rowsAffected);
		}

		// execute SQL DROP TABLE Command using ExecuteNonQuery
		public static void DropTable (IDbConnection cnc) {
				 
			IDbCommand dropCommand = cnc.CreateCommand ();

			dropCommand.CommandText =
				"DROP TABLE mono_mysql_test";
				
			int rowsAffected;
			rowsAffected = dropCommand.ExecuteNonQuery ();
			Console.WriteLine("Rows Affected: " + rowsAffected);

		}

		// execute stored procedure using ExecuteScalar()
		public static object CallStoredProcedure (IDbConnection cnc) {
				 
			IDbCommand callStoredProcCommand = cnc.CreateCommand ();
			object data;

			callStoredProcCommand.CommandType = 
				CommandType.StoredProcedure;
			callStoredProcCommand.CommandText =
				"version";
							
			data = callStoredProcCommand.ExecuteScalar ();

			return data;
		}

		// execute SQL INSERT Command using ExecuteNonQuery()
		public static void InsertData (IDbConnection cnc) {		

			IDbCommand insertCommand = cnc.CreateCommand();
		
			insertCommand.CommandText =
				"INSERT INTO mono_mysql_test (" +
				"tinyint_value," +
				"smallint_value," +
				"mediumint_value," +
				"int_value," +
				"integer_value," +
				"bigint_value," + 
				"real_value," + 
				"double_value," + 
				"float_value," +
				"decimal_value," + 
				"numeric_value," + 
				"char_value," + 
				"varchar_value," + 
				"date_value," + 
				"time_value," +
				"timestamp_value," +
				"datetime_value," +
				"tinyblob_value," +
				"blob_value," +
				"mediumblob_value," +
				"longblob_value," +
				"tinytext_value," +
				"text_value," +
				"mediumtext_value," +
				"longtext_value," +
				"enum_value," +
				"set_value " +
				") VALUES (" +
				"1,2,3,4,5,6, " +
				"1.1, 2.2, 3.3, " +
				"10.10, 11.11, " +
				"'AB','mono'," +
				"'2002-12-31', '11:15:07'," + 
				"'20021231111507', '2002-12-31 11:15:07'," +
				"'fe','fi','fo','thumb','i','smell','some','food'," +
				"'cat', 'value2,value3' " +
				")";

			int rowsAffected;
			rowsAffected = insertCommand.ExecuteNonQuery ();
			Console.WriteLine("Rows Affected: " + rowsAffected);
		}

		// execute a SQL SELECT Query using ExecuteReader() to retrieve
		// a IDataReader so we retrieve data
		public static IDataReader SelectData (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// FIXME: System.Data classes need to handle NULLs
			//        this would be done by System.DBNull ?
			// FIXME: System.Data needs to handle more data types
			
			selectCommand.CommandText = 
				"SELECT * " +
				"FROM mono_mysql_test";
			
			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		public static IDataReader SelectDataUsingInsertCommand (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL INSERT Command, not a Query
			selectCommand.CommandText =
				"INSERT INTO mono_mysql_test (" +
				"tinyint_value," +
				"smallint_value," +
				"mediumint_value," +
				"int_value," +
				"integer_value," +
				"bigint_value," + 
				"real_value," + 
				"double_value," + 
				"float_value," +
				"decimal_value," + 
				"numeric_value," + 
				"char_value," + 
				"varchar_value," + 
				"date_value," + 
				"time_value," +
				"timestamp_value," +
				"datetime_value," +
				"tinyblob_value," +
				"blob_value," +
				"mediumblob_value," +
				"longblob_value," +
				"tinytext_value," +
				"text_value," +
				"mediumtext_value," +
				"longtext_value," +
				"enum_value," +
				"set_value " +
				") VALUES (" +
				"91,92,93,94,95,96, " +
				"91.1, 92.2, 93.3, " +
				"910.10, 911.11, " +
				"'CD','mcs'," +
				"'2003-11-23', '10:24:45'," + 
				"'20031123122445', '2003-11-23 10:24:45'," +
				"'ack','bleh','heh','pop','me','nope','yeah','fun'," +
				"'dog', 'value1,value3,value4' " +
				")";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command not (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		public static IDataReader SelectDataUsingCommand (IDbConnection cnc) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL Command, not a Query
			selectCommand.CommandText = 
				"SET AUTOCOMMIT=0";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}


		// execute an SQL UPDATE Command using ExecuteNonQuery()
		public static void UpdateData (IDbConnection cnc) {
	
			IDbCommand updateCommand = cnc.CreateCommand();		
		
			updateCommand.CommandText = 
				"UPDATE mono_mysql_test " +				
				"SET " +
				"int_value = 777 " +
				"WHERE char_value = 'AB'";

			updateCommand.ExecuteNonQuery ();		
		}

		// used to do a min(), max(), count(), sum(), or avg()
		// execute SQL SELECT Query using ExecuteScalar
		public static object SelectAggregate (IDbConnection cnc, String agg) {
	
			IDbCommand selectCommand = cnc.CreateCommand();
			object data;

			Console.WriteLine("Aggregate: " + agg);

			selectCommand.CommandType = CommandType.Text;
			selectCommand.CommandText = 
				"SELECT " + agg +
				"FROM mono_mysql_test";

			data = selectCommand.ExecuteScalar ();

			Console.WriteLine("Agg Result: " + data);

			return data;
		}

		// used internally by ReadData() to read each result set
		public static void ReadResult(IDataReader rdr, DataTable dt) {
                        			
			// number of columns in the table
			Console.WriteLine("   Total Columns: " +
				dt.Rows.Count);

			// display the schema
			string colName;
			string colValue;
			foreach (DataRow schemaRow in dt.Rows) {
				foreach (DataColumn schemaCol in dt.Columns) {
					colName = schemaCol.ColumnName;
					colValue = (schemaRow[schemaCol]).ToString();
					Console.WriteLine(colName + " = " + colValue);
				}
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
					metadataValue = 
						"    Col " + 
						c + ": " + 
						rdr.GetName(c);
						
					// column data
					if(rdr.IsDBNull(c) == true)
						dataValue = " is NULL";
					else
						dataValue = 
							": " + 
							rdr.GetValue(c).ToString();
					
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
		public static void ReadData(IDataReader rdr) {

			int results = 0;
			if(rdr == null) {
		
				Console.WriteLine("IDataReader has a Null Reference.");
			}
			else {
				do {
					results++;
					if(rdr.FieldCount > 0) {
						// Results for
						// SQL SELECT Queries
						// have RecordsAffected = -1
						// and GetSchemaTable() returns a reference to a DataTable
						DataTable dt = rdr.GetSchemaTable();
						Console.WriteLine("Result is from a SELECT SQL Query.  Records Affected: " + rdr.RecordsAffected);
						
						Console.WriteLine("Result Set " + results + "...");

						ReadResult(rdr, dt);
					}
					if(rdr.RecordsAffected >= 0) {
						// Results for 
						// SQL INSERT, UPDATE, DELETE Commands 
						// have RecordsAffected >= 0
						Console.WriteLine("Result is from a SQL Command (INSERT,UPDATE,DELETE).  Records Affected: " + rdr.RecordsAffected);
					}
					else {
						// Results for
						// SQL Commands not INSERT, UPDATE, nor DELETE
						// have RecordsAffected == -1
						// and GetSchemaTable() returns a null reference
						Console.WriteLine("Result is from a SQL Command not (INSERT,UPDATE,DELETE).   Records Affected: " + rdr.RecordsAffected);
					}
				} while(rdr.NextResult());
				Console.WriteLine("Total Result sets: " + results);
			
				rdr.Close();
			}
		}
		
		/* MySQL provider tests */
		public static void PerformTest (IDbConnection cnc) {

			IDataReader reader;
			Object oDataValue;

			Console.WriteLine ("\tMySQL provider specific tests...\n");

			/* Drops the mono_mysql_test table. */
			
			Console.WriteLine ("\t\tDrop table: ");
			try {
				DropTable (cnc);
				Console.WriteLine ("OK");
			}
			catch (MySqlException e) {
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
				SelectAggregate (cnc, "avg(int_value)");
				SelectAggregate (cnc, "min(char_value)");
				SelectAggregate (cnc, "max(integer_value)");
				SelectAggregate (cnc, "sum(double_value)");

				/* Select values */
				Console.WriteLine ("\t\tSelect values from the database: ");
				reader = SelectData (cnc);
				ReadData(reader);

				/* SQL Command via ExecuteReader/MySqlDataReader */
				/* Command is not INSERT, UPDATE, or DELETE */
				Console.WriteLine("\t\tCall ExecuteReader with a SQL Command. (Not INSERT,UPDATE,DELETE).");
				reader = SelectDataUsingCommand(cnc);
				ReadData(reader);

				/* SQL Command via ExecuteReader/MySqlDataReader */
				/* Command is INSERT, UPDATE, or DELETE */
				Console.WriteLine("\t\tCall ExecuteReader with a SQL Command. (Is INSERT,UPDATE,DELETE).");
				reader = SelectDataUsingInsertCommand(cnc);
				ReadData(reader);

				// Call a Stored Procedure named Version()
				Console.WriteLine("\t\tCalling stored procedure version()");
				object obj = CallStoredProcedure(cnc);
				Console.WriteLine("Result: " + obj);

				Console.WriteLine("Database Server Version: " + 
					((MySqlConnection)cnc).ServerVersion);

				/* Clean up */
				//Console.WriteLine ("Clean up...");
				//Console.WriteLine ("\t\tDrop table...");
				//DropTable (cnc);
				//Console.WriteLine("OK");
			}
			catch(Exception e) {
				Console.WriteLine("Exception caught: " + e);
			}
		}

		[STAThread]
		public static void Main(string[] args) {

			MySqlConnection dbconn = new MySqlConnection ();
			
			// ConnectionString can be:
			//   "Server=localhost;Database=test;User ID=someuser;Password=somepass"
			// or it could be:
			//   "host=localhost;dbname=test;user=someuser;passwd=somepass"
			string connectionString = 
				"dbname=test;";
			dbconn.ConnectionString =  connectionString;

			dbconn.Open();
			PerformTest(dbconn);
			dbconn.Close();
		}
	}
}
