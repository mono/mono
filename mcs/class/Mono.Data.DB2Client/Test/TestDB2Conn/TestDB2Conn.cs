#region Licence
	/// DB2DriverCS Test Code - A DB2 driver test for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
	/// 
#endregion

using System;
using System.Data;
using DB2ClientCS;
using System.Text;

namespace TestDB2Conn {
	/// <summary>
	/// Code to test DB2DriverCS.
	/// </summary>
	class TestDB2Client 
	{
		static void DoTest(string database, string userid, string password) 
		{
			IDbCommand command = null;
			string connectionString = String.Format(
				"DSN={0};UID={1};PWD={2}",
				database, userid, password);
			try {
				Console.WriteLine("Create DB2 client Connection...");
				DB2ClientConnection DB2Conn = new DB2ClientConnection();
				
				Console.WriteLine("connection string: " + connectionString);
				
				Console.WriteLine("Set connection string...");
				DB2Conn.ConnectionString = connectionString;
				
				Console.WriteLine("Open a connection...");
				DB2Conn.Open();
				
				string createTestTableSQL = 
					"CREATE TABLE mono_db2_test1 ( " +
					"   testid varchar(2), " +
					"   testdesc varchar(16) " +
					")";
				Console.WriteLine("SQL:\n" + createTestTableSQL);
				
				Console.WriteLine("Create a command using sql and connection...");
				command = new DB2ClientCommand(createTestTableSQL,DB2Conn);
				Console.WriteLine("Execute Non Query...");
				command.ExecuteNonQuery();

				string selectSQL = "select * from employee";
				Console.WriteLine("SQL:\n" + selectSQL);
				Console.WriteLine("create command and set connection and connection string...");
				command = new DB2ClientCommand(selectSQL,DB2Conn);
				Console.WriteLine("ExecuteReader...");
				IDataReader dr = command.ExecuteReader();
				Console.WriteLine("Read row...");
				dr.Read();
				Console.WriteLine("Read row...");
				dr.Read();
				Console.WriteLine("GetString...");
				string dt = dr.GetString(1);
				Console.WriteLine("dt: " + dt);
				string s = dr.GetString(5);
				Console.WriteLine("s: " + s);
				DateTime t = dr.GetDateTime(6);
				Console.WriteLine("t: " + t);
				
				Console.WriteLine("Close connection...");
				DB2Conn.Close();
			}
			catch(DB2ClientException e) {
				System.Console.Write(e.Message);
			}
		}
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			if(args.Length != 3)
				Console.WriteLine("Usage: mono TestDB2Conn.exe database userid password");
			else {
				Console.WriteLine("Test Begin.");
				// database, userid, password
				DoTest(args[0], args[1], args[2]);
				Console.WriteLine("Test End.");
			}			
		}
	}
}
