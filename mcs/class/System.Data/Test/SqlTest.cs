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
using System.Text;

namespace Test.Mono.Data.SqlClient {

	class SqlTest {

		// execute SQL CREATE TABLE Command using ExecuteNonQuery()
		static void CreateTable (IDbConnection cnc) {
						
			IDbCommand createCommand = cnc.CreateCommand();
	
			createCommand.CommandText = 
				"create table mono_sql_test (" +
				"bit_value bit, " +
				"binary_value binary (8), " +
				"char_value char(50), " +
				"datetime_value datetime, " +
				"decimal_value decimal(15, 3), " +
				"float_value float, " +
				"image_value image, " +
				"int_value int, " +
				"money_value money, " +
				"nchar_value nchar(50), " +
				"ntext_value ntext, " +
				"nvarchar_value nvarchar(20), " +
				"real_value real, " + 
				"smalldatetime_value smalldatetime, " +
				"smallint_value smallint, " +
				"smallmoney_value smallmoney, " +
				"text_value text, " +
				"timestamp_value timestamp, " +
				"tinyint_value tinyint, " +
				"uniqueidentifier_value uniqueidentifier, "  +
				"varbinary_value varbinary (8), " +
				"varchar_value varchar(20), " +
				"null_bit_value bit, " +
				"null_binary_value binary (8), " +
				"null_char_value char(50), " +
				"null_datetime_value datetime, " +
				"null_decimal_value decimal(15, 3), " +
				"null_float_value float, " +
				"null_image_value image, " +
				"null_int_value int, " +
				"null_money_value int, " +
				"null_nchar_value nchar(50), " +
				"null_ntext_value ntext, " +
				"null_nvarchar_value nvarchar(20), " +
				"null_real_value real, " + 
				"null_smalldatetime_value smalldatetime, " +
				"null_smallint_value smallint, " +
				"null_smallmoney_value int, " +
				"null_text_value text, " +
				"null_tinyint_value tinyint, " +
				"null_uniqueidentifier_value uniqueidentifier, "  +
				"null_varbinary_value varbinary (8), " +
				"null_varchar_value varchar(20) " +
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
				"bit_value, " +
				"binary_value, " +
				"char_value, " +
				"datetime_value, " +
				"decimal_value, " +
				"float_value, " +
				"image_value, " +
				"int_value, " +
				"money_value, " +
				"nchar_value, " +
				"ntext_value, " +
				"nvarchar_value, " +
				"real_value, " + 
				"smalldatetime_value, " +
				"smallint_value, " +
				"smallmoney_value, " +
				"text_value, " +
				"tinyint_value, " +
				"uniqueidentifier_value, " +
				"varbinary_value, " +
				"varchar_value " +
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
				"@p14, " +
				"@p15, " +
				"@p16, " +
				"@p17, " +
				"@p18, " +
				"@p19, " +
				"@p20, " +
				"@p21 " +
				")";

			SqlParameterCollection parameters = ((SqlCommand) insertCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit);
			parameters.Add ("@p2",  SqlDbType.Binary, 8);
			parameters.Add ("@p3",  SqlDbType.Char, 14);
			parameters.Add ("@p4",  SqlDbType.DateTime);
			parameters.Add ("@p5",  SqlDbType.Decimal);
			parameters.Add ("@p6",  SqlDbType.Float);
			parameters.Add ("@p7",  SqlDbType.Image);
			parameters.Add ("@p8",  SqlDbType.Int);
			parameters.Add ("@p9",  SqlDbType.Money);
			parameters.Add ("@p10", SqlDbType.NChar, 16);
			parameters.Add ("@p11", SqlDbType.NText);
			parameters.Add ("@p12", SqlDbType.NVarChar, 19);
			parameters.Add ("@p13", SqlDbType.Real);
			parameters.Add ("@p14", SqlDbType.SmallDateTime);
			parameters.Add ("@p15", SqlDbType.SmallInt);
			parameters.Add ("@p16", SqlDbType.SmallMoney);
			parameters.Add ("@p17", SqlDbType.Text);
			parameters.Add ("@p18", SqlDbType.TinyInt);
			parameters.Add ("@p19", SqlDbType.UniqueIdentifier);
			parameters.Add ("@p20", SqlDbType.VarBinary, 8);
			parameters.Add ("@p21", SqlDbType.VarChar, 17);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = new byte[2] {0x12,0x34};
			parameters ["@p3"].Value = "This is a char";
			parameters ["@p4"].Value = new DateTime (1959, 7, 17); // My mom's birthday!

			parameters ["@p5"].Value = 123456789012.345;
			parameters ["@p5"].Precision = 15;
			parameters ["@p5"].Scale = 3;

			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = new byte[4] {0xde, 0xad, 0xbe, 0xef};
			parameters ["@p8"].Value = 1048000;
			parameters ["@p9"].Value = 31337.456;
			parameters ["@p10"].Value = "This is an nchar";
			parameters ["@p11"].Value = "This is an ntext";
			parameters ["@p12"].Value = "This is an nvarchar";
			parameters ["@p13"].Value = 3.141592;
			parameters ["@p14"].Value = new DateTime (1976, 10, 31); // My birthday!
			parameters ["@p15"].Value = -22;
			parameters ["@p16"].Value = 31337.456;
			parameters ["@p17"].Value = "This is a text";
			parameters ["@p18"].Value = 15;
			parameters ["@p19"].Value = Guid.NewGuid ();
			parameters ["@p20"].Value = new byte[2] {0x56,0x78};
			parameters ["@p21"].Value = "This is a varchar";

			insertCommand.ExecuteNonQuery ();
		}

		// execute SQL INSERT Command using ExecuteNonQuery()
		static void InsertEdgeCaseData (IDbConnection cnc) {		

			IDbCommand insertCommand = cnc.CreateCommand();
		
			insertCommand.CommandText =
				"insert into mono_sql_test (" +
				"varbinary_value " +
				") values (" +
				"@p20 " +
				")";

			SqlParameterCollection parameters = ((SqlCommand) insertCommand).Parameters;

			parameters.Add ("@p20", SqlDbType.VarBinary, 8);

			parameters ["@p20"].Value = new byte[0] {};

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
				"bit_value, " +
				"binary_value, " +
				"char_value, " +
				"datetime_value, " +
				"decimal_value, " +
				"float_value, " +
				"image_value, " +
				"int_value, " +
				"money_value, " +
				"nchar_value, " +
				"ntext_value, " +
				"nvarchar_value, " +
				"real_value, " + 
				"smalldatetime_value, " +
				"smallint_value, " +
				"smallmoney_value, " +
				"text_value, " +
				"timestamp_value, " +
				"tinyint_value, " +
				"uniqueidentifier_value, "  +
				"varbinary_value, " +
				"varchar_value, " +
				"null_bit_value, " +
				"null_binary_value, " +
				"null_char_value, " +
				"null_datetime_value, " +
				"null_decimal_value, " +
				"null_float_value, " +
				"null_image_value, " +
				"null_int_value, " +
				"null_money_value, " +
				"null_nchar_value, " +
				"null_ntext_value, " +
				"null_nvarchar_value, " +
				"null_real_value, " + 
				"null_smalldatetime_value, " +
				"null_smallint_value, " +
				"null_smallmoney_value, " +
				"null_text_value, " +
				"null_tinyint_value, " +
				"null_uniqueidentifier_value, "  +
				"null_varbinary_value, " +
				"null_varchar_value " +
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
				"bit_value, " +
				"binary_value, " +
				"char_value, " +
				"datetime_value, " +
				"decimal_value, " +
				"float_value, " +
				"image_value, " +
				"int_value, " +
				"money_value, " +
				"nchar_value, " +
				"ntext_value, " +
				"nvarchar_value, " +
				"real_value, " + 
				"smalldatetime_value, " +
				"smallint_value, " +
				"smallmoney_value, " +
				"text_value, " +
				"tinyint_value, " +
				"uniqueidentifier_value, "  +
				"varbinary_value, " +
				"varchar_value " +
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
				"@p14, " +
				"@p15, " +
				"@p16, " +
				"@p17, " +
				"@p18, " +
				"@p19, " +
				"@p20, " +
				"@p21 " +
				")";

			SqlParameterCollection parameters = ((SqlCommand) selectCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit);
			parameters.Add ("@p2",  SqlDbType.Binary, 8);
			parameters.Add ("@p3",  SqlDbType.Char, 14);
			parameters.Add ("@p4",  SqlDbType.DateTime);
			parameters.Add ("@p5",  SqlDbType.Decimal);
			parameters.Add ("@p6",  SqlDbType.Float);
			parameters.Add ("@p7",  SqlDbType.Image);
			parameters.Add ("@p8",  SqlDbType.Int);
			parameters.Add ("@p9",  SqlDbType.Money);
			parameters.Add ("@p10", SqlDbType.NChar, 16);
			parameters.Add ("@p11", SqlDbType.NText);
			parameters.Add ("@p12", SqlDbType.NVarChar, 19);
			parameters.Add ("@p13", SqlDbType.Real);
			parameters.Add ("@p14", SqlDbType.SmallDateTime);
			parameters.Add ("@p15", SqlDbType.SmallInt);
			parameters.Add ("@p16", SqlDbType.SmallMoney);
			parameters.Add ("@p17", SqlDbType.Text);
			parameters.Add ("@p18", SqlDbType.TinyInt);
			parameters.Add ("@p19", SqlDbType.UniqueIdentifier);
			parameters.Add ("@p20", SqlDbType.VarBinary, 8);
			parameters.Add ("@p21", SqlDbType.VarChar, 17);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = new byte[2] {0x9a,0xbc};
			parameters ["@p3"].Value = "This is a char";
			parameters ["@p4"].Value = DateTime.Now;

			parameters ["@p5"].Value = 123456789012.345;
			parameters ["@p5"].Precision = 15;
			parameters ["@p5"].Scale = 3;

			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = new byte[4] {0xfe, 0xeb, 0xda, 0xed};
			parameters ["@p8"].Value = 1048000;
			parameters ["@p9"].Value = 31337.456;
			parameters ["@p10"].Value = "This is an nchar";
			parameters ["@p11"].Value = "This is an ntext";
			parameters ["@p12"].Value = "This is an nvarchar";
			parameters ["@p13"].Value = 3.141592;
			parameters ["@p14"].Value = new DateTime (1978, 6, 30); // My brother's birthday!
			parameters ["@p15"].Value = -22;
			parameters ["@p16"].Value = 31337.456;
			parameters ["@p17"].Value = "This is a text";
			parameters ["@p18"].Value = 15;
			parameters ["@p19"].Value = Guid.NewGuid ();
			parameters ["@p20"].Value = new byte[2] {0xde, 0xef};
			parameters ["@p21"].Value = "This is a varchar";

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
				"bit_value              = @p1, " +
				"tinyint_value          = @p2, " +
				"smallint_value         = @p3, " +
				"int_value              = @p4, " +
				"char_value             = @p5, " +
				"nchar_value            = @p6, " +
				"varchar_value          = @p7, " +
				"nvarchar_value         = @p8, " +
				"text_value             = @p9, " +
				"ntext_value            = @p10, " +
				"uniqueidentifier_value = @p11, " +
				"binary_value           = @p12, " +
				"varbinary_value        = @p13  " +
				"where smallint_value   = @p14";

			SqlParameterCollection parameters = ((SqlCommand) updateCommand).Parameters;

			parameters.Add ("@p1",  SqlDbType.Bit);
			parameters.Add ("@p2",  SqlDbType.TinyInt);
			parameters.Add ("@p3",  SqlDbType.SmallInt);
			parameters.Add ("@p4",  SqlDbType.Int);
			parameters.Add ("@p5",  SqlDbType.Char, 10);
			parameters.Add ("@p6",  SqlDbType.NChar, 10);
			parameters.Add ("@p7",  SqlDbType.VarChar, 14);
			parameters.Add ("@p8",  SqlDbType.NVarChar, 14);
			parameters.Add ("@p9",  SqlDbType.Text);
			parameters.Add ("@p10", SqlDbType.NText);
			parameters.Add ("@p11", SqlDbType.UniqueIdentifier);
			parameters.Add ("@p12", SqlDbType.Binary, 8);
			parameters.Add ("@p13", SqlDbType.VarBinary, 8);
			parameters.Add ("@p14", SqlDbType.SmallInt);

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
			parameters ["@p11"].Value = Guid.NewGuid ();
			parameters ["@p12"].Value = new byte[2] {0x57,0x3e};
			parameters ["@p13"].Value = new byte[2] {0xa2,0xf7};
			parameters ["@p14"].Value = -22;

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
					if (rdr.IsDBNull(c) == true)
						dataValue = " is NULL";
					else if ((Type) dr["DataType"] == typeof (byte[])) 
						dataValue = 
							": 0x" + 
							BitConverter.ToString ((byte[]) rdr.GetValue (c)).Replace ("-", "").ToLower ();
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

				/* Inserts edge case values */
				Console.WriteLine ("\t\tInsert values that require special coding: ");
				InsertEdgeCaseData (cnc);
				Console.WriteLine ("OK");			

				/* Select aggregates */
				SelectAggregate (cnc, "count(*)");
				// FIXME: still having a problem with avg()
				//        because it returns a decimal.
				//        It may have something to do
				//        with culture not being set
				//        properly.
				//SelectAggregate (cnc, "avg(int_value)");
				SelectAggregate (cnc, "min(varchar_value)");
				SelectAggregate (cnc, "max(int_value)");
				SelectAggregate (cnc, "sum(int_value)");

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
				//DropTable (cnc);
				Console.WriteLine("OK");
			}
			catch(Exception e) {
				Console.WriteLine("Exception caught: " + e);
			}
		}

		[STAThread]
		static void Main(string[] args) {
			string connectionString = "";
						
			if(args.Length == 3 || args.Length == 4) {
				if(args.Length == 3) {
					connectionString = String.Format(
						"Server={0};" + 
						"Database={1};" +
						"User ID={2};",
						args[0], args[1], args[2]);
				}
				else if(args.Length == 4) {
					connectionString = String.Format(
						"Server={0};" + 
						"Database={1};" +
						"User ID={2};" +
						"Password={3}",
						args[0], args[1], args[2], args[3]);
				}
			}
			else {
				Console.WriteLine("Usage: mono SqlTest.exe sql_server database user_id password");
				return;
			}

			SqlConnection cnc = new SqlConnection ();
			cnc.ConnectionString =  connectionString;

			cnc.Open();
			DoSqlTest(cnc);
			cnc.Close();
		}
	}
}
