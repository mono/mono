// ConnectionManager.cs - Singleton ConnectionManager class to manage
// database connections for test cases.
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
#if !NO_ODBC
using System.Data.Odbc;
#endif
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data.Connected
{
	public class ConnectionManager
	{
		private static ConnectionManager instance;
		private ConnectionHolder<SqlConnection> sql;

		private const string OdbcEnvVar = "SYSTEM_DATA_ODBC_V3";
		private const string SqlEnvVar = "SYSTEM_DATA_MSSQL_V3";

		private ConnectionManager ()
		{
			//Environment.SetEnvironmentVariable(OdbcEnvVar, @"Driver={MySQL ODBC 5.3 Unicode Driver};server=127.0.0.1;uid=sa;pwd=qwerty123;");
			//Environment.SetEnvironmentVariable(SqlEnvVar, @"Server=.\SQLEXPRESS;Database=master;User Id=sa;Password=qwerty123;");

			// Generate a random db name
			DatabaseName = "monotest" + Guid.NewGuid().ToString().Substring(0, 7);

			sql = CreateSqlConfig (SqlEnvVar);
			if (sql != null)
				CreateMssqlDatabase();
			
#if !NO_ODBC
			odbc = CreateOdbcConfig (OdbcEnvVar);
			if (odbc != null)
				CreateMysqlDatabase();
#endif
		}

		static ConnectionHolder<SqlConnection> CreateSqlConfig (string envVarName)
		{
			string connectionString = Environment.GetEnvironmentVariable (envVarName);
			if (string.IsNullOrEmpty (connectionString))
				return null;

			SqlConnection connection;
#if MOBILE
			connection = new SqlConnection ();
#else
			DbProviderFactory factory = DbProviderFactories.GetFactory ("System.Data.SqlClient");
			connection = (SqlConnection)factory.CreateConnection ();
#endif

			var engine = new EngineConfig {
				Type = EngineType.SQLServer,
				ClientVersion = 9,
				QuoteCharacter = "&quot;",
				SupportsMicroseconds = true,
				SupportsUniqueIdentifier = true,
				SupportsTimestamp = true,
			};

			return new ConnectionHolder<SqlConnection> (engine, connection, connectionString);
		}

#if !NO_ODBC
		static ConnectionHolder<OdbcConnection> CreateOdbcConfig (string envVarName)
		{
			string connectionString = Environment.GetEnvironmentVariable (envVarName);
			if (string.IsNullOrEmpty (connectionString))
				return null;
#if MOBILE
			connection = new OdbcConnection ();
#else
			DbProviderFactory factory = DbProviderFactories.GetFactory ("System.Data.Odbc");
			var connection = (OdbcConnection)factory.CreateConnection ();
#endif

			var engine = new EngineConfig {
				Type = EngineType.MySQL,
				QuoteCharacter = "`",
				RemovesTrailingSpaces = true,
				EmptyBinaryAsNull = true,
				SupportsDate = true,
				SupportsTime = true
			};

			return new ConnectionHolder<OdbcConnection> (engine, connection, connectionString);
		}
#endif

		private void CreateMssqlDatabase()
		{
			DBHelper.ExecuteNonQuery(sql.Connection, $"CREATE DATABASE [{DatabaseName}]");
			sql.ConnectionString = sql.ConnectionString.Replace(sql.Connection.Database, DatabaseName);
			sql.CloseConnection();

			string query = File.ReadAllText(TestResourceHelper.GetFullPathOfResource ("Test/ProviderTests/sql/sqlserver.sql"));

			var queries = SplitSqlStatements(query);
			foreach (var subQuery in queries)
			{
				DBHelper.ExecuteNonQuery(sql.Connection, subQuery);
			}
		}

#if !NO_ODBC
		private void CreateMysqlDatabase()
		{
			DBHelper.ExecuteNonQuery(odbc.Connection, $"CREATE DATABASE {DatabaseName}");
			odbc.Connection.ChangeDatabase(DatabaseName);
			odbc.ConnectionString += $"database={DatabaseName}";

			string query = File.ReadAllText(TestResourceHelper.GetFullPathOfResource ("Test/ProviderTests/sql/MySQL_5.sql"));

			var groups = query.Replace("delimiter ", "")
				.Split(new[] { "//\n" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var subQuery in groups[0].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Concat(groups.Skip(1)))
			{
				DBHelper.ExecuteNonQuery(odbc.Connection, subQuery);
			}
		}
#endif

		private void DropMssqlDatabase()
		{
			sql.Connection.ChangeDatabase("master");
			string query = $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\nDROP DATABASE [{DatabaseName}]";
			DBHelper.ExecuteNonQuery(sql.Connection, query);
		}

#if !NO_ODBC
		private void DropMysqlDatabase()
		{
			string query = $"DROP DATABASE [{DatabaseName}]";
			DBHelper.ExecuteNonQuery(odbc.Connection, query);
		}
#endif

		// Split SQL script by "GO" statements
		private static IEnumerable<string> SplitSqlStatements(string sqlScript)
		{
			var statements = Regex.Split(sqlScript,	@"^\s*GO.*$",
					RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			return statements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(' ', '\r', '\n'));
		}

		public static ConnectionManager Instance => instance ?? (instance = new ConnectionManager());

		public string DatabaseName { get; }

#if !NO_ODBC

		private ConnectionHolder<OdbcConnection> odbc;

		public ConnectionHolder<OdbcConnection> Odbc
		{
			get
			{
				if (odbc == null)
					Assert.Ignore($"{OdbcEnvVar} environment variable is not set");
				return odbc;
			}
		}
#endif

		public ConnectionHolder<SqlConnection> Sql
		{
			get
			{
				if (sql == null)
					Assert.Ignore($"{SqlEnvVar} environment variable is not set");
				return sql;
			}
		}

		public void Close()
		{
			sql?.CloseConnection();
#if !NO_ODBC			
			odbc?.CloseConnection();
#endif
		}
	}

	public class ConnectionHolder<TConnection> where TConnection : DbConnection
	{
		private TConnection connection;

		public EngineConfig EngineConfig { get; }

		public TConnection Connection
		{
			get
			{
				if (!(connection.State == ConnectionState.Closed || 
					connection.State == ConnectionState.Broken))
					connection.Close();
				connection.ConnectionString = ConnectionString;
				connection.Open();
				return connection;
			}
		}

		public void CloseConnection()
		{
			if (connection != null && connection.State != ConnectionState.Closed)
				connection.Close();
		}

		public string ConnectionString { get; set; }

		public ConnectionHolder(EngineConfig engineConfig, TConnection connection, string connectionString)
		{
			EngineConfig = engineConfig;
			this.connection = connection;
			ConnectionString = connectionString;
			if (!ConnectionString.EndsWith(";"))
				ConnectionString += ";";
		}

		public bool IsAzure => ConnectionString.ToLower().Contains("database.windows.net");
	}
}
