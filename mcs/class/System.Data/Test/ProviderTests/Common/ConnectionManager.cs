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
		static ConnectionManager () 
		{
			Singleton = new ConnectionManager ();
		}

		private ConnectionManager ()
		{
			string connection_name = "sqlserver-tds";//"mysql-odbc";//Environment.GetEnvironmentVariable ("PROVIDER_TESTS_CONNECTION");
			if (string.IsNullOrEmpty(connection_name))
				Assert.Ignore($"PROVIDER_TESTS_CONNECTION environment variable is not set.");

			var connections = (ConnectionConfig []) ConfigurationManager.GetSection ("providerTests");
			foreach (ConnectionConfig connConfig in connections) {
				if (connConfig.Name != connection_name)
					continue;

				ConnectionString = connConfig.ConnectionString;
				DbProviderFactory factory = DbProviderFactories.GetFactory (
					connConfig.Factory);
				Connection = factory.CreateConnection ();
				Connection.ConnectionString = ConnectionString;
				ConnectionString = Connection.ConnectionString;
				Engine = connConfig.Engine;
				return;
			}

			throw new ArgumentException ("Connection '" + connection_name + "' not found.");
		}

		public static ConnectionManager Singleton { get; }

		public DbConnection Connection { get; }

		public string ConnectionString { get; }

		internal EngineConfig Engine { get; }

		public void OpenConnection ()
		{
			if (!(Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken))
				Connection.Close ();
			Connection.ConnectionString = ConnectionString;
			Connection.Open ();
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

			Assert.Ignore($"Connection string is not provided for {provder}");
		}
	}
}
