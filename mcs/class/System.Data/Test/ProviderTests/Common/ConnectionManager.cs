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


namespace MonoTests.System.Data
{
	public class ConnectionManager
	{
		private static ConnectionManager Instance;
		private DbConnection _connection;
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
				ConfigurationManager.GetSection ("providerTests");
			foreach (ConnectionConfig connConfig in connections) {
				if (connConfig.Name != connection_name)
					continue;

				_connectionString = connConfig.ConnectionString;
				DbProviderFactory factory = DbProviderFactories.GetFactory (
					connConfig.Factory);
				_connection = factory.CreateConnection ();
				_connection.ConnectionString = _connectionString;
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
		DbConnection
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
