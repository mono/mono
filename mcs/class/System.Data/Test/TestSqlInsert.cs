//
// System.Data.SqlClient.SqlError.cs
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
			String connectionString;
			String insertStatement;

			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=danmorg;" +
				"password=viewsonic";

			insertStatement = 
				"insert into sometable " +
				"(tid, tdesc) " +
				"values ('beer', 'Beer for All!') ";

			// Connect to a PostgreSQL database
			Console.WriteLine ("Connect to database...");
			conn = new SqlConnection(connectionString);

			// create SQL INSERT command
			Console.WriteLine ("Create SQL INSERT Command...");
			cmd = new SqlCommand (insertStatement, conn);

			// execute the SQL command
			Console.WriteLine ("Execute SQL Command...");
			cmd.ExecuteNonQuery();

			// Close connection to database
			Console.WriteLine ("Close database connection...");
			conn.Close();

			Console.WriteLine ("Assuming everything" +
				"was successful.");
			Console.WriteLine ("Verify data in database to " +
				"see if row is there.");
		}
	}
}
