//
// TestPgSqlInsert.cs
//
// To Test PgSqlConnection and PgSqlCommand by connecting
// to a PostgreSQL database 
// and then executing an INSERT SQL statement
//
// To use:
//   change strings to your database, userid, tables, etc...:
//        connectionString
//        insertStatement
//
// To test:
//   mcs TestPgSqlInsert.cs -r System.Data
//   mint TestPgSqlInsert.exe
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using Mono.Data.PostgreSqlClient;

namespace TestSystemDataPgSqlClient
{
	class TestPgSqlInsert
	{
		[STAThread]
		static void Main(string[] args) {
			PgSqlConnection conn = null;
			PgSqlCommand cmd = null;
			PgSqlTransaction trans = null;

			int rowsAffected = -1;

			String connectionString = "";
			String insertStatement = "";
			String deleteStatement = "";
	
			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";

			insertStatement = 
				"insert into NoSuchTable " +
				"(tid, tdesc) " +
				"values ('beer', 'Beer for All!') ";

			deleteStatement = 
				"delete from sometable " +
				"where tid = 'beer' ";

			try {
				// Connect to a PostgreSQL database
				Console.WriteLine ("Connect to database...");
				conn = new PgSqlConnection(connectionString);
				conn.Open();
			
				// begin transaction
				Console.WriteLine ("Begin Transaction...");
				trans = conn.BeginTransaction();

				// create SQL DELETE command
				Console.WriteLine ("Create Command initializing " +
					"with an DELETE statement...");
				cmd = new PgSqlCommand (deleteStatement, conn);

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
			catch(PgSqlException e) {
				// Display the SQL Errors and Rollback the database
				Console.WriteLine("PgSqlException caught: " +
					e.ToString());
				if(trans != null) {
					trans.Rollback();
					Console.WriteLine("Database has been Rolled back!");
				}
			}
			finally {
				if(conn != null)
					if(conn.State == ConnectionState.Open)
						conn.Close();
			}
		}
	}
}
