//
// Mono.Data.SqliteClient.SqliteConnection.cs
//
// Represents an open connection to a Sqlite database file.
//
// Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//            Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//
// Copyright (C) 2002  Vladimir Vukicevic
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using System.Data;

namespace Mono.Data.SqliteClient
{
	public class SqliteConnection : IDbConnection
	{

		#region Fields
		
		private string conn_str;
		private string db_file;
		private int db_mode;
		private int db_version;
		private IntPtr sqlite_handle;
		private ConnectionState state;
		
		#endregion

		#region Constructors and destructors
		
		public SqliteConnection ()
		{
			db_file = null;
			db_mode = 0644;
			db_version = 3;
			state = ConnectionState.Closed;
			sqlite_handle = IntPtr.Zero;
		}
		
		public SqliteConnection (string connstring) : this ()
		{
			ConnectionString = connstring;
		}
		
		public void Dispose ()
		{
			Close ();
		}
		                
		#endregion

		#region Properties
		
		public string ConnectionString {
			get { return conn_str; }
			set { SetConnectionString(value); }
		}
		
		public int ConnectionTimeout {
			get { return 0; }
		}
		
		public string Database {
			get { return db_file; }
		}
		
		public ConnectionState State {
			get { return state; }
		}
		
		internal int Version {
			get { return db_version; }
		}

		internal IntPtr Handle {
			get { return sqlite_handle; }
		}
		
		public int LastInsertRowId {
			get {
				if (Version == 3)
					return Sqlite.sqlite3_last_insert_rowid (Handle);
				else
					return Sqlite.sqlite_last_insert_rowid (Handle);
			}
		}
		
		#endregion

		#region Private Methods
		
		private void SetConnectionString(string connstring)
		{
			if (connstring == null) {
				Close ();
				conn_str = null;
				return;
			}
			
			if (connstring != conn_str) {
				Close ();
				conn_str = connstring;
				
				db_file = null;
				db_mode = 0644;
				
				string[] conn_pieces = connstring.Split (',');
				foreach (string piece in conn_pieces) {
					piece.Trim ();
					string[] arg_pieces = piece.Split ('=');
					if (arg_pieces.Length != 2) {
						throw new InvalidOperationException ("Invalid connection string");
					}
					string token = arg_pieces[0].ToLower ();
					string tvalue = arg_pieces[1];
					string tvalue_lc = arg_pieces[1].ToLower ();
					if (token == "uri") {
						if (tvalue_lc.StartsWith ("file://")) {
							db_file = tvalue.Substring (6);
						} else if (tvalue_lc.StartsWith ("file:")) {
							db_file = tvalue.Substring (5);
						} else if (tvalue_lc.StartsWith ("/")) {
							db_file = tvalue;
						} else {
							throw new InvalidOperationException ("Invalid connection string: invalid URI");
						}
					} else if (token == "mode") {
						db_mode = Convert.ToInt32 (tvalue);
					} else if (token == "version") {
						db_version = Convert.ToInt32 (tvalue);
					}
				}
				
				if (db_file == null) {
					throw new InvalidOperationException ("Invalid connection string: no URI");
				}
			}
		}
		
		#endregion

		#region Internal Methods
		
		internal void StartExec ()
		{
			// use a mutex here
			state = ConnectionState.Executing;
		}
		
		internal void EndExec ()
		{
			state = ConnectionState.Open;
		}
		
		#endregion

		#region Public Methods
		
		public IDbTransaction BeginTransaction ()
		{
			if (state != ConnectionState.Open)
				throw new InvalidOperationException("Invalid operation: The connection is close");
			
			SqliteTransaction t = new SqliteTransaction();
			t.Connection = this;
			SqliteCommand cmd = this.CreateCommand();
			cmd.CommandText = "BEGIN";
			cmd.ExecuteNonQuery();
			return t;
		}
		
		public IDbTransaction BeginTransaction (IsolationLevel il)
		{
			return null;
		}
		
		public void Close ()
		{
			if (state != ConnectionState.Open) {
				return;
			}
			
			state = ConnectionState.Closed;
		
			if (Version == 3)
				Sqlite.sqlite3_close (sqlite_handle);
			else 
				Sqlite.sqlite_close(sqlite_handle);
			sqlite_handle = IntPtr.Zero;
		}
		
		public void ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}
		
		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}
		
		public SqliteCommand CreateCommand ()
		{
			return new SqliteCommand (null, this);
		}
		
		public void Open ()
		{
			if (conn_str == null) {
				throw new InvalidOperationException ("No database specified");
			}
			
			if (state != ConnectionState.Closed) {
				return;
			}
			
			IntPtr errmsg = IntPtr.Zero;
			if (Version == 3) {
				int err = Sqlite.sqlite3_open(db_file, out sqlite_handle);
				if (err == (int)SqliteError.ERROR)
					throw new ApplicationException (Sqlite.sqlite3_errmsg (sqlite_handle));
			} else {
				sqlite_handle = Sqlite.sqlite_open(db_file, db_mode, out errmsg);
			
				if (errmsg != IntPtr.Zero) {
					string msg = Marshal.PtrToStringAnsi (errmsg);
					Sqlite.sqliteFree (errmsg);
					throw new ApplicationException (msg);
				}
			}
			state = ConnectionState.Open;
		}
		
		#endregion

	}
}
