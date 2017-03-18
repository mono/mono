using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	[SetUpFixture]
	public class SetupDb
	{
		[SetUp]
		public void CreateDatabase()
		{
			// generate a random db name
			string dbName = "monotest" + Guid.NewGuid().ToString().Substring(0, 7);
			var manager = ConnectionManager.Singleton;
			manager.OpenConnection();
			manager.DatabaseName = dbName;

			switch (manager.Engine.Type)
			{
				case EngineType.SQLServer:
					CreateMssqlDatabase(manager);
					break;
				case EngineType.MySQL:
					CreateMySqlDatabase(manager);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		[TearDown]
		public void DropDatabase()
		{
			var manager = ConnectionManager.Singleton;
			manager.OpenConnection();

			switch (manager.Engine.Type)
			{
				case EngineType.SQLServer:
					DropMssqlDatabase(manager);
					break;
				case EngineType.MySQL:
					DropMysqlDatabase(manager);
					break;
				default:
					throw new NotImplementedException();
			}
			manager.CloseConnection();
		}

		private void CreateMssqlDatabase(ConnectionManager manager)
		{
			var connection = manager.Connection;
			DBHelper.ExecuteNonQuery(connection, $"CREATE DATABASE [{manager.DatabaseName}]");
			connection.ChangeDatabase(manager.DatabaseName);

			string query = File.ReadAllText(@"sqlserver.sql");

			var queries = SplitSqlStatements(query);
			foreach (var subQuery in queries)
			{
				DBHelper.ExecuteNonQuery(connection, subQuery);
			}
		}

		private void DropMssqlDatabase(ConnectionManager manager)
		{
			manager.Connection.ChangeDatabase("master");
			string sql = $"ALTER DATABASE [{manager.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\nDROP DATABASE [{manager.DatabaseName}]";
			DBHelper.ExecuteNonQuery(manager.Connection, sql);
		}

		private void CreateMySqlDatabase(ConnectionManager manager)
		{
			var connection = manager.Connection;
			DBHelper.ExecuteNonQuery(connection, $"CREATE DATABASE {manager.DatabaseName}");
			connection.ChangeDatabase(manager.DatabaseName);
			manager.ConnectionString += $"database={manager.DatabaseName}";

			string query = File.ReadAllText(@"MySQL_5.sql");

			var groups = query.Replace("delimiter ", "")
				.Split(new[] {"//\n"}, StringSplitOptions.RemoveEmptyEntries);

			foreach (var subQuery in groups[0].Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries).Concat(groups.Skip(1)))
			{
				DBHelper.ExecuteNonQuery(connection, subQuery);
			}
		}

		private void DropMysqlDatabase(ConnectionManager manager)
		{
			string sql = $"DROP DATABASE [{manager.DatabaseName}]";
			DBHelper.ExecuteNonQuery(manager.Connection, sql);
		}

		// Split SQL script by "GO" statements
		private static IEnumerable<string> SplitSqlStatements(string sqlScript)
		{
			var statements = Regex.Split(sqlScript,
					$@"^[\t ]*GO[\t ]*\d*[\t ]*(?:--.*)?$",
					RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			return statements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(' ', '\r', '\n'));
		}
	}
}