// testclob.cs - tests loading a text file into an oracle clob and vice-versa
using System;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using System.IO;

class TestClob 
{
	static string infilename = @"cs-parser.cs";
	static string outfilename = @"cs-parser2.cs";
	static string connectionString = "data source=palis;user id=scott;password=PLACEHOLDER"

	public static void Main (string[] args) 
	{
		OracleConnection con = new OracleConnection();
		con.ConnectionString = connectionString;
		con.Open();
		
		CLOBTest (con);
		ReadClob (con);
		
		con.Close();
		con = null;
	}

	// read the CLOB into file "cs-parser2.cs"
	public static void ReadClob (OracleConnection connection) 
	{
		OracleCommand rcmd = connection.CreateCommand ();
		rcmd.CommandText = "SELECT CLOB_COLUMN FROM CLOBTEST";
		OracleDataReader reader2 = rcmd.ExecuteReader ();
		if (!reader2.Read ())
			Console.WriteLine ("ERROR: RECORD NOT FOUND");

		Console.WriteLine ("  TESTING OracleLob OBJECT 2...");
		OracleLob lob2 = reader2.GetOracleLob (0);
		Console.WriteLine ("  LENGTH: {0}", lob2.Length);
		Console.WriteLine ("  CHUNK SIZE: {0}", lob2.ChunkSize);

		string lobvalue = (string) lob2.Value;
		
		using (StreamWriter sw = new StreamWriter(outfilename)) {
			sw.Write(lobvalue);
		}

		lob2.Close ();
		reader2.Close ();

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

		try {
			// read file "cs-parser.cs" into the oracle clob
			using (StreamReader sr = new StreamReader(infilename)) {
				string sbuff = sr.ReadToEnd ();
				byte[] evalue = encoding.GetBytes (sbuff);
				lob.Write (evalue, 0, evalue.Length);
			}
		}
		catch (Exception e) {
			Console.WriteLine("The file could not be read:");
			Console.WriteLine(e.Message);
		}
		lob.Close ();

		Console.WriteLine ("  CLOSING READER...");
			
		reader.Close ();
		transaction.Commit ();
	}
}