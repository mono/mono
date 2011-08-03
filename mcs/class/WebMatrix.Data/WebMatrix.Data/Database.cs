//
// Database.cs
//
// Copyright (c) 2011 Novell
//
// Authors:
//     Jérémie "garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

#if NET_4_0

using System;
using System.Linq;
using System.Threading;
using System.Dynamic;
using System.Data.Common;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;

namespace WebMatrix.Data
{
	public class Database : IDisposable
	{
		public static event EventHandler<ConnectionEventArgs> ConnectionOpened;

		bool opened;
		DbConnection connection;
		readonly string providerName;

		static readonly Dictionary<string, string> lastInsertedIdStmts = new Dictionary<string, string> ();

		static Database ()
		{
			// Add your own DB-specific way to do so
			lastInsertedIdStmts.Add ("System.Data.SqlClient", "select @@IDENTITY;");
			lastInsertedIdStmts.Add ("Mono.Data.Sqlite", "select last_insert_rowid ();");
			lastInsertedIdStmts.Add ("MySql.Data", "SELECT LAST_INSERT_ID();");
			lastInsertedIdStmts.Add ("Npgsql", "SELECT lastval();");
		}

		private Database (DbConnection connection, string providerName)
		{
			this.connection = connection;
			this.providerName = providerName;
		}

		public static Database Open (string name)
		{
			var config = ConfigurationManager.ConnectionStrings[name];
			if (config == null)
				throw new ArgumentException ("name", string.Format ("Database with name {0} doesn't exist", name));

			return OpenConnectionString (config.ConnectionString, config.ProviderName);
		}

		public static Database OpenConnectionString (string connectionString)
		{
			return OpenConnectionString (connectionString, "System.Data.SqlClient");
		}

		public static Database OpenConnectionString (string connectionString, string providerName)
		{
			var factory = DbProviderFactories.GetFactory (providerName);
			var conn = factory.CreateConnection ();
			conn.ConnectionString = connectionString;

			return new Database (conn, providerName);
		}

		public void Close ()
		{
			opened = false;
			connection.Close ();
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (opened)
					connection.Close ();
				connection.Dispose ();
			}
		}

		public int Execute (string commandText, params object[] args)
		{
			var command = PrepareCommand (commandText);
			PrepareCommandParameters (command, args);

			EnsureConnectionOpened ();

			var result = command.ExecuteNonQuery ();

			command.Dispose ();

			return result;
		}

		public IEnumerable<dynamic> Query (string commandText, params object[] args)
		{
			var result = QueryInternal (commandText, args, false);

			return result != null ? result.Select (r => new DynamicRecord (r)) : null;
		}

		public dynamic QuerySingle (string commandText, params object[] args)
		{
			var result = QueryInternal (commandText, args, true);

			return result != null ? new DynamicRecord (result[0]) : null;
		}

		List<Dictionary<string, object>> QueryInternal (string commandText, object[] args, bool unique)
		{
			EnsureConnectionOpened ();

			var command = PrepareCommand (commandText);
			PrepareCommandParameters (command, args);
			string[] columnsNames;
			var rows = new List<Dictionary<string, object>> ();

			using (var reader = command.ExecuteReader ()) {
				if (!reader.Read () || !reader.HasRows)
					return null;

				columnsNames = new string [reader.FieldCount];

				do {				
					var fields = new Dictionary<string, object> ();

					for (int i = 0; i < reader.FieldCount; ++i) {
						if (columnsNames[i] == null)
							columnsNames[i] = reader.GetName (i);

						fields[columnsNames[i]] = reader[i];
					}

					rows.Add (fields);
				} while (!unique && reader.Read ());
			}

			command.Dispose ();

			return rows;
		}

		public object QueryValue (string commandText, params object[] args)
		{
			EnsureConnectionOpened ();

			var command = PrepareCommand (commandText);
			PrepareCommandParameters (command, args);

			var result = command.ExecuteScalar ();

			command.Dispose ();

			return result;
		}

		public object GetLastInsertId ()
		{
			string sql;
			if (!lastInsertedIdStmts.TryGetValue (providerName, out sql))
				throw new NotSupportedException ("This operation is not available for your database");

			return QueryValue (sql);
		}

		DbCommand PrepareCommand (string commandText)
		{
			var command = connection.CreateCommand ();
			command.CommandText = commandText;

			return command;
		}

		static void PrepareCommandParameters (DbCommand command, object[] args)
		{
			if (args.Length == 0)
				return;

			int index = 0;

			foreach (var arg in args) {
				var param = command.CreateParameter ();
				param.ParameterName = "@" + index;
				param.Value = args[index++];
				command.Parameters.Add (param);
			}
		}

		static void TriggerConnectionOpened (Database self, DbConnection connection)
		{
			EventHandler<ConnectionEventArgs> evt = ConnectionOpened;
			if (evt != null)
				evt (self, new ConnectionEventArgs (connection));
		}

		void EnsureConnectionOpened ()
		{
			if (opened)
				return;

			connection.Open ();
			opened = true;
			TriggerConnectionOpened (this, connection);
		}

		public DbConnection Connection {
			get {
				return connection;
			}
		}		
	}
}

#endif