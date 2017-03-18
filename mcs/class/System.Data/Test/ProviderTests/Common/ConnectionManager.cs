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
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using NUnit.Framework;


namespace MonoTests.System.Data.Connected
{
	public class ConnectionManager
	{
		private static ConnectionManager instance;
		private string databaseName;

		private ConnectionManager ()
		{
			//string envVariable = @"sqlserver-tds|server=EGORBO\SQLEXPRESS;database=master;user id=sa;password=qwerty123";
			//string envVariable = @"mysql-odbc|Driver={MySQL ODBC 5.2 Unicode Driver};server=127.0.0.1;uid=sa;pwd=qwerty123;";
			string envVariable = Environment.GetEnvironmentVariable ("SYSTEM_DATA_CONNECTIONSTRING") ?? string.Empty;

			var envParts = envVariable.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
			if (envParts.Length == 0 || string.IsNullOrEmpty(envParts[0]))
				Assert.Ignore($"SYSTEM_DATA_CONNECTIONSTRING environment variable is not set.");

			string connectionName = envParts[0];
			string connectionString = envVariable.Remove(0, envParts[0].Length + 1);

			var connections = (ConnectionConfig []) ConfigurationManager.GetSection ("providerTests");
			foreach (ConnectionConfig connConfig in connections)
			{
				if (connConfig.Name != connectionName)
					continue;

				DbProviderFactory factory = DbProviderFactories.GetFactory (connConfig.Factory);
				Connection = factory.CreateConnection ();
				Connection.ConnectionString = connectionString;
				ConnectionString = Connection.ConnectionString;
				Engine = connConfig.Engine;
				return;
			}

			throw new ArgumentException ("Connection '" + connectionName + "' not found.");
		}

		public static ConnectionManager Singleton => instance ?? (instance = new ConnectionManager());

		public string DatabaseName { get; set; }

		public string ConnectionString { get; set; }

		public DbConnection Connection { get; }

		internal EngineConfig Engine { get; }

		public DbConnection OpenConnection ()
		{
			if (!(Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken))
				Connection.Close ();
			Connection.ConnectionString = ConnectionString;
			Connection.Open ();
			return Connection;
		}

		public TDbConnection OpenConnection<TDbConnection>() where TDbConnection : DbConnection
		{
			return (TDbConnection) OpenConnection();
		}

		public void CloseConnection ()
		{
			if (Connection != null && Connection.State != ConnectionState.Closed)
				Connection.Close ();
		}

		public static void RequireProvider(ProviderType provder)
		{
			if (provder == ProviderType.SqlClient && 
				Singleton.Connection is SqlConnection)
				return;

			if (provder == ProviderType.Odbc && 
				Singleton.Connection is OdbcConnection)
				return;

			if (provder == ProviderType.Any && Singleton.Connection != null)
				return;

			Assert.Ignore($"Connection string is not provided for {provder}");
		}
	}
}
