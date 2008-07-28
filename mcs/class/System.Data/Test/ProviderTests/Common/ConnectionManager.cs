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

using Mono.Data;

namespace MonoTests.System.Data
{
	public class ConnectionManager
	{
		private static ConnectionManager Instance;
		private IDbConnection _connection;
		private string _connectionString;

		static ConnectionManager () 
		{
			Instance = new ConnectionManager ();
		}

		private ConnectionManager ()
		{
			string connectionString = ConfigurationSettings.AppSettings ["ConnString"];
			if (connectionString == null || connectionString.Equals (String.Empty))
				throw new ArgumentException ("Connection string is not set!");
			_connection = ProviderFactory.CreateConnectionFromConfig ("ConnString");
			_connectionString = Connection.ConnectionString;
		}

		public static ConnectionManager Singleton {
			get {return Instance;}
		}

		public IDbConnection Connection {
			get {return _connection;}
		}

		public string ConnectionString {
			get {return _connectionString;}
		}

		public void OpenConnection ()
		{
			if (_connection == null)
				_connection = ProviderFactory.CreateConnectionFromConfig ("ConnString");
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
