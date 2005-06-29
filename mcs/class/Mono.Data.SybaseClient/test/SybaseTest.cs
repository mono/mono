/* SybaseTest.cs - Tests for Sybase ASE
 * based on SqlTest.cs
 * which is based on the PostgresTest.cs
 * 
 * Copyright (C) 2002 Gonzalo Paniagua Javier
 * Copyright (C) 2002,2005 Daniel Morgan
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
using System.Text;
using Mono.Data.SybaseClient;

namespace Test.Mono.Data.SybaseClient 
{
	class SybaseTest 
	{
		// execute SQL CREATE TABLE Command using ExecuteNonQuery()
		static void CreateTable (IDbConnection cnc) 
		{				
			IDbCommand createCommand = cnc.CreateCommand();
	
			createCommand.CommandText = 
				"CREATE TABLE mono_sybase_test (" +
				" bit_value bit not null, " +
				" binary_value binary(8) not null, " +
				" char_value char(50) not null, " +
				" datetime_value datetime not null, " +
				" decimal_value decimal(15,3) not null, " +
				" float_value float not null, " +
				" int_value int not null, " +
				" money_value money not null, " +
				" nchar_value nchar(50) not null, " +
				" nvarchar_value nvarchar(20) not null, " +
				" real_value real not null, " + 
				" smalldatetime_value smalldatetime not null, " +
				" smallint_value smallint not null, " +
				" smallmoney_value smallmoney not null, " +
				" timestamp_value timestamp not null, " +
				" tinyint_value tinyint not null, " +
				" varbinary_value varbinary (8) not null, " +
				" varchar_value varchar(20) not null, " +
				" null_binary_value binary(8) null, " +
				" null_char_value char(50) null, " +
				" null_datetime_value datetime null, " +
				" null_decimal_value decimal(15,3) null, " +
				" null_float_value float null, " +
				" null_int_value int null, " +
				" null_money_value int null, " +
				" null_nchar_value nchar(50) null, " +
				" null_nvarchar_value nvarchar(20) null, " +
				" null_real_value real null, " + 
				" null_smalldatetime_value smalldatetime null, " +
				" null_smallint_value smallint null, " +
				" null_smallmoney_value int null, " +
				" null_tinyint_value tinyint null, " +
				" null_varbinary_value varbinary (8) null, " +
				" null_varchar_value varchar(20) null " +
				" )";			
	
			createCommand.ExecuteNonQuery ();
		}

		// execute SQL DROP TABLE Command using ExecuteNonQuery
		static void DropTable (IDbConnection cnc) 
		{		 
			IDbCommand dropCommand = cnc.CreateCommand ();

			dropCommand.CommandText =
				"drop table mono_sybase_test";
							
			try {
				dropCommand.ExecuteNonQuery ();
			} catch (SybaseException e) {
				Console.WriteLine ("SybaseException caught: " + e.Message);
			}
		}

		// execute stored procedure using ExecuteScalar()
		static object CallStoredProcedure (IDbConnection cnc) 
		{		 
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
		static void InsertData (IDbConnection cnc) 
		{		
			IDbCommand insertCommand = cnc.CreateCommand();
		
			insertCommand.CommandText =
				"INSERT INTO mono_sybase_test (" +
				" bit_value, " +
				" binary_value, " +
				" char_value, " +
				" datetime_value, " +
				" decimal_value, " +
				" float_value, " +
				" int_value, " +
				" money_value, " +
				" nchar_value, " +
				" nvarchar_value, " +
				" real_value, " + 
				" smalldatetime_value, " +
				" smallint_value, " +
				" smallmoney_value, " +
				" tinyint_value, " +
				" varbinary_value, " +
				" varchar_value " +
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
				"@p17 " +
				")";

			SybaseParameterCollection parameters = ((SybaseCommand) insertCommand).Parameters;

			parameters.Add ("@p1",  SybaseType.Bit);
			parameters.Add ("@p2",  SybaseType.Binary, 8);
			parameters.Add ("@p3",  SybaseType.Char, 14);
			parameters.Add ("@p4",  SybaseType.DateTime);
			parameters.Add ("@p5",  SybaseType.Decimal);
			parameters.Add ("@p6",  SybaseType.Float);
			parameters.Add ("@p7",  SybaseType.Int);
			parameters.Add ("@p8",  SybaseType.Money);
			parameters.Add ("@p9", SybaseType.NChar, 16);
			parameters.Add ("@p10", SybaseType.NVarChar, 19);
			parameters.Add ("@p11", SybaseType.Real);
			parameters.Add ("@p12", SybaseType.SmallDateTime);
			parameters.Add ("@p13", SybaseType.SmallInt);
			parameters.Add ("@p14", SybaseType.SmallMoney);
			parameters.Add ("@p15", SybaseType.TinyInt);
			parameters.Add ("@p16", SybaseType.VarBinary, 8);
			parameters.Add ("@p17", SybaseType.VarChar, 17);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = new byte[2] {0x12,0x34};
			parameters ["@p3"].Value = "This is a char";
			parameters ["@p4"].Value = new DateTime (1959, 7, 17); // My mom's birthday!

			parameters ["@p5"].Value = 123456789012.345;
			parameters ["@p5"].Precision = 15;
			parameters ["@p5"].Scale = 3;

			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = 1048000;
			parameters ["@p8"].Value = 31337.456;
			parameters ["@p9"].Value = "This is an nchar";
			parameters ["@p10"].Value = "This is an nvarchar";
			parameters ["@p11"].Value = 3.141592;
			parameters ["@p12"].Value = new DateTime (1976, 10, 31); // My birthday!
			parameters ["@p13"].Value = -22;
			parameters ["@p14"].Value = 31337.456;
			parameters ["@p15"].Value = 15;
			parameters ["@p16"].Value = new byte[2] {0x56,0x78};
			parameters ["@p17"].Value = "This is a varchar";

			insertCommand.ExecuteNonQuery ();
		}

		// execute a SQL SELECT Query using ExecuteReader() to retrieve
		// a IDataReader so we retrieve data
		static IDataReader SelectData (IDbConnection cnc) 
		{
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			selectCommand.CommandText = 
				"SELECT " +				
				" bit_value, " +
				" binary_value, " +
				" char_value, " +
				" datetime_value, " +
				" decimal_value, " +
				" float_value, " +
				" int_value, " +
				" money_value, " +
				" nchar_value, " +
				" nvarchar_value, " +
				" real_value, " + 
				" smalldatetime_value, " +
				" smallint_value, " +
				" smallmoney_value, " +
				" timestamp_value, " +
				" tinyint_value, " +
				" varbinary_value, " +
				" varchar_value, " +
				" null_binary_value, " +
				" null_char_value, " +
				" null_datetime_value, " +
				" null_decimal_value, " +
				" null_float_value, " +
				" null_int_value, " +
				" null_money_value, " +
				" null_nchar_value, " +
				" null_nvarchar_value, " +
				" null_real_value, " + 
				" null_smalldatetime_value, " +
				" null_smallint_value, " +
				" null_smallmoney_value, " +
				" null_tinyint_value, " +
				" null_varbinary_value, " +
				" null_varchar_value " +
				"FROM mono_sybase_test";


			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		static IDataReader SelectDataUsingInsertCommand (IDbConnection cnc)
		{
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL INSERT Command, not a Query
			selectCommand.CommandText =
				"INSERT INTO mono_sybase_test (" +
				" bit_value, " +
				" binary_value, " +
				" char_value, " +
				" datetime_value, " +
				" decimal_value, " +
				" float_value, " +
				" int_value, " +
				" money_value, " +
				" nchar_value, " +
				" nvarchar_value, " +
				" real_value, " + 
				" smalldatetime_value, " +
				" smallint_value, " +
				" smallmoney_value, " +
				" tinyint_value, " +
				" varbinary_value, " +
				" varchar_value " +
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
				"@p17 " +
				")";

			SybaseParameterCollection parameters = ((SybaseCommand) selectCommand).Parameters;

			parameters.Add ("@p1",  SybaseType.Bit);
			parameters.Add ("@p2",  SybaseType.Binary, 8);
			parameters.Add ("@p3",  SybaseType.Char, 14);
			parameters.Add ("@p4",  SybaseType.DateTime);
			parameters.Add ("@p5",  SybaseType.Decimal);
			parameters.Add ("@p6",  SybaseType.Float);
			parameters.Add ("@p7",  SybaseType.Int);
			parameters.Add ("@p8",  SybaseType.Money);
			parameters.Add ("@p9", SybaseType.NChar, 16);
			parameters.Add ("@p10", SybaseType.NVarChar, 19);
			parameters.Add ("@p11", SybaseType.Real);
			parameters.Add ("@p12", SybaseType.SmallDateTime);
			parameters.Add ("@p13", SybaseType.SmallInt);
			parameters.Add ("@p14", SybaseType.SmallMoney);
			parameters.Add ("@p15", SybaseType.TinyInt);
			parameters.Add ("@p16", SybaseType.VarBinary, 8);
			parameters.Add ("@p17", SybaseType.VarChar, 17);

			parameters ["@p1"].Value = true;
			parameters ["@p2"].Value = new byte[2] {0x9a,0xbc};
			parameters ["@p3"].Value = "This is a char";
			parameters ["@p4"].Value = DateTime.Now;

			parameters ["@p5"].Value = 123456789012.345;
			parameters ["@p5"].Precision = 15;
			parameters ["@p5"].Scale = 3;

			parameters ["@p6"].Value = 3.1415926969696;
			parameters ["@p7"].Value = 1048000;
			parameters ["@p8"].Value = 31337.456;
			parameters ["@p9"].Value = "This is an nchar";
			parameters ["@p10"].Value = "This is an nvarchar";
			parameters ["@p11"].Value = 3.141592;
			parameters ["@p12"].Value = new DateTime (2001, 7, 9);
			parameters ["@p13"].Value = -22;
			parameters ["@p14"].Value = 31337.456;
			parameters ["@p15"].Value = 15;
			parameters ["@p16"].Value = new byte[2] {0xde, 0xef};
			parameters ["@p17"].Value = "This is a varchar";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}

		// Tests a SQL Command not (INSERT, UPDATE, DELETE)
		// executed via ExecuteReader
		static IDataReader SelectDataUsingCommand (IDbConnection cnc) 
		{
			IDbCommand selectCommand = cnc.CreateCommand();
			IDataReader reader;

			// This is a SQL Command, not a Query
			selectCommand.CommandText = 
				"SET FMTONLY OFF";

			reader = selectCommand.ExecuteReader ();

			return reader;
		}


		// execute an SQL UPDATE Command using ExecuteNonQuery()
		static void UpdateData (IDbConnection cnc) 
		{
			IDbCommand updateCommand = cnc.CreateCommand();		
		
			updateCommand.CommandText = 
				"update mono_sybase_test " +				
				"set " +
				"bit_value              = @p1, " +
				"tinyint_value          = @p2, " +
				"smallint_value         = @p3, " +
				"int_value              = @p4, " +
				"char_value             = @p5, " +
				"nchar_value            = @p6, " +
				"varchar_value          = @p7, " +
				"nvarchar_value         = @p8, " +
				"binary_value           = @p9, " +
				"varbinary_value        = @p10  " +
				"where smallint_value   = @p11";

			SybaseParameterCollection parameters = ((SybaseCommand) updateCommand).Parameters;

			parameters.Add ("@p1",  SybaseType.Bit);
			parameters.Add ("@p2",  SybaseType.TinyInt);
			parameters.Add ("@p3",  SybaseType.SmallInt);
			parameters.Add ("@p4",  SybaseType.Int);
			parameters.Add ("@p5",  SybaseType.Char, 10);
			parameters.Add ("@p6",  SybaseType.NChar, 10);
			parameters.Add ("@p7",  SybaseType.VarChar, 14);
			parameters.Add ("@p8",  SybaseType.NVarChar, 14);
			parameters.Add ("@p9", SybaseType.Binary, 8);
			parameters.Add ("@p10", SybaseType.VarBinary, 8);
			parameters.Add ("@p11", SybaseType.SmallInt);

			parameters ["@p1"].Value = false;
			parameters ["@p2"].Value = 2;
			parameters ["@p3"].Value = 5;
			parameters ["@p4"].Value = 3;
			parameters ["@p5"].Value = "Mono.Data!";
			parameters ["@p6"].Value = "Mono.Data!";
			parameters ["@p7"].Value = "It was not me!";
			parameters ["@p8"].Value = "It was not me!";
			parameters ["@p9"].Value = new byte[2] {0x57,0x3e};
			parameters ["@p10"].Value = new byte[2] {0xa2,0xf7};
			parameters ["@p11"].Value = -22;

			updateCommand.ExecuteNonQuery ();		
		}

		// used to do a min(), max(), count(), sum(), or avg()
		// execute SQL SELECT Query using ExecuteScalar
		static object SelectAggregate (IDbConnection cnc, String agg) 
		{
			IDbCommand selectCommand = cnc.CreateCommand();
			object data;

			Console.WriteLine("Aggregate: " + agg);

			selectCommand.CommandType = CommandType.Text;
			selectCommand.CommandText = 
				"select " + agg +
				"from mono_sybase_test";

			data = selectCommand.ExecuteScalar ();

			Console.WriteLine("Agg Result: " + data);

			return data;
		}

		// used internally by ReadData() to read each result set
		static void ReadResult(IDataReader rdr, DataTable dt) 
		{        			
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
		
		/* Sybase provider tests */
		static void DoSybaseTest (IDbConnection cnc) 
		{
			IDataReader reader;
			Object oDataValue;

			Console.WriteLine ("\tSybase provider specific tests...\n");

			/* Drops the mono_sybase_test table. */
			Console.WriteLine ("\t\tDrop table: ");
			try {
				DropTable (cnc);
				Console.WriteLine ("OK");
			}
			catch (SybaseException e) {
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
					((SybaseConnection)cnc).ServerVersion);

				/* Clean up */
				Console.WriteLine ("Clean up...");
				Console.WriteLine ("\t\tDrop table...");
				
				Console.WriteLine("OK");
			}
			catch(Exception e) {
				Console.WriteLine("Exception caught: " + e);
			}
		}

		[STAThread]
		static void Main(string[] args) 
		{
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
				Console.WriteLine("Usage: mono SybaseTest.exe sql_server database user_id password");
				return;
			}

			SybaseConnection cnc = new SybaseConnection ();
			cnc.ConnectionString =  connectionString;

			cnc.Open();
			DoSybaseTest(cnc);
			cnc.Close();
		}
	}
}

