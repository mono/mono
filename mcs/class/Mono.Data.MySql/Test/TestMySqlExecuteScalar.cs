//
// TestSqlExecuteScalar.cs
//
// To Test MySqlConnection and MySqlCommand by connecting
// to a MySQL database 
// and then executing an SELECT SQL statement
// using ExecuteScalar
//
// To use:
//   change strings to your database, userid, tables, etc...:
//        connectionString
//        selectStatement
//
// To test:
//   mcs TestMySqlExecuteScalar.cs -r System.Data.dll -r Mono.Data.MySql.dll
//   mono TestMySqlExecuteScalar.exe
//
// Author:
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C)Copyright 2002 Daniel Morgan
//

using System;
using System.Data;
using Mono.Data.MySql;

namespace TestMonoDataMysql {
	class TestMySqlInsert {
		[STAThread]
		static void Main(string[] args) {
			MySqlConnection conn;
			MySqlCommand cmd;

			String connectionString;
			String selectStatement;
				
			connectionString = 
				"dbname=test";

			selectStatement = 
				"select count(*)" +
				"from sometable"; 
				
			// Connect to a MySQL database
			Console.WriteLine ("Connect to database...");
			conn = new MySqlConnection(connectionString);
			conn.Open();

			// create SELECT command
			Console.WriteLine ("Create Command initializing " +
				"with an SELECT statement...");
			cmd = new MySqlCommand (selectStatement, conn);

			// execute the SELECT SQL command
			Console.WriteLine ("Execute SELECT SQL Command...");
			Object obj = cmd.ExecuteScalar();
			Console.WriteLine ("Object: " + obj.ToString());

			// Close connection to database
			Console.WriteLine ("Close database connection...");
			conn.Close();

			Console.WriteLine ("Assuming everything " +
				"was successful.");
		}
	}
}
