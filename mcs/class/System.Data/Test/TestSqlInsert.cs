//
// TestSqlInsert.cs
//
// To Test SqlConnection and SqlCommand by connecting
// to a PostgreSQL database 
// and then executing an INSERT SQL statement
//
// To use:
//   change strings to your database, userid, tables, etc...:
//        connectionString
//        insertStatement
//
// To test:
//   mcs TestSqlInsert.cs -r System.Data
//   mint TestSqlInsert.exe
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Data.SqlClient;

namespace TestSystemDataSqlClient
{
	class TestSqlInsert
	{
		[STAThread]
		static void Main(string[] args)
		{
			SqlConnection conn;
			SqlCommand cmd;
			SqlTransaction trans;

			int rowsAffected;

			String connectionString;
			String insertStatement;
			String deleteStatement;
	
			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";

			insertStatement = 
				"insert into sometable " +
				"(tid, tdesc) " +
				"values ('beer', 'Beer for All!') ";

			deleteStatement = 
				"delete from sometable " +
				"where tid = 'beer' ";

			// Connect to a PostgreSQL database
			Console.WriteLine ("Connect to database...");
			conn = new SqlConnection(connectionString);
			conn.Open();

			// begin transaction
			Console.WriteLine ("Begin Transaction...");
			trans = conn.BeginTransaction();

			// create SQL DELETE command
			Console.WriteLine ("Create Command initializing " +
				"with an DELETE statement...");
			cmd = new SqlCommand (deleteStatement, conn);

			// execute the DELETE SQL command
			Console.WriteLine ("Execute DELETE SQL Command...");
			rowsAffected = cmd.ExecuteNonQuery();
			Console.WriteLine ("Rows Affected: " + rowsAffected);

			// change the SQL command to an SQL INSERT Command
			Console.WriteLine ("Now use INSERT SQL Command...");
			cmd.CommandText = insertStatement;

			// execute the INSERT SQL command
			Console.WriteLine ("Execute INSERT SQL Command...");
			rowsAffected = cmd.ExecuteNonQuery();
			Console.WriteLine ("Rows Affected: " + rowsAffected);

			// if successfull at INSERT, commit the transaction,
			// otherwise, do a rollback the transaction using
			// trans.Rollback();
			// FIXME: need to have exceptions working in
			//        SqlClient classes before you can do rollback
			Console.WriteLine ("Commit transaction...");
			trans.Commit();

			// Close connection to database
			Console.WriteLine ("Close database connection...");
			conn.Close();

			Console.WriteLine ("Assuming everything " +
				"was successful.");
			Console.WriteLine ("Verify data in database to " +
				"see if row is there.");
		}
	}
}
