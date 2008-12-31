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
#if NET_2_0
using System.Data.Common;
#endif

#if ONLY_1_1
using Mono.Data;
#endif

namespace MonoTests.System.Data
{
	public class ConnectionManager
	{
		private static ConnectionManager Instance;
#if NET_2_0
		private DbConnection _connection;
#else
		private IDbConnection _connection;
#endif
		private string _connectionString;
		private EngineConfig _engine;

		static ConnectionManager () 
		{
			Instance = new ConnectionManager ();
		}

		private ConnectionManager ()
		{
			string connection_name = Environment.GetEnvironmentVariable ("PROVIDER_TESTS_CONNECTION");
			if (connection_name == null || connection_name.Length == 0)
				throw new ArgumentException ("PROVIDER_TESTS_CONNECTION environment variable is not set.");

			ConnectionConfig [] connections = (ConnectionConfig [])
#if NET_2_0
				ConfigurationManager.GetSection ("providerTests");
#else
				ConfigurationSettings.GetConfig ("providerTests");
#endif
			foreach (ConnectionConfig connConfig in connections) {
				if (connConfig.Name != connection_name)
					continue;

				_connectionString = connConfig.ConnectionString;
#if NET_2_0
				DbProviderFactory factory = DbProviderFactories.GetFactory (
					connConfig.Factory);
				_connection = factory.CreateConnection ();
				_connection.ConnectionString = _connectionString;
#else
				_connection = ProviderFactory.CreateConnection (
					string.Concat ("factory=", connConfig.Factory,
						";", _connectionString));
#endif
				_connectionString = _connection.ConnectionString;
				_engine = connConfig.Engine;
				return;
			}

			throw new ArgumentException ("Connection '" + connection_name + "' not found.");
		}

		public static ConnectionManager Singleton {
			get {return Instance;}
		}

		public
#if NET_2_0
		DbConnection
#else
		IDbConnection
#endif
		Connection {
			get {return _connection;}
		}

		public string ConnectionString {
			get {return _connectionString;}
		}

		internal EngineConfig Engine {
			get { return _engine; }
		}

		public void OpenConnection ()
		{
			if (!(_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken))
				_connection.Close ();
			_connection.ConnectionString = _connectionString;
			_connection.Open ();
		}

		public void CloseConnection ()
		{
			if (_connection != null && _connection.State != ConnectionState.Closed)
				_connection.Close ();
		}
	}
}
