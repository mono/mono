//
// TestSqlInsert.cs
//
// To Test MySqlConnection, MySqlCommand, and MySqlTransaction 
// by connecting to a MySQL database 
// and then executing some SQL statements
//
// To use:
//   change strings to your database, userid, tables, etc...:
//        connectionString
//        insertStatement
//        sqlToBeRolledBack
//
// To test:
//   mcs TestMySqlInsert.cs -r System.Data.dll -r Mono.Data.MySql.dll
//   mono TestMySqlInsert.exe
//
// Author:
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C)Copyright 2002 Daniel Morgan
//

using System;
using System.Data;
using Mono.Data.MySql;

namespace TestMonoDataMysql
{
	class TestMySqlInsert
	{
		[STAThread]
		static void Main(string[] args)
		{
			MySqlConnection conn;
			MySqlCommand cmd;
			MySqlTransaction trans;

			int rowsAffected;

			String connectionString;
			String insertStatement;
			String deleteStatement;
	
			connectionString = 
				"dbname=test";

			insertStatement = 
				"insert into sometable " +
				"(tid, tdesc) " +
				"values ('beer', 'Beer for All!') ";

			deleteStatement = 
				"delete from sometable " +
				"where tid = 'beer' ";

			// Connect to a MySQL database
			Console.WriteLine ("Connect to database...");
			conn = new MySqlConnection(connectionString);
			conn.Open();

			// begin transaction
			Console.WriteLine ("Begin Transaction...");
			trans = conn.BeginTransaction();

			// create SQL DELETE command
			Console.WriteLine ("Create Command initializing " +
				"with an DELETE statement...");
			cmd = new MySqlCommand (deleteStatement, conn);

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
			//        Mono.Data.MySql classes before you can do rollback
			Console.WriteLine ("Commit transaction...");
			trans.Commit();

			cmd = null;
			trans = null;

			string sqlToBeRolledBack = 
				"insert into sometable " +
				"(tid, tdesc) " +
				"values ('beer', 'Will not be committed!') ";

			Console.WriteLine("Create new command to be rolled back");
			cmd = conn.CreateCommand();
			cmd.CommandText = sqlToBeRolledBack;

			Console.WriteLine("Test Begin Transaction");
			trans = conn.BeginTransaction();

			Console.WriteLine("Execute INSERT SQL...");
			rowsAffected = cmd.ExecuteNonQuery();
			Console.WriteLine ("Rows Affected: " + rowsAffected);

			Console.WriteLine("Rollback Transaction...");
			trans.Rollback();

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
