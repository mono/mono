// 
// TestOracleClient.cs - Tests Sytem.Data.OracleClient
//                       data provider in Mono.
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.OCI
//
// Tests:
//     Assembly: System.Data.OracleClient.dll
//     Namespace: System.Data.OracleClient
// 
// To Compile:
// mcs TestOracleClient.cs /r:System.Data.dll /r:System.Data.OracleClient.dll /nowarn:0168
//
// Author: 
//     Daniel Morgan <danielmorgan@verizon.net>
//         
// Copyright (C) Daniel Morgan, 2002, 2004-2005
// 

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using System.Threading;

namespace Test.OracleClient
{
	public class OracleTest
	{
		private static Thread t = null;
		private static string conStr;
		public static readonly int MAX_CONNECTIONS = 30; // max connections default to 100, but I will set to 30.

		public OracleTest() 
		{

		}

		static void MonoTest(OracleConnection con)  
		{
			Console.WriteLine ("  Drop table MONO_ORACLE_TEST ...");
			try {
				OracleCommand cmd2 = con.CreateCommand ();
				cmd2.CommandText = "DROP TABLE MONO_ORACLE_TEST";
				cmd2.ExecuteNonQuery ();
			}
			catch (OracleException oe1) {
				// ignore if table already exists
			}

			OracleCommand cmd = null;

			Console.WriteLine("  Creating table MONO_ORACLE_TEST...");
			cmd = new OracleCommand();
			cmd.Connection = con;
			cmd.CommandText = "CREATE TABLE MONO_ORACLE_TEST ( " +
				" varchar2_value VarChar2(32),  " +
				" long_value long, " +
				" number_whole_value Number(18), " +
				" number_scaled_value Number(18,2), " +
 				" number_integer_value Integer, " +
 				" float_value Float, " +
 				" date_value Date, " +
 				" char_value Char(32), " +
 				" clob_value Clob, " +
 				" blob_value Blob, " +
				" clob_empty_value Clob, " +
				" blob_empty_value Blob, " +
				" varchar2_null_value VarChar2(32),  " +
				" number_whole_null_value Number(18), " +
				" number_scaled_null_value Number(18,2), " +
				" number_integer_null_value Integer, " +
				" float_null_value Float, " +
				" date_null_value Date, " +
				" char_null_value Char(32), " +
				" clob_null_value Clob, " +
				" blob_null_value Blob " +
				")";

			cmd.ExecuteNonQuery();

			Console.WriteLine("  Begin Trans for table MONO_ORACLE_TEST...");
			OracleTransaction trans = con.BeginTransaction ();

			Console.WriteLine("  Inserting value into MONO_ORACLE_TEST...");
			cmd = new OracleCommand();
			cmd.Connection = con;
			cmd.Transaction = trans;
			cmd.CommandText = "INSERT INTO mono_oracle_test " +
 				" ( varchar2_value,  " +
				"  long_value, " +
 				"  number_whole_value, " +
  				"  number_scaled_value, " +
  				"  number_integer_value, " +
  				"  float_value, " +
  				"  date_value, " +
  				"  char_value, " +
  				"  clob_value, " +
  				"  blob_value, " +
				"  clob_empty_value, " +
				"  blob_empty_value " +
				") " +
 				" VALUES( " +
  				"  'Mono', " +
				"  'This is a LONG column', " +
  				"  123, " +
  				"  456.78, " +
  				"  8765, " +
  				"  235.2, " +
  				"  TO_DATE( '2004-12-31', 'YYYY-MM-DD' ), " +
  				"  'US', " +
  				"  EMPTY_CLOB(), " +
  				"  EMPTY_BLOB()," +
				"  EMPTY_CLOB(), " +
				"  EMPTY_BLOB()" +
				")";

			cmd.ExecuteNonQuery();

			Console.WriteLine("  Select/Update CLOB columns on table MONO_ORACLE_TEST...");

			// update BLOB and CLOB columns
			OracleCommand select = con.CreateCommand ();
			select.Transaction = trans;
			select.CommandText = "SELECT CLOB_VALUE, BLOB_VALUE FROM MONO_ORACLE_TEST FOR UPDATE";
			OracleDataReader reader = select.ExecuteReader ();
			if (!reader.Read ())
				Console.WriteLine ("ERROR: RECORD NOT FOUND");
			// update clob_value
			Console.WriteLine("     Update CLOB column on table MONO_ORACLE_TEST...");
			OracleLob clob = reader.GetOracleLob (0);
			byte[] bytes = null;
			UnicodeEncoding encoding = new UnicodeEncoding ();
			bytes = encoding.GetBytes ("Mono is fun!");
			clob.Write (bytes, 0, bytes.Length);
			clob.Close ();
			// update blob_value
			Console.WriteLine("     Update BLOB column on table MONO_ORACLE_TEST...");
			OracleLob blob = reader.GetOracleLob (1);
			bytes = new byte[6] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x036 };
			blob.Write (bytes, 0, bytes.Length);
			blob.Close ();
			
			Console.WriteLine("  Commit trans for table MONO_ORACLE_TEST...");
			trans.Commit ();

			// OracleCommand.ExecuteReader of MONO_ORACLE_TEST table
			Console.WriteLine("  Read simple test for table MONO_ORACLE_TEST...");
			ReadSimpleTest(con, "SELECT * FROM MONO_ORACLE_TEST");

			// OracleCommand.ExecuteScalar
			Console.WriteLine(" -ExecuteScalar tests...");
			string varchar2_value = (string) ReadScalar (con,"SELECT MAX(varchar2_value) FROM MONO_ORACLE_TEST");
			Console.WriteLine("     String Value: " + varchar2_value);

			Console.WriteLine("  Read Scalar: number_whole_value");
			decimal number_whole_value = (decimal) 
			ReadScalar (con,"SELECT MAX(number_whole_value) FROM MONO_ORACLE_TEST");
			Console.WriteLine("     Int32 Value: " + number_whole_value.ToString());

			Console.WriteLine("  Read Scalar: number_scaled_value");
			decimal number_scaled_value = (decimal) 
			ReadScalar (con,"SELECT number_scaled_value FROM MONO_ORACLE_TEST");
			Console.WriteLine("     Decimal Value: " + number_scaled_value.ToString());
		
			Console.WriteLine("  Read Scalar: date_value");
			DateTime date_value = (DateTime) 
			ReadScalar (con,"SELECT date_value FROM MONO_ORACLE_TEST");
			Console.WriteLine("     DateTime Value: " + date_value.ToString());
			
			Console.WriteLine("  Read Scalar: clob_value");
			string clob_value = (string) 
			ReadScalar (con,"SELECT clob_value FROM MONO_ORACLE_TEST");
			Console.WriteLine("     CLOB Value: " + clob_value);

			Console.WriteLine("  Read Scalar: blob_value");
			byte[] blob_value = (byte[]) 
			ReadScalar (con,"SELECT blob_value FROM MONO_ORACLE_TEST");
			string sblob_value = GetHexString (blob_value);
			Console.WriteLine("     BLOB Value: " + sblob_value);
			
			// OracleCommand.ExecuteOracleScalar
			Console.WriteLine(" -ExecuteOracleScalar tests...");
			Console.WriteLine("  Read Oracle Scalar: varchar2_value");
			ReadOracleScalar (con,"SELECT MAX(varchar2_value) FROM MONO_ORACLE_TEST");

			Console.WriteLine("  Read Oracle Scalar: number_whole_value");
			ReadOracleScalar (con,"SELECT MAX(number_whole_value) FROM MONO_ORACLE_TEST");

			Console.WriteLine("  Read Oracle Scalar: number_scaled_value");
			ReadOracleScalar (con,"SELECT number_scaled_value FROM MONO_ORACLE_TEST");
		
			Console.WriteLine("  Read Oracle Scalar: date_value");
			ReadOracleScalar (con,"SELECT date_value FROM MONO_ORACLE_TEST");
			
			Console.WriteLine("  Read Oracle Scalar: clob_value");
			ReadOracleScalar (con,"SELECT clob_value FROM MONO_ORACLE_TEST");

			Console.WriteLine("  Read Oracle Scalar: blob_value");
			ReadOracleScalar (con,"SELECT blob_value FROM MONO_ORACLE_TEST");
		}

		static object ReadScalar (OracleConnection con, string selectSql) 
		{
			OracleCommand cmd = null;
			cmd = con.CreateCommand();
			cmd.CommandText = selectSql;

			object o = cmd.ExecuteScalar ();

			string dataType = o.GetType ().ToString ();
			Console.WriteLine ("       DataType: " + dataType);
			return o;
		}

		static void ReadOracleScalar (OracleConnection con, string selectSql) 
		{
			OracleCommand cmd = null;
			cmd = con.CreateCommand();
			cmd.CommandText = selectSql;

			object o = cmd.ExecuteOracleScalar ();

			string dataType = o.GetType ().ToString ();
			Console.WriteLine ("       DataType: " + dataType);
			if (dataType.Equals("System.Data.OracleClient.OracleLob"))
				o = ((OracleLob) o).Value;
			if (o.GetType ().ToString ().Equals ("System.Byte[]"))
				o = GetHexString ((byte[])o);
			
			Console.WriteLine ("          Value: " + o.ToString ());
		}

		static void ReadSimpleTest(OracleConnection con, string selectSql) 
		{
			OracleCommand cmd = null;
			OracleDataReader reader = null;
		
			cmd = con.CreateCommand();
			cmd.CommandText = selectSql;
			reader = cmd.ExecuteReader();
		
			Console.WriteLine("  Results...");
			Console.WriteLine("    Schema");
			DataTable table;
			table = reader.GetSchemaTable();
			for(int c = 0; c < reader.FieldCount; c++) {
				Console.WriteLine("  Column " + c.ToString());
				DataRow row = table.Rows[c];
			
				string strColumnName = row["ColumnName"].ToString();
				string strBaseColumnName = row["BaseColumnName"].ToString();
				string strColumnSize = row["ColumnSize"].ToString();
				string strNumericScale = row["NumericScale"].ToString();
				string strNumericPrecision = row["NumericPrecision"].ToString();
				string strDataType = row["DataType"].ToString();

				Console.WriteLine("      ColumnName: " + strColumnName);
				Console.WriteLine("      BaseColumnName: " + strBaseColumnName);
				Console.WriteLine("      ColumnSize: " + strColumnSize);
				Console.WriteLine("      NumericScale: " + strNumericScale);
				Console.WriteLine("      NumericPrecision: " + strNumericPrecision);
				Console.WriteLine("      DataType: " + strDataType);
			}

			int r = 0;
			Console.WriteLine ("    Data");
			while (reader.Read ()) {
				r++;
				Console.WriteLine ("       Row: " + r.ToString ());
				for (int f = 0; f < reader.FieldCount; f++) {
					string sname = "";
					object ovalue = "";
					string svalue = "";
					string sDataType = "";
					string sFieldType = "";
					string sDataTypeName = "";
					string sOraDataType = "";

					sname = reader.GetName (f);

					if (reader.IsDBNull (f)) {
						ovalue = DBNull.Value;
						svalue = "";
						sDataType = "DBNull.Value";
						sOraDataType = "DBNull.Value";
					}
					else {
						ovalue = reader.GetValue (f);
						//ovalue = reader.GetOracleValue (f);
						object oravalue = null;
					
						sDataType = ovalue.GetType ().ToString ();
						switch (sDataType) {
						case "System.Data.OracleClient.OracleString":
							oravalue = ((OracleString) ovalue).Value;
							break;
						case "System.Data.OracleClient.OracleNumber":
							oravalue = ((OracleNumber) ovalue).Value;
							break;
						case "System.Data.OracleClient.OracleLob":
							OracleLob lob = (OracleLob) ovalue;
							oravalue = lob.Value;
							lob.Close ();
							break;
						case "System.Data.OracleClient.OracleDateTime":
							oravalue = ((OracleDateTime) ovalue).Value;
							break;
						case "System.Byte[]":
							oravalue = GetHexString((byte[])ovalue);
							break;
						case "System.Decimal":
							Console.WriteLine("           *** Get Decimal, Int16, Int32, Int64, Float, Double, ...");
							decimal dec = reader.GetDecimal (f);
							Console.WriteLine("             GetDecimal: " + dec.ToString ());

							oravalue = (object) dec;

							try {
								reader.GetInt16 (f);
							} catch (NotSupportedException e) {
								Console.WriteLine ("            ** Expected exception caught for GetInt16: NotSupportedException: " + e.Message);
							}

							try {
								long lng = reader.GetInt64 (f);
								Console.WriteLine("           GetInt64: " + lng.ToString ());
								int n = reader.GetInt32 (f);
								Console.WriteLine("           GetInt32: " + n.ToString ());
								float flt = reader.GetFloat (f);
								Console.WriteLine("           GetFloat: " + flt.ToString ());
								double dbl = reader.GetDouble (f);
								Console.WriteLine("           GetDouble: " + dbl.ToString ());
							} catch (OverflowException oe1) {
								Console.WriteLine ("            ** Overflow exception for numbers to big or too small: " + oe1.Message);
								}
						
							break;
						default:
							oravalue = ovalue.ToString ();

							break;
						}
					
						sOraDataType = oravalue.GetType ().ToString ();
						if (sOraDataType.Equals ("System.Byte[]")) 
							svalue = GetHexString ((byte[]) oravalue);
						else
							svalue = oravalue.ToString();
						
					}
					sFieldType = reader.GetFieldType(f).ToString();
					sDataTypeName = reader.GetDataTypeName(f);

					Console.WriteLine("           Field: " + f.ToString());
					Console.WriteLine("               Name: " + sname);
					Console.WriteLine("               Value: " + svalue);
					Console.WriteLine("               Oracle Data Type: " + sOraDataType);
					Console.WriteLine("               Data Type: " + sDataType);
					Console.WriteLine("               Field Type: " + sFieldType);
					Console.WriteLine("               Data Type Name: " + sDataTypeName);
				}
			}
			if(r == 0)
				Console.WriteLine("  No data returned.");
		}
		
		static void DataAdapterTest (OracleConnection connection)
		{
			Console.WriteLine("  Create select command...");
			OracleCommand command = connection.CreateCommand ();
			command.CommandText = "SELECT * FROM EMP";

			Console.WriteLine("  Create data adapter...");
			OracleDataAdapter adapter = new OracleDataAdapter (command);

			Console.WriteLine("  Create DataSet...");
			DataSet dataSet = new DataSet ("EMP");

			Console.WriteLine("  Fill DataSet via data adapter...");
			adapter.Fill (dataSet);

			Console.WriteLine("  Get DataTable...");
			DataTable table = dataSet.Tables [0];

			Console.WriteLine("  Display each row...");
			int rowCount = 0;
			foreach (DataRow row in table.Rows) {
				Console.WriteLine ("    row {0}", rowCount + 1);
				for (int i = 0; i < table.Columns.Count; i += 1) {
					Console.WriteLine ("      {0}: {1}", table.Columns [i].ColumnName, row [i]);
				}
				Console.WriteLine ();
				rowCount += 1;
			}
		}

		static void RollbackTest (OracleConnection connection)
		{
			OracleTransaction transaction = connection.BeginTransaction ();

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO EMP (EMPNO, ENAME, JOB) VALUES (8787, 'T Coleman', 'Monoist')";

			Console.WriteLine ("  Inserting record ...");

			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";
			select.Transaction = transaction;
			OracleDataReader reader = select.ExecuteReader ();
			reader.Read ();

			Console.WriteLine ("  Row count SHOULD BE 1, VALUE IS {0}", reader.GetValue (0));
			reader.Close ();

			Console.WriteLine ("  Rolling back transaction ...");

			transaction.Rollback ();

			select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";

			reader = select.ExecuteReader ();
			reader.Read ();
			Console.WriteLine ("  Row count SHOULD BE 0, VALUE IS {0}", reader.GetValue (0));
			reader.Close ();
		}
		
		static void CommitTest (OracleConnection connection)
		{
			OracleTransaction transaction = connection.BeginTransaction ();

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO EMP (EMPNO, ENAME, JOB) VALUES (8787, 'T Coleman', 'Monoist')";

			Console.WriteLine ("  Inserting record ...");

			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";
			select.Transaction = transaction;

			Console.WriteLine ("  Row count SHOULD BE 1, VALUE IS {0}", select.ExecuteScalar ());

			Console.WriteLine ("  Committing transaction ...");

			transaction.Commit ();

			select = connection.CreateCommand ();
			select.CommandText = "SELECT COUNT(*) FROM EMP WHERE EMPNO = 8787";

			Console.WriteLine ("Row count SHOULD BE 1, VALUE IS {0}", select.ExecuteScalar ());
			transaction = connection.BeginTransaction ();
			OracleCommand delete = connection.CreateCommand ();
			delete.Transaction = transaction;
			delete.CommandText = "DELETE FROM EMP WHERE EMPNO = 8787";
			delete.ExecuteNonQuery ();
			transaction.Commit ();
		}

		public static void ParameterTest (OracleConnection connection)
		{
			Console.WriteLine("  Setting NLS_DATE_FORMAT...");

			OracleCommand cmd2 = connection.CreateCommand();
			cmd2.CommandText = "ALTER SESSION SET NLS_DATE_FORMAT = 'YYYY-MM-DD HH24:MI:SS'";
		
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  Drop table MONO_TEST_TABLE2...");
			try {
				cmd2.CommandText = "DROP TABLE MONO_TEST_TABLE7";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine("  Create table MONO_TEST_TABLE7...");

			cmd2.CommandText = "CREATE TABLE MONO_TEST_TABLE7(" +
				" COL1 VARCHAR2(8) NOT NULL, " +
				" COL2 VARCHAR2(32), " +
				" COL3 NUMBER(18,2) NOT NULL, " +
				" COL4 NUMBER(18,2), " +
				" COL5 DATE NOT NULL, " +
				" COL6 DATE, " +
				" COL7 BLOB NOT NULL, " +
				" COL8 BLOB, " +
				" COL9 CLOB NOT NULL, " +
				" COL10 CLOB " +
				")";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  COMMIT...");
			cmd2.CommandText = "COMMIT";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  create insert command...");

			OracleTransaction trans = connection.BeginTransaction ();
			OracleCommand cmd = connection.CreateCommand ();
			cmd.Transaction = trans;

			cmd.CommandText = "INSERT INTO MONO_TEST_TABLE7 " + 
				"(COL1,COL2,COL3,COL4,COL5,COL6,COL7,COL8,COL9,COL10) " + 
				"VALUES(:P1,:P2,:P3,:P4,:P5,:P6,:P7,:P8,:P9,:P10)";
		
			Console.WriteLine("  Add parameters...");

			OracleParameter parm1 = cmd.Parameters.Add (":P1", OracleType.VarChar, 8);
			OracleParameter parm2 = cmd.Parameters.Add (":P2", OracleType.VarChar, 32);
		
			OracleParameter parm3 = cmd.Parameters.Add (":P3", OracleType.Number);
			OracleParameter parm4 = cmd.Parameters.Add (":P4", OracleType.Number);
		
			OracleParameter parm5 = cmd.Parameters.Add (":P5", OracleType.DateTime);
			OracleParameter parm6 = cmd.Parameters.Add (":P6", OracleType.DateTime);

			// FIXME: fix BLOBs and CLOBs in OracleParameter

			OracleParameter parm7 = cmd.Parameters.Add (":P7", OracleType.Blob);
			OracleParameter parm8 = cmd.Parameters.Add (":P8", OracleType.Blob);

			OracleParameter parm9 = cmd.Parameters.Add (":P9", OracleType.Clob);
			OracleParameter parm10 = cmd.Parameters.Add (":P10", OracleType.Clob);

			// TODO: implement out, return, and ref parameters

			string s = "Mono";
			decimal d = 123456789012345.678M;
			DateTime dt = DateTime.Now;

			string clob = "Clob";
			byte[] blob = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35 };
		
			Console.WriteLine("  Set Values...");

			parm1.Value = s;
			parm2.Value = DBNull.Value;
		
			parm3.Value = d;
			parm4.Value = DBNull.Value;
		
			parm5.Value = dt;
			parm6.Value = DBNull.Value;
		
			parm7.Value = blob;
			parm8.Value = DBNull.Value;

			parm9.Value = clob;
			parm10.Value = DBNull.Value;
		
			Console.WriteLine("  ExecuteNonQuery...");

			cmd.ExecuteNonQuery ();
			trans.Commit();
		}

		public static void CLOBTest (OracleConnection connection)
		{		
			Console.WriteLine ("  BEGIN TRANSACTION ...");

			OracleTransaction transaction = connection.BeginTransaction ();

			Console.WriteLine ("  Drop table CLOBTEST ...");
			try {
				OracleCommand cmd2 = connection.CreateCommand ();
				cmd2.Transaction = transaction;
				cmd2.CommandText = "DROP TABLE CLOBTEST";
				cmd2.ExecuteNonQuery ();
			}
			catch (OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine ("  CREATE TABLE ...");

			OracleCommand create = connection.CreateCommand ();
			create.Transaction = transaction;
			create.CommandText = "CREATE TABLE CLOBTEST (CLOB_COLUMN CLOB)";
			create.ExecuteNonQuery ();

			Console.WriteLine ("  INSERT RECORD ...");

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO CLOBTEST VALUES (EMPTY_CLOB())";
			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.Transaction = transaction;
			select.CommandText = "SELECT CLOB_COLUMN FROM CLOBTEST FOR UPDATE";
			Console.WriteLine ("  SELECTING A CLOB (CHARACTER) VALUE FROM CLOBTEST");

			OracleDataReader reader = select.ExecuteReader ();
			if (!reader.Read ())
				Console.WriteLine ("ERROR: RECORD NOT FOUND");

			Console.WriteLine ("  TESTING OracleLob OBJECT ...");
			OracleLob lob = reader.GetOracleLob (0);
			Console.WriteLine ("  LENGTH: {0}", lob.Length);
			Console.WriteLine ("  CHUNK SIZE: {0}", lob.ChunkSize);

			UnicodeEncoding encoding = new UnicodeEncoding ();

			byte[] value = new byte [lob.Length * 2];

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);
			Console.WriteLine ("  UPDATING VALUE TO 'TEST ME!'");
			value = encoding.GetBytes ("TEST ME!");
			lob.Write (value, 0, value.Length);

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);
			Console.WriteLine ("  RE-READ VALUE...");
			lob.Seek (1, SeekOrigin.Begin);

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);
			value = new byte [lob.Length * 2];
			lob.Read (value, 0, value.Length);
			Console.WriteLine ("  VALUE: {0}", encoding.GetString (value));
			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);

			Console.WriteLine ("  CLOSE OracleLob...");
			lob.Close ();

			Console.WriteLine ("  CLOSING READER...");
			
			reader.Close ();
			transaction.Commit ();
		}

		public static void BLOBTest (OracleConnection connection) 
		{
			Console.WriteLine ("  BEGIN TRANSACTION ...");

			OracleTransaction transaction = connection.BeginTransaction ();

			Console.WriteLine ("  Drop table BLOBTEST ...");
			try {
				OracleCommand cmd2 = connection.CreateCommand ();
				cmd2.Transaction = transaction;
				cmd2.CommandText = "DROP TABLE BLOBTEST";
				cmd2.ExecuteNonQuery ();
			}
			catch (OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine ("  CREATE TABLE ...");

			OracleCommand create = connection.CreateCommand ();
			create.Transaction = transaction;
			create.CommandText = "CREATE TABLE BLOBTEST (BLOB_COLUMN BLOB)";
			create.ExecuteNonQuery ();

			Console.WriteLine ("  INSERT RECORD ...");

			OracleCommand insert = connection.CreateCommand ();
			insert.Transaction = transaction;
			insert.CommandText = "INSERT INTO BLOBTEST VALUES (EMPTY_BLOB())";
			insert.ExecuteNonQuery ();

			OracleCommand select = connection.CreateCommand ();
			select.Transaction = transaction;
			select.CommandText = "SELECT BLOB_COLUMN FROM BLOBTEST FOR UPDATE";
			Console.WriteLine ("  SELECTING A BLOB (Binary) VALUE FROM BLOBTEST");

			OracleDataReader reader = select.ExecuteReader ();
			if (!reader.Read ())
				Console.WriteLine ("ERROR: RECORD NOT FOUND");

			Console.WriteLine ("  TESTING OracleLob OBJECT ...");
			OracleLob lob = reader.GetOracleLob (0);
			
			byte[] value = null;
			string bvalue = "";

			Console.WriteLine ("  UPDATING VALUE");

			byte[] bytes = new byte[6];
			bytes[0] = 0x31;
			bytes[1] = 0x32;
			bytes[2] = 0x33;
			bytes[3] = 0x34;
			bytes[4] = 0x35;
			bytes[5] = 0x36;

			lob.Write (bytes, 0, bytes.Length);

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);
			Console.WriteLine ("  RE-READ VALUE...");
			lob.Seek (1, SeekOrigin.Begin);

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);
			value = new byte [lob.Length];
			lob.Read (value, 0, value.Length);
			
			bvalue = "";
			if (value.GetType ().ToString ().Equals ("System.Byte[]")) 
				bvalue = GetHexString (value);
			Console.WriteLine ("  Bytes: " + bvalue);

			Console.WriteLine ("  CURRENT POSITION: {0}", lob.Position);

			Console.WriteLine ("  CLOSE OracleLob...");
			lob.Close ();

			Console.WriteLine ("  CLOSING READER...");
			
			reader.Close ();
			transaction.Commit ();
		}

		static void Wait(string msg) 
		{
			Console.WriteLine(msg);
			if (msg.Equals(""))
				Console.WriteLine("Waiting...  Press Enter to continue...");
			Console.ReadLine();
		}

		// use this function to read a byte array into a string
		// for easy display of binary data, such as, a BLOB value
		public static string GetHexString (byte[] bytes)
		{ 			
			string bvalue = "";
			
			StringBuilder sb2 = new StringBuilder();
			for (int z = 0; z < bytes.Length; z++) {
				byte byt = bytes[z];
				sb2.Append (byt.ToString("x"));
			}
			if (sb2.Length > 0)
				bvalue = "0x" + sb2.ToString ();
	
			return bvalue;
		}

		static void StoredProcedureTest1 (OracleConnection con) 
		{
			// test stored procedure with no parameters
			
			
			OracleCommand cmd2 = con.CreateCommand ();

			Console.WriteLine("  Drop table MONO_TEST_TABLE1...");
			try {
				cmd2.CommandText = "DROP TABLE MONO_TEST_TABLE1";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if table did not exist
			}

			Console.WriteLine("  Drop procedure SP_TEST1...");
			try {
				cmd2.CommandText = "DROP PROCEDURE SP_TEST1";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if procedure did not exist
			}

			Console.WriteLine("  Create table MONO_TEST_TABLE1...");
			cmd2.CommandText = "CREATE TABLE MONO_TEST_TABLE1 (" +
					" COL1 VARCHAR2(8), "+
					" COL2 VARCHAR2(32))";
			cmd2.ExecuteNonQuery ();
			
			Console.WriteLine("  Create stored procedure SP_TEST1...");
			cmd2.CommandText = "CREATE PROCEDURE SP_TEST1 " +
				" IS " +
				" BEGIN " +
				"	INSERT INTO MONO_TEST_TABLE1 (COL1,COL2) VALUES ('aaa','bbbb');" +
				"	COMMIT;" +
				" END;";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("COMMIT...");
			cmd2.CommandText = "COMMIT";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  Call stored procedure sp_test1...");
			OracleCommand cmd3 = con.CreateCommand ();
			cmd3.CommandType = CommandType.StoredProcedure;
			cmd3.CommandText = "sp_test1";
			cmd3.ExecuteNonQuery ();
		}

		static void StoredProcedureTest2 (OracleConnection con) 
		{
			// test stored procedure with 2 parameters

			Console.WriteLine("  Drop table MONO_TEST_TABLE2...");
			OracleCommand cmd2 = con.CreateCommand ();

			try {
				cmd2.CommandText = "DROP TABLE MONO_TEST_TABLE2";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine("  Drop procedure SP_TEST2...");
			try {
				cmd2.CommandText = "DROP PROCEDURE SP_TEST2";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine("  Create table MONO_TEST_TABLE2...");
						
			cmd2.CommandText = "CREATE TABLE MONO_TEST_TABLE2 (" +
				" COL1 VARCHAR2(8), "+
				" COL2 VARCHAR2(32))";
			cmd2.ExecuteNonQuery ();
			
			Console.WriteLine("  Create stored procedure SP_TEST2...");
			cmd2.CommandText = "CREATE PROCEDURE SP_TEST2(parm1 VARCHAR2,parm2 VARCHAR2) " +
				" IS " +
				" BEGIN " +
				"	INSERT INTO MONO_TEST_TABLE2 (COL1,COL2) VALUES (parm1,parm2);" +
				"	COMMIT;" +
				" END;";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  COMMIT...");
			cmd2.CommandText = "COMMIT";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  Call stored procedure SP_TEST2 with two parameters...");
			OracleCommand cmd3 = con.CreateCommand ();
			cmd3.CommandType = CommandType.StoredProcedure;
			cmd3.CommandText = "sp_test2";

			OracleParameter myParameter1 = new OracleParameter("parm1", OracleType.VarChar);
			myParameter1.Value = "yyy13";
			myParameter1.Size = 8;
			myParameter1.Direction = ParameterDirection.Input;
		
			OracleParameter myParameter2 = new OracleParameter("parm2", OracleType.VarChar);
			myParameter2.Value = "iii13";
			myParameter2.Size = 32;
			myParameter2.Direction = ParameterDirection.Input;

			cmd3.Parameters.Add (myParameter1);
			cmd3.Parameters.Add (myParameter2);

			cmd3.ExecuteNonQuery ();
		}

		static void ShowConnectionProperties (OracleConnection con) 
		{
			try {
				Console.WriteLine ("ServerVersion: " + con.ServerVersion);
			} catch (System.InvalidOperationException ioe) {
				Console.WriteLine ("InvalidOperationException caught.");
				Console.WriteLine ("Message: " + ioe.Message);
			}

			Console.WriteLine ("DataSource: " + con.DataSource);
		}

		static void NullAggregateTest (OracleConnection con)
		{
			Console.WriteLine("  Drop table MONO_TEST_TABLE3...");
			OracleCommand cmd2 = con.CreateCommand ();

			try {
				cmd2.CommandText = "DROP TABLE MONO_TEST_TABLE3";
				cmd2.ExecuteNonQuery ();
			}
			catch(OracleException oe1) {
				// ignore if table already exists
			}

			Console.WriteLine("  Create table MONO_TEST_TABLE3...");
						
			cmd2.CommandText = "CREATE TABLE MONO_TEST_TABLE3 (" +
				" COL1 VARCHAR2(8), "+
				" COL2 VARCHAR2(32))";

			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  Insert some rows into table MONO_TEST_TABLE3...");
			cmd2.CommandText = "INSERT INTO MONO_TEST_TABLE3 (COL1, COL2) VALUES ('1','one')";
			cmd2.ExecuteNonQuery ();

			cmd2.CommandText = "INSERT INTO MONO_TEST_TABLE3 (COL1, COL2) VALUES ('1','uno')";
			cmd2.ExecuteNonQuery ();
			
			cmd2.CommandText = "INSERT INTO MONO_TEST_TABLE3 (COL1, COL2) VALUES ('3','three')";
			cmd2.ExecuteNonQuery ();
			
			cmd2.CommandText = "INSERT INTO MONO_TEST_TABLE3 (COL1, COL2) VALUES ('3', null)";
			cmd2.ExecuteNonQuery ();

			cmd2.CommandText = "INSERT INTO MONO_TEST_TABLE3 (COL1, COL2) VALUES ('3','few')";
			cmd2.ExecuteNonQuery ();

			Console.WriteLine("  ExecuteScalar...");
			cmd2.CommandText = "SELECT COL1, COUNT(COL2) AS MAX_COL1 FROM MONO_TEST_TABLE3 GROUP BY COL1";
			OracleDataReader reader = cmd2.ExecuteReader ();
			Console.WriteLine (" Read...");
			while (reader.Read ()) {

				object obj0 = reader.GetValue (0);
				Console.WriteLine("Value 0: " + obj0.ToString ());
				object obj1 = reader.GetValue (1);
				Console.WriteLine("Value 1: " + obj1.ToString ());
			
				Console.WriteLine (" Read...");
			}

			Console.WriteLine (" No more records.");
		}

		static void OnInfoMessage (object sender, OracleInfoMessageEventArgs e) 
		{
			Console.WriteLine("InfoMessage Message: " + e.Message.ToString());
			Console.WriteLine("InfoMessage Code: " + e.Code.ToString());
			Console.WriteLine("InfoMessage Source: " + e.Source.ToString());
		}

		static void OnStateChange (object sender, StateChangeEventArgs e) 
		{
			Console.WriteLine("StateChange CurrentSate:" + e.CurrentState.ToString ());
			Console.WriteLine("StateChange OriginalState:" + e.OriginalState.ToString ());
		}

		public static void ConnectionPoolingTest1 () {
			Console.WriteLine("Start Connection Pooling Test 1...");
			OracleConnection[] connections = null;
			int maxCon = MAX_CONNECTIONS + 1; // add 1 more over the max connections to cause it to wait for the next available connection
			int i = 0;

			try {
				connections = new OracleConnection[maxCon];			
		
				for (i = 0; i < maxCon; i++) {
					Console.WriteLine("   Open connection: {0}", i);
					connections[i] = new OracleConnection(conStr);
					connections[i].Open ();
				}
			} catch (InvalidOperationException e) {
				Console.WriteLine("Expected exception InvalidOperationException caught.");
				Console.WriteLine(e);
			}

			for (i = 0; i < maxCon; i++) {
				if (connections[i] != null) {
					Console.WriteLine("   Close connection: {0}", i);
					if (connections[i].State == ConnectionState.Open)
						connections[i].Close ();
					connections[i].Dispose ();
					connections[i] = null;
				}
			}

			connections = null;

			Console.WriteLine("Done Connection Pooling Test 1.");
		}

		public static void ConnectionPoolingTest2 () {
			Console.WriteLine("Start Connection Pooling Test 2...");
			OracleConnection[] connections = null;
			int maxCon = MAX_CONNECTIONS;
			int i = 0;

			connections = new OracleConnection[maxCon];			
		
			for (i = 0; i < maxCon; i++) {
				Console.WriteLine("   Open connection: {0}", i);
				connections[i] = new OracleConnection(conStr);
				connections[i].Open ();
			}
		
			Console.WriteLine("Start another thread...");
			t = new Thread(new ThreadStart(AnotherThreadProc));
			t.Start ();

			Console.WriteLine("Sleep...");
			Thread.Sleep(100);

			Console.WriteLine("Closing...");
			for (i = 0; i < maxCon; i++) {
				if (connections[i] != null) {
					Console.WriteLine("   Close connection: {0}", i);
					if (connections[i].State == ConnectionState.Open)
						connections[i].Close ();
					connections[i].Dispose ();
					connections[i] = null;
				}
			}

			connections = null;
		}

		private static void AnotherThreadProc () {
			Console.WriteLine("Open connection via another thread...");
			OracleConnection[] connections = null;
			int maxCon = MAX_CONNECTIONS; 
			int i = 0;

			connections = new OracleConnection[maxCon];			
		
			for (i = 0; i < maxCon; i++) {
				Console.WriteLine("   Open connection: {0}", i);
				connections[i] = new OracleConnection(conStr);
				connections[i].Open ();
			}

			Console.WriteLine("Done Connection Pooling Test 2.");
			System.Environment.Exit (0);
		}

		[STAThread]
		static void Main(string[] args) 
		{ 	
			if(args.Length != 3) {
				Console.WriteLine("Usage: mono TestOracleClient database userid password");
				return;
			}

			string connectionString = String.Format(
				"Data Source={0};" +
				"User ID={1};" +
				"Password={2}",
				args[0], args[1], args[2]);

			conStr = connectionString;

			OracleConnection con1 = new OracleConnection();

			ShowConnectionProperties (con1);

			con1.ConnectionString = connectionString;

			con1.InfoMessage += new OracleInfoMessageEventHandler (OnInfoMessage);
			con1.StateChange += new StateChangeEventHandler (OnStateChange);
			Console.WriteLine("Opening...");
			con1.Open ();
			Console.WriteLine("Opened.");

			ShowConnectionProperties (con1);

			Console.WriteLine ("Mono Oracle Test BEGIN ...");
			MonoTest (con1);
			Console.WriteLine ("Mono Oracle Test END ...");

			Wait ("");
			
			Console.WriteLine ("LOB Test BEGIN...");
			CLOBTest (con1);
			BLOBTest (con1);
			Console.WriteLine ("LOB Test END.");
			Wait ("");

			Console.WriteLine ("Read Simple Test BEGIN - scott.emp...");
                        ReadSimpleTest(con1, "SELECT e.*, e.rowid FROM scott.emp e");
			Console.WriteLine ("Read Simple Test END - scott.emp");

			Wait ("");
			
			Console.WriteLine ("DataAdapter Test BEGIN...");
                        DataAdapterTest(con1);
			Console.WriteLine ("DataAdapter Test END.");

			Wait ("");

			Console.WriteLine ("Rollback Test BEGIN...");
                        RollbackTest(con1);
			Console.WriteLine ("Rollback Test END.");

			Wait ("");

			Console.WriteLine ("Commit Test BEGIN...");
                        CommitTest(con1);
			Console.WriteLine ("Commit Test END.");

			Wait ("");

			Console.WriteLine ("Parameter Test BEGIN...");
                        ParameterTest(con1);
			ReadSimpleTest(con1, "SELECT * FROM MONO_TEST_TABLE7");
			Console.WriteLine ("Parameter Test END.");

			Wait ("");
			
			Console.WriteLine ("Stored Proc Test 1 BEGIN...");
			StoredProcedureTest1 (con1);
			ReadSimpleTest(con1, "SELECT * FROM MONO_TEST_TABLE1");
			Console.WriteLine ("Stored Proc Test 1 END...");

			Wait ("");

			Console.WriteLine ("Stored Proc Test 2 BEGIN...");
			StoredProcedureTest2 (con1);
			ReadSimpleTest(con1, "SELECT * FROM MONO_TEST_TABLE2");
			Console.WriteLine ("Stored Proc Test 2 END...");

			Wait ("");

			Console.WriteLine ("Null Aggregate Warning BEGIN test...");
			NullAggregateTest (con1);
			Console.WriteLine ("Null Aggregate Warning END test...");

			Console.WriteLine("Closing...");
			con1.Close ();
			Console.WriteLine("Closed.");

			conStr = conStr + ";pooling=true;min pool size=4;max pool size=" + MAX_CONNECTIONS.ToString ();
			ConnectionPoolingTest1 ();
			ConnectionPoolingTest2 ();

			Console.WriteLine("Done.");
		}
	}
}

