//
// TestSqlConnection.cs - tests connection via ServerName:
//   "Server=hostname"
//   "Server=hostname\\instance"
//   "Server=hostname,port"
//
// Test Connections for SqlClient, SybaseClient, and TdsClient
//
// Author: 
//      Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Daniel Morgan, 2003
//
// To build this test on Linux:
// mcs TestSqlConnection.cs -r System.Data.dll \
//     -r Mono.Data.SybaseClient.dll -r Mono.Data.TdsClient.dll
//
// To build this test on Windows via Cygwin:
// mono C:/cygwin/home/MyHome/mono/install/bin/mcs.exe TestSqlConnection.cs \
//      -lib:C:/cygwin/home/MyHome/mono/install/lib -r System.Data.dll \
//      -r Mono.Data.SybaseClient.dll -r Mono.Data.TdsClient.dll
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//#define IncludeSybaseAndTdsClient

using System;
using System.Data;
using System.Data.SqlClient;
#if IncludeSybaseAndTdsClient
	using Mono.Data.TdsClient;
	using Mono.Data.SybaseClient;
#endif // IncludeSybaseAndTdsClient

public class TestSqlConnection 
{
	public static void Main(string[] args) 
	{
		Console.WriteLine("Start TestSqlConnection.");
		if (args.Length != 6 && args.Length != 7) {
			Console.WriteLine(
				"\nUsage: mono TestSqlConnection.exe Client Table Column Server Database UserID [Password]\n\n" +
#if IncludeSybaseAndTdsClient
				"\tClient is one of the following: SqlClient, TdsClient, or SybaseClient\n" +
#else
				"\tClient is: SqlClient.  No support for TdsClient nor SybaseClient\n" +
#endif // IncludeSybaseAndTdsClient
				"\tTable is the name of the database table to select from\n" +
				"\tColumn is the name of the column in the Table to select from\n" +
				"\tServer is the SQL Server to connect.  Use one of the following forms:\n" +
				"\t\tHOSTNAME            Ex: MYHOST\n" +
				"\t\tHOSTNAME,port       Ex: MYHOST,1433\n" +
				"\t\tHOSTNAME\\\\instance  Ex: MYHOST\\\\NETSDK  Note: only works with SqlClient\n" +
				"\tDatabase is the name of the database to use\n" +
				"\tUser ID is the user's User ID\n" +
				"\tPassword is the user's Password   Note: if ommitted, a blank password is used\n" +
				"Exampes:\n" +
				"\tEx 1: SqlClient employee lname MYHOST pubs myuserid mypassword\n" +
				"\tEx 3: SqlClient employee lname MYHOST,1443 pubs myuserid mypassword\n" +
				"\tEx 2: SqlClient Products ProductName MYHOST\\\\NETSDK myuserid mypassword\n" +
				"\tEx 4: SqlClient employee lname MYHOST pubs myuserid\n" +
				"\tEx 5: TdsClient sometable somecolumn MYHOST test myuserid mypassword\n" +
				"\tEx 6: SybaseClient sometable somecolumn MYHOST test myuserid mypassword\n");

			return;
		}

		string client = args[0];
		string tableName = args[1];
		string columnName = args[2];
		
		string server = args[3];
		string database = args[4];
		string userid = args[5];
		string password = "";
		if (args.Length == 7)
			password  = args[6];
		
		string constr;
		string sql;

		Console.WriteLine("\nClient: " + client);
		Console.WriteLine("Table Name: " + tableName);
		Console.WriteLine("Column Name: " + columnName);
		Console.WriteLine("Server: " + server);
		Console.WriteLine("Database: " + database);
		Console.WriteLine("User ID: " + userid);
		Console.WriteLine("Password: " + password);

		sql = "SELECT " + columnName + " FROM " + tableName;
		
		constr = 
			"Server=" + server + ";" + 
			"Database=" + database + ";" +
			"User ID=" + userid + ";" +
			"Password=" + password + ";";	

		Console.WriteLine("\nConnectionString: " + constr);
		Console.WriteLine("SQL: " + sql);
		
		Console.WriteLine("\nCreating Connection...");

		IDbConnection con = null;
		switch (client.ToUpper()) {
		case "SQLCLIENT":
			con = new SqlConnection();
			break;
#if IncludeSybaseAndTdsClient
		case "TDSCLIENT":
			con = new TdsConnection();
			break;
		case "SYBASECLIENT":
			con = new SybaseConnection();
			break;
		default:
			Console.WriteLine("Invalid client: " + client + "\nUse SqlClient, TdsClient, or SybaseClient");
			return;
#else
		default:
			Console.WriteLine("Invalid client: " + client + "\nUse SqlClient.  No support for TdsClient nor SybaseClient.");
			return;

#endif
		}
		Console.WriteLine("set connection string...");
		con.ConnectionString = constr;
		Console.WriteLine("open connection...");
		try {
			con.Open();
		}
		catch(SqlException se) {
			Console.WriteLine("SqlException caught");
			Console.WriteLine("Message: " + se.Message);
			Console.WriteLine("Procedure: " + se.Procedure);
			Console.WriteLine("Class: " + se.Class);
			Console.WriteLine("Number: " + se.Number);
			Console.WriteLine("Source: " + se.Source);
			Console.WriteLine("State: " + se.State);
			Console.WriteLine("Errors:");
			foreach(SqlError error in se.Errors) {
				Console.WriteLine("  SqlError:");
				Console.WriteLine("     Message: " + se.Message);
				Console.WriteLine("     Line Number: " + se.LineNumber);
				Console.WriteLine("     Procedure: " + se.Procedure);
				Console.WriteLine("     Class: " + se.Class);
				Console.WriteLine("     Number: " + se.Number);
				Console.WriteLine("     Server: " + se.Server);
				Console.WriteLine("     Source: " + se.Source);
				Console.WriteLine("     State: " + se.State);
			}
			Console.WriteLine("StackTrace: " + se.StackTrace);
			Console.WriteLine("TargetSite: " + se.TargetSite);
			Exception ie = se.InnerException;
			if(ie != null) {
				Console.WriteLine("InnerException:");
				Console.WriteLine("   Message: " + se.Message);
				Console.WriteLine("   Class: " + se.Class);
				Console.WriteLine("   Number: " + se.Number);
				Console.WriteLine("   Source: " + se.Source);
				Console.WriteLine("   State: " + se.State);
				Console.WriteLine("   StackTrace: " + se.StackTrace);
				Console.WriteLine("   TargetSite: " + se.TargetSite);
			}
			return;
		}
		Console.WriteLine("Creating command...");
		IDbCommand cmd = con.CreateCommand();
		Console.WriteLine("set SQL...");
		cmd.CommandText = sql;
		Console.WriteLine("execute reader...");
		IDataReader reader = cmd.ExecuteReader();
		Console.WriteLine("read first row...");
		if(reader.Read()) {
			Console.WriteLine("  Value: " + reader[columnName].ToString());
		}
		else {
			Console.WriteLine("  No data returned.  Or either, no permission to read data.");
		}

		Console.WriteLine("Clean up...");
		// clean up
		reader.Close();
		reader = null;
		cmd.Dispose();
		cmd = null;
		con.Close();
		con = null;
		Console.WriteLine("Done.");
	}
}

