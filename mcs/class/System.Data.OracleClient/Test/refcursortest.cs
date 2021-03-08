 using System;
 using System.Data;
 using System.Data.OracleClient;
 
 public class Test
 {
    public static void Main (string[] args)
    {
        string connectionString =
          "Data Source=testdb;" +
          "User ID=scott;" +
          "Password=PLACEHOLDER;";
        OracleConnection connection = null;
        connection = new OracleConnection (connectionString);
        connection.Open ();
 
	Console.WriteLine("Setup test package and data...");
	OracleCommand cmddrop = connection.CreateCommand();
 
	cmddrop.CommandText = "DROP TABLE TESTTABLE";
	try { 
		cmddrop.ExecuteNonQuery(); 
	} 
	catch(OracleException e) {
		Console.WriteLine("Ignore this error: " + e.Message); 
	}
	cmddrop.Dispose();
	cmddrop = null;

Console.WriteLine("Create table TESTTABLE..."); 
	OracleCommand cmd = connection.CreateCommand();
 
	// create table TESTTABLE
	cmd.CommandText = 
		"create table TESTTABLE (\n" +
		" col1 numeric(18,0),\n" +
		" col2 varchar(32),\n" +
		" col3 date, col4 blob)";

	cmd.ExecuteNonQuery();
 Console.WriteLine("Insert 3 rows...");
	// insert some rows into TESTTABLE
	cmd.CommandText = 
		"insert into TESTTABLE\n" +
		"(col1, col2, col3, col4)\n" +
		"values(45, 'Mono', sysdate, EMPTY_BLOB())";
	cmd.ExecuteNonQuery();
 
	cmd.CommandText = 
		"insert into TESTTABLE\n" +
		"(col1, col2, col3, col4)\n" +
		"values(136, 'Fun', sysdate, EMPTY_BLOB())";
	cmd.ExecuteNonQuery();
 
	cmd.CommandText = 
		"insert into TESTTABLE\n" +
		"(col1, col2, col3, col4)\n" +
		"values(526, 'System.Data.OracleClient', sysdate, EMPTY_BLOB())";
	cmd.ExecuteNonQuery();

Console.WriteLine("commit...");

	cmd.CommandText = "commit";
	cmd.ExecuteNonQuery();

Console.WriteLine("Update blob...");

			// update BLOB and CLOB columns
			OracleCommand select = connection.CreateCommand ();
			select.Transaction = connection.BeginTransaction();
			select.CommandText = "SELECT col1, col4 FROM testtable FOR UPDATE";
			OracleDataReader readerz = select.ExecuteReader ();
			if (!readerz.Read ())
				Console.WriteLine ("ERROR: RECORD NOT FOUND");
			// update blob_value
			Console.WriteLine("     Update BLOB column on table testtable...");
			OracleLob blob = readerz.GetOracleLob (1);
			byte[] bytes = new byte[6] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x036 };
			blob.Write (bytes, 0, bytes.Length);
			blob.Close ();
			readerz.Close();
			select.Transaction.Commit();
			select.Dispose();
			select = null;
			
 
	cmd.CommandText = "commit";
	cmd.ExecuteNonQuery();

Console.WriteLine("Create package...");
 
	// create Oracle package TestTablePkg
	cmd.CommandText = 
		"CREATE OR REPLACE PACKAGE TestTablePkg\n" +
		"AS\n" +
		"	TYPE T_CURSOR IS REF CURSOR;\n" +
		"\n" +
		"	PROCEDURE GetData(tableCursor OUT T_CURSOR);\n" +
		"END TestTablePkg;";
	cmd.ExecuteNonQuery();
 
	// create Oracle package body for package TestTablePkg
	cmd.CommandText = 
		"CREATE OR REPLACE PACKAGE BODY TestTablePkg AS\n" +
		"  PROCEDURE GetData(tableCursor OUT T_CURSOR)\n" +	
                "  IS\n" +
		"  BEGIN\n" +
		"    OPEN tableCursor FOR\n" +
		"    SELECT *\n" +
		"    FROM TestTable;\n" +
		"  END GetData;\n" +
		"END TestTablePkg;";
	cmd.ExecuteNonQuery();
 
	cmd.Dispose();
	cmd = null;
 
	Console.WriteLine("Set up command and parameters to call stored proc...");
	OracleCommand command = new OracleCommand("TestTablePkg.GetData", connection);
	command.CommandType = CommandType.StoredProcedure;
	OracleParameter parameter = new OracleParameter("tableCursor", OracleType.Cursor);
	parameter.Direction = ParameterDirection.Output;
	command.Parameters.Add(parameter);
 
	Console.WriteLine("Execute...");
	command.ExecuteNonQuery();
 
	Console.WriteLine("Get OracleDataReader for cursor output parameter...");
	OracleDataReader reader = (OracleDataReader) parameter.Value;
			
	Console.WriteLine("Read data***...");
	int r = 0;
	while (reader.Read()) {
		Console.WriteLine("Row {0}", r);
		for (int f = 0; f < reader.FieldCount; f ++) {
			Console.WriteLine("FieldType: " + reader.GetFieldType(f).ToString());
			object val = ""; 
			if (f==3) {
				Console.WriteLine("blob");
				//OracleLob lob = reader.GetOracleLob (f);
				//val = lob.Value;
				val = reader.GetValue(f);
				if (((byte[])val).Length == 0)
					val = "Empty Blob (Not Null)";
				else
					val = BitConverter.ToString((byte[])val);
			}
			else
				val = reader.GetOracleValue(f);
			
			Console.WriteLine("    Field {0} Value: {1}", f, val);
		}
		r ++;
	}
	Console.WriteLine("Rows retrieved: {0}", r);
 
	Console.WriteLine("Clean up...");
	reader.Close();
	reader = null;
	command.Dispose();
	command = null;
 
        connection.Close();
        connection = null;
    }
 }



