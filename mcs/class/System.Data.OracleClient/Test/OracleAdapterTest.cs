//
// OracleAdapterTest.cs - tests select/insert/update/delete of
//                        a DataSet/DataTable with 
//                        OracleDataAdapter and OracleCommandBuilder
//
// Author:
//      Daniel Morgan <danielmorgan@verizon.net>
//
// Copyright (C) Daniel Morgan, 2005
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Text;

class OracleAdapterTest 
{
	static string infilename = @"mono-win32-setup-dark.bmp";

	public static void Main(string[] args) 
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

		OracleConnection con = new OracleConnection ();
		con.ConnectionString = connectionString;
		con.Open ();
		
		Setup (con);
		ReadSimpleTest (con, "SELECT * FROM mono_adapter_test");
		
		GetMetaData (con);

		Insert (con);
		ReadSimpleTest (con, "SELECT * FROM mono_adapter_test");
		
		Update (con);
		ReadSimpleTest (con, "SELECT * FROM mono_adapter_test");

		//Delete (con);
		//ReadSimpleTest (con, "SELECT * FROM mono_adapter_test");
		
		con.Close ();		
	}

	public static void GetMetaData(OracleConnection con) 
	{
		OracleCommand cmd = null;
		OracleDataReader rdr = null;
		
		cmd = con.CreateCommand();
		cmd.CommandText = "select * from mono_adapter_test";

		Console.WriteLine("Read Schema With KeyInfo");
		rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly);
		
		DataTable dt;
		dt = rdr.GetSchemaTable();
		foreach (DataRow schemaRow in dt.Rows) {
			foreach (DataColumn schemaCol in dt.Columns) {
				Console.WriteLine(schemaCol.ColumnName + 
					" = " + 
					schemaRow[schemaCol]);
				Console.WriteLine("---Type: " + schemaRow[schemaCol].GetType ().ToString());
			}
			Console.WriteLine("");
		}

		Console.WriteLine("Read Schema with No KeyInfo");

		rdr = cmd.ExecuteReader();

		dt = rdr.GetSchemaTable();
		foreach (DataRow schemaRow in dt.Rows) {
			foreach (DataColumn schemaCol in dt.Columns) {
				Console.WriteLine(schemaCol.ColumnName + 
					" = " + 
					schemaRow[schemaCol]);
				Console.WriteLine("---Type: " + schemaRow[schemaCol].GetType ().ToString());
				Console.WriteLine();
			}
		}

	}


	public static void Setup (OracleConnection con)
	{
		Console.WriteLine ("  Drop table mono_adapter_test ...");
		try {
			OracleCommand cmd2 = con.CreateCommand ();
			cmd2.CommandText = "DROP TABLE mono_adapter_test";
			cmd2.ExecuteNonQuery ();
		}
		catch (OracleException oe1) {
			// ignore if table already exists
		}

		OracleCommand cmd = null;
		int rowsAffected = 0;

		Console.WriteLine("  Creating table mono_adapter_test...");
		cmd = new OracleCommand ();
		cmd.Connection = con;
		cmd.CommandText = "CREATE TABLE mono_adapter_test ( " +
			" varchar2_value VarChar2(32),  " +
			" number_whole_value Number(18) PRIMARY KEY, " +
			" number_scaled_value Number(18,2), " +
			" number_integer_value Integer, " +
			" float_value Float, " +
			" date_value Date, " +
			" char_value Char(32), " +
			" clob_value Clob, " +
			" blob_value Blob ) ";

		rowsAffected = cmd.ExecuteNonQuery();

		Console.WriteLine("  Begin Trans for table mono_adapter_test...");
		OracleTransaction trans = con.BeginTransaction ();

		Console.WriteLine("  Inserting value into mono_adapter_test...");
		cmd = new OracleCommand();
		cmd.Connection = con;
		cmd.Transaction = trans;
		cmd.CommandText = "INSERT INTO mono_adapter_test " +
			" ( varchar2_value,  " +
			"  number_whole_value, " +
			"  number_scaled_value, " +
			"  number_integer_value, " +
			"  float_value, " +
			"  date_value, " +
			"  char_value, " +
			"  clob_value, " +
			"  blob_value " +
			") " +
			" VALUES( " +
			"  'Mono', " +
			"  11, " +
			"  456.78, " +
			"  8765, " +
			"  235.2, " +
			"  TO_DATE( '2004-12-31', 'YYYY-MM-DD' ), " +
			"  'US', " +
			"  EMPTY_CLOB(), " +
			"  EMPTY_BLOB() " +
			")";

		rowsAffected = cmd.ExecuteNonQuery();

		Console.WriteLine("  Select/Update CLOB columns on table mono_adapter_test...");

		// update BLOB and CLOB columns
		OracleCommand select = con.CreateCommand ();
		select.Transaction = trans;
		select.CommandText = "SELECT CLOB_VALUE, BLOB_VALUE FROM mono_adapter_test FOR UPDATE";
		OracleDataReader reader = select.ExecuteReader ();
		if (!reader.Read ())
			Console.WriteLine ("ERROR: RECORD NOT FOUND");
		// update clob_value
		Console.WriteLine("     Update CLOB column on table mono_adapter_test...");
		OracleLob clob = reader.GetOracleLob (0);
		byte[] bytes = null;
		UnicodeEncoding encoding = new UnicodeEncoding ();
		bytes = encoding.GetBytes ("Mono is fun!");
		clob.Write (bytes, 0, bytes.Length);
		clob.Close ();
		// update blob_value
		Console.WriteLine("     Update BLOB column on table mono_adapter_test...");
		OracleLob blob = reader.GetOracleLob (1);
		bytes = new byte[6] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x036 };
		blob.Write (bytes, 0, bytes.Length);
		blob.Close ();
			
		Console.WriteLine("  Commit trans for table mono_adapter_test...");
		trans.Commit ();

		CommitCursor (con);
	}

	public static void Insert (OracleConnection con) 
	{
		Console.WriteLine("================================");
		Console.WriteLine("=== Adapter Insert =============");
		Console.WriteLine("================================");
		OracleTransaction transaction = con.BeginTransaction ();
		
		Console.WriteLine("   Create adapter...");
		OracleDataAdapter da = new OracleDataAdapter("select * from mono_adapter_test", con);
		da.SelectCommand.Transaction = transaction;
		
		Console.WriteLine("   Create command builder...");
		OracleCommandBuilder mycb = new OracleCommandBuilder(da);

		Console.WriteLine("   Create data set ...");
		DataSet ds = new DataSet();

		Console.WriteLine("Set missing schema action...");
		da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
		
		Console.WriteLine("Get data from file...");
		FileStream fs = new FileStream(infilename, FileMode.OpenOrCreate, FileAccess.Read);
		Byte[] mydata = new Byte[fs.Length];			
		fs.Read(mydata, 0, (int) fs.Length);
		fs.Close();
		
		Console.WriteLine("Fill data set via adapter...");
		da.Fill(ds, "mono_adapter_test");

		Console.WriteLine("New Row...");
		DataRow myRow;
		myRow = ds.Tables["mono_adapter_test"].NewRow();

		byte[] bytes = new byte[] { 0x45,0x46,0x47,0x48,0x49,0x50 };

		Console.WriteLine("Set values in the new DataRow...");
		myRow["varchar2_value"] = "OracleClient";
		myRow["number_whole_value"] = 22;
		myRow["number_scaled_value"] = 12.34;
		myRow["number_integer_value"] = 456;
		myRow["float_value"] = 98.76;
		myRow["date_value"] = new DateTime(2001,07,09);
		myRow["char_value"] = "Romeo";
		myRow["clob_value"] = "clobtest";
		myRow["blob_value"] = bytes;

		Console.WriteLine("Add DataRow to DataTable...");		
		ds.Tables["mono_adapter_test"].Rows.Add(myRow);

		Console.WriteLine("da.Update(ds...");
		da.Update(ds, "mono_adapter_test");

		transaction.Commit();
	}

	public static void Update (OracleConnection con) 
	{
		Console.WriteLine("================================");
		Console.WriteLine("=== Adapter Update =============");
		Console.WriteLine("================================");

		OracleTransaction transaction = con.BeginTransaction ();

		Console.WriteLine("   Create adapter...");
		OracleCommand selectCmd = con.CreateCommand ();
		selectCmd.Transaction = transaction;
		selectCmd.CommandText = "SELECT * FROM mono_adapter_test";
		OracleDataAdapter da = new OracleDataAdapter(selectCmd);
		Console.WriteLine("   Create command builder...");
		OracleCommandBuilder mycb = new OracleCommandBuilder(da);
		Console.WriteLine("   Create data set ...");
		DataSet ds = new DataSet();

		Console.WriteLine("Set missing schema action...");
		da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
		
		Console.WriteLine("Fill data set via adapter...");
		da.Fill(ds, "mono_adapter_test");
		DataRow myRow;

		Console.WriteLine("New Row...");
		myRow = ds.Tables["mono_adapter_test"].Rows[0];

		Console.WriteLine("Tables Count: " + ds.Tables.Count.ToString());

		DataTable table = ds.Tables["mono_adapter_test"];
		DataRowCollection rows;
		rows = table.Rows;
		Console.WriteLine("Row Count: " + rows.Count.ToString());
		myRow = rows[0];

		byte[] bytes = new byte[] { 0x45,0x46,0x47,0x48,0x49,0x50 };

		Console.WriteLine("Set values in the new DataRow...");
		
		Console.WriteLine("Columns count: " + table.Columns.Count.ToString());

		myRow["varchar2_value"] = "Puppy Power!";
		myRow["number_whole_value"] = 33;
		myRow["number_scaled_value"] = 12.34;
		myRow["number_scaled_value"] = 12.34;
		myRow["number_integer_value"] = 456;
		myRow["float_value"] = 98.76;
		myRow["date_value"] = new DateTime(2001,07,09);
		myRow["char_value"] = "Romeo";
		myRow["clob_value"] = "clobtest";
		myRow["blob_value"] = bytes;

		Console.WriteLine("da.Update(ds...");
		da.Update(ds, "mono_adapter_test");

		transaction.Commit();
	}

	public static void Delete (OracleConnection con) 
	{
		Console.WriteLine("================================");
		Console.WriteLine("=== Adapter Delete =============");
		Console.WriteLine("================================");
		OracleTransaction transaction = con.BeginTransaction ();
		
		Console.WriteLine("   Create adapter...");
		OracleDataAdapter da = new OracleDataAdapter("SELECT * FROM mono_adapter_test", con);
		da.SelectCommand.Transaction = transaction;

		Console.WriteLine("   Create command builder...");
		OracleCommandBuilder mycb = new OracleCommandBuilder(da);

		Console.WriteLine("   Create data set ...");
		DataSet ds = new DataSet();

		Console.WriteLine("Set missing schema action...");
		da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
		
		Console.WriteLine("Fill data set via adapter...");
		da.Fill(ds, "mono_adapter_test");

		Console.WriteLine("Get DataRow...");
		DataTable table = ds.Tables["mono_adapter_test"];
		DataRowCollection rows = table.Rows;
		DataRow myRow = rows[0];

		Console.WriteLine("row remove...");
		rows.Remove(myRow);

		Console.WriteLine("da.Update(table...");
		da.Update(table);

		Console.WriteLine("Commit...");
		transaction.Commit();
	}

	static void CommitCursor (OracleConnection con) 
	{
		OracleCommand cmd = con.CreateCommand ();
		cmd.CommandText = "COMMIT";
		cmd.ExecuteNonQuery ();
		cmd.Dispose ();
		cmd = null;
	}

	static void ReadSimpleTest (OracleConnection con, string selectSql) 
	{
		OracleCommand cmd = null;
		OracleDataReader reader = null;
		
		cmd = con.CreateCommand ();
		cmd.CommandText = selectSql;
		reader = cmd.ExecuteReader ();
		
		Console.WriteLine("  Results...");
		Console.WriteLine("    Schema");
		DataTable table;
		table = reader.GetSchemaTable ();
		for (int c = 0; c < reader.FieldCount; c++) {
			Console.WriteLine("  Column " + c.ToString ());
			DataRow row = table.Rows[c];
			
			string strColumnName = row["ColumnName"].ToString();
			string strBaseColumnName = row["BaseColumnName"].ToString();
			string strColumnSize = row["ColumnSize"].ToString();
			string strNumericScale = row["NumericScale"].ToString();
			string strNumericPrecision = row["NumericPrecision"].ToString();
			string strDataType = row["DataType"].ToString();
			string strBaseTableName = row["BaseTableName"].ToString();
			string strBaseSchemaName = row["BaseSchemaName"].ToString();

			Console.WriteLine("      ColumnName: " + strColumnName);
			Console.WriteLine("      BaseColumnName: " + strBaseColumnName);
			Console.WriteLine("      ColumnSize: " + strColumnSize);
			Console.WriteLine("      NumericScale: " + strNumericScale);
			Console.WriteLine("      NumericPrecision: " + strNumericPrecision);
			Console.WriteLine("      DataType: " + strDataType);
			Console.WriteLine("      BaseTableName: " + strBaseTableName);
			Console.WriteLine("      BaseSchemaName: " + strBaseSchemaName);
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
					ovalue = reader.GetOracleValue (f);
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
					default:
						oravalue = "*** no test available ***";
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
				//Console.WriteLine("               Oracle Data Type: " + sOraDataType);
				//Console.WriteLine("               Data Type: " + sDataType);
				//Console.WriteLine("               Field Type: " + sFieldType);
				//Console.WriteLine("               Data Type Name: " + sDataTypeName);
			}
		}
		if(r == 0)
			Console.WriteLine("  No data returned.");
	}

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
}

