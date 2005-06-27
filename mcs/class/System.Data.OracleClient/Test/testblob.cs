// testblob.cs - tests loading a binary file into an oracle blob and vice-versa
using System;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using System.IO;

class TestBlob 
{
	static string infilename = @"mono-win32-setup-dark.bmp";
	static string outfilename = @"mono-win32-setup-dark2.bmp";
	static string connectionString = "Data Source=palis;User ID=scott;Password=tiger";

	public static void Main (string[] args) 
	{
		OracleConnection con = new OracleConnection();
		con.ConnectionString = connectionString;
		con.Open();

		BLOBTest (con);
		ReadBlob (con);
		
		con.Close();
		con = null;
	}

	// read the BLOB into file "cs-parser2.cs"
	public static void ReadBlob (OracleConnection connection) 
	{
		if (File.Exists(outfilename) == true) {
			Console.WriteLine("Filename already exists: " + outfilename);
			return;
		}

		OracleCommand rcmd = connection.CreateCommand ();
		rcmd.CommandText = "SELECT BLOB_COLUMN FROM BLOBTEST";
		OracleDataReader reader2 = rcmd.ExecuteReader ();
		if (!reader2.Read ())
			Console.WriteLine ("ERROR: RECORD NOT FOUND");

		Console.WriteLine ("  TESTING OracleLob OBJECT 2...");
		OracleLob lob2 = reader2.GetOracleLob (0);
		Console.WriteLine ("  LENGTH: {0}", lob2.Length);
		Console.WriteLine ("  CHUNK SIZE: {0}", lob2.ChunkSize);

		byte[] lobvalue = (byte[]) lob2.Value;

		FileStream fs = new FileStream(outfilename, FileMode.CreateNew);
		BinaryWriter w = new BinaryWriter(fs);
		w.Write(lobvalue);
		w.Close();
		fs.Close();

		lob2.Close ();
		reader2.Close ();
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
		Console.WriteLine ("  SELECTING A BLOB (Binary Large Object) VALUE FROM BLOBTEST");

		OracleDataReader reader = select.ExecuteReader ();
		if (!reader.Read ())
			Console.WriteLine ("ERROR: RECORD NOT FOUND");

		Console.WriteLine ("  TESTING OracleLob OBJECT ...");
		OracleLob lob = reader.GetOracleLob (0);
		Console.WriteLine ("  LENGTH: {0}", lob.Length);
		Console.WriteLine ("  CHUNK SIZE: {0}", lob.ChunkSize);

		try {
			if (File.Exists(infilename) == false) {
				Console.WriteLine("Filename does not exist: " + infilename);
				return;
			}

			FileStream fs = new FileStream(infilename, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			
			byte[] bytes = null;
			int bufferLen = 8192;
			bytes = r.ReadBytes (bufferLen);

			while(bytes.Length > 0) {
				Console.WriteLine("byte count: " + bytes.Length.ToString());
				lob.Write (bytes, 0, bytes.Length);
				if (bytes.Length < bufferLen)
					break;
				bytes = r.ReadBytes (bufferLen);
			}

			r.Close();
			fs.Close ();	
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
