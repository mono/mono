//
// SqliteTest.cs - Test for the Sqlite ADO.NET Provider in Mono.Data.SqliteClient
//                 This provider works on Linux and Windows and uses the native
//                 sqlite.dll or sqlite.so library.
//
// Modify or add to this test as needed...
//
// SQL Lite can be downloaded from
// http://www.hwaci.com/sw/sqlite/download.html
//
// There are binaries for Windows and Linux.
//
// To compile:
//  mcs SqliteTest.cs -r System.Data.dll -r Mono.Data.SqliteClient.dll
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//

using System;
using System.Data;
using Mono.Data.SqliteClient;

namespace Test.Mono.Data.SqliteClient
{
	class SqliteTest
	{
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("If this test works, you should get:");
			Console.WriteLine("Data 1: 5");
			Console.WriteLine("Data 2: Mono");

			Console.WriteLine("create SqliteConnection...");
			SqliteConnection dbcon = new SqliteConnection();
			
			// the connection string is a URL that points
			// to a file.  If the file does not exist, a 
			// file is created.

			// "URI=file:some/path"
			string connectionString =
				"URI=file:SqliteTest.db";
			Console.WriteLine("setting ConnectionString using: " + 
				connectionString);
			dbcon.ConnectionString = connectionString;
				
			Console.WriteLine("open the connection...");
			dbcon.Open();

			Console.WriteLine("create SqliteCommand to CREATE TABLE MONO_TEST");
			SqliteCommand dbcmd = new SqliteCommand();
			dbcmd.Connection = dbcon;
			
			dbcmd.CommandText = 
				"CREATE TABLE MONO_TEST ( " +
				"NID INT, " +
				"NDESC TEXT )";
			Console.WriteLine("execute command...");
			dbcmd.ExecuteNonQuery();

			Console.WriteLine("set and execute command to INSERT INTO MONO_TEST");
			dbcmd.CommandText =
				"INSERT INTO MONO_TEST  " +
				"(NID, NDESC )"+
				"VALUES(5,'Mono')";
			dbcmd.ExecuteNonQuery();

			Console.WriteLine("set command to SELECT FROM MONO_TEST");
			dbcmd.CommandText =
				"SELECT * FROM MONO_TEST";
			SqliteDataReader reader;
			Console.WriteLine("execute reader...");
			reader = dbcmd.ExecuteReader();

			Console.WriteLine("read and display data...");
			while(reader.Read()) {
				Console.WriteLine("Data 1: " + reader[0].ToString());
				Console.WriteLine("Data 2: " + reader[1].ToString());
			}

			Console.WriteLine("read and display data using DataAdapter...");
			SqliteDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM MONO_TEST", connectionString);
			DataSet dataset = new DataSet();
			adapter.Fill(dataset);
			foreach(DataTable myTable in dataset.Tables){
				foreach(DataRow myRow in myTable.Rows){
					foreach (DataColumn myColumn in myTable.Columns){
						Console.WriteLine(myRow[myColumn]);
					}
				}
			}

			
			Console.WriteLine("clean up...");
			dataset.Dispose();
			adapter.Dispose();
			reader.Close();
			dbcmd.Dispose();
			dbcon.Close();

			Console.WriteLine("Done.");
		}
	}
}
