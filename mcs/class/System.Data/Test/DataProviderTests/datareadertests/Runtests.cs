using System;
using System.Configuration;
using MonoTests.System.Data;

class RunTest {

	public static void Main(string [] args) 
	{

		string [] databases = null;
		if (args.Length == 0 || (args.Length == 1 && args [0].Equals ("all"))) {
			// Run test for all databases
			string listOfDbs = ConfigurationSettings.AppSettings ["Databases"];
			databases = listOfDbs.Split (';');
		} else {
			databases = (string []) args.Clone ();
		}
		
		BaseRetrieve dbRetrieve = null;

		foreach (string str in databases) {

			switch (str) {

			case "mysql" :
				Console.WriteLine ("\n ****** Running tests for MYSQL ***** \n");
				dbRetrieve = new MySqlRetrieve ("mysql");
				dbRetrieve.RunTest ();
				break;

			case "mssql" :
				Console.WriteLine ("\n ****** Running tests for MS SQL ***** \n");
				dbRetrieve = new MsSqlRetrieve ("mssql");
				dbRetrieve.RunTest ();
				Console.WriteLine ("\n ****** Running MS SQL - specific tests ***** \n");
				dbRetrieve = new SqlRetrieve ("mssql");
				dbRetrieve.RunTest ();
				break;

			case "oracle" :
				Console.WriteLine ("\n ****** Running tests for ORACLE ***** \n");
				dbRetrieve = new OraRetrieve ("oracle");
				dbRetrieve.RunTest ();
				break;

			case "postgres" :
				Console.WriteLine ("\n ****** Running tests for POSTGRE ***** \n");
				dbRetrieve = new PostgresRetrieve ("postgres");
				dbRetrieve.RunTest ();
				break;

			case "mysql-odbc" :
				Console.WriteLine ("\n ****** Running tests for MySql (using ODBC) ***** \n");
				dbRetrieve = new MySqlOdbcRetrieve ("mysql");
				dbRetrieve.RunTest ();
				break;

			case "mssql-odbc" :
				Console.WriteLine ("\n ****** Running tests for MsSql (using ODBC) ***** \n");
				dbRetrieve = new MsSqlOdbcRetrieve ("mssql");
				dbRetrieve.RunTest ();
				break;

			case "oracle-odbc" :
				Console.WriteLine ("\n ****** Running tests for Oracle (using ODBC) ***** \n");
				dbRetrieve = new OracleOdbcRetrieve ("oracle");
				dbRetrieve.RunTest ();
				break;

			case "postgres-odbc" :
				Console.WriteLine ("\n ****** Running tests for Postgres (using ODBC) ***** \n");
				dbRetrieve = new PostgreOdbcRetrieve ("postgres");
				dbRetrieve.RunTest ();
				break;
			}
		}
	}
}
