//
// Mono.Data.SqliteClient.SqliteCommand.cs
//
// Represents a Transact-SQL statement or stored procedure to execute against 
// a Sqlite database file.
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
using System.Text;
using System.Runtime.InteropServices;
using System.Data;

namespace Mono.Data.SqliteClient 
{
	public class SqliteCommand : IDbCommand
	{

		#region Fields
		
		private SqliteConnection parent_conn;
		//private SqliteTransaction transaction;
		private IDbTransaction transaction;
		private string sql;
		private int timeout;
		private CommandType type;
		private UpdateRowSource upd_row_source;
		private SqliteParameterCollection sql_params;
		
		#endregion

		#region Constructors and destructors
		
		public SqliteCommand ()
		{
			sql = "";
			sql_params = new SqliteParameterCollection ();
		}
		
		public SqliteCommand (string sqlText)
		{
			sql = sqlText;
			sql_params = new SqliteParameterCollection ();
		}
		
		public SqliteCommand (string sqlText, SqliteConnection dbConn)
		{
			sql = sqlText;
			parent_conn = dbConn;
			sql_params = new SqliteParameterCollection ();
		}
		
		public SqliteCommand (string sqlText, SqliteConnection dbConn, IDbTransaction trans)
		{
			sql = sqlText;
			parent_conn = dbConn;
			transaction = trans;
			sql_params = new SqliteParameterCollection ();
		}
		
		public void Dispose ()
		{
		}
		
		#endregion

		#region Properties
		
		public string CommandText {
			get { return sql; }
			set { sql = value; }
		}
		
		public int CommandTimeout {
			get { return timeout; }
			set { timeout = value; }
		}
		
		public CommandType CommandType {
			get { return type; }
			set { type = value; }
		}
		
		IDbConnection IDbCommand.Connection {
			get { return parent_conn; }
			set {
					if (!(value is SqliteConnection)) {
						throw new InvalidOperationException ("Can't set Connection to something other than a SqliteConnection");
					}
					parent_conn = (SqliteConnection) value;
			}
		}
		
		public SqliteConnection Connection {
			get { return parent_conn; }
			set { parent_conn = value; }
		}
		
		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}
		
		public SqliteParameterCollection Parameters {
			get { return sql_params; }
		}
		
		public IDbTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}
		
		public UpdateRowSource UpdatedRowSource {
			get { return upd_row_source; }
			set { upd_row_source = value; }
		}
		                
		#endregion

		#region Internal Methods
		
		internal int NumChanges () 
		{
			if (parent_conn.Version == 3)
				return Sqlite.sqlite3_changes(parent_conn.Handle);
			else
				return Sqlite.sqlite_changes(parent_conn.Handle);
		}
		
		#endregion

		#region Public Methods
		
		public void Cancel ()
		{
		}
		
		public void Prepare ()
		{
		}
		
		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}
		
		public SqliteParameter CreateParameter ()
		{
			return new SqliteParameter ();
		}
		
		public int ExecuteNonQuery ()
		{
			int rows_affected;
			SqliteDataReader r = ExecuteReader (CommandBehavior.Default, false, out rows_affected);
			return rows_affected;
		}
		
		public object ExecuteScalar ()
		{
			SqliteDataReader r = ExecuteReader ();
			if (r == null || !r.Read ()) {
				return null;
			}
			object o = r[0];
			r.Close ();
			return o;
		}
		
		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}
		
		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
		
		public SqliteDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}
		
		public SqliteDataReader ExecuteReader (CommandBehavior behavior)
		{
			int r;
			return ExecuteReader (behavior, true, out r);
		}
		
		public SqliteDataReader ExecuteReader (CommandBehavior behavior, bool want_results, out int rows_affected)
		{
			SqliteDataReader reader = null;
			SqliteError err = SqliteError.OK;
			IntPtr errMsg = IntPtr.Zero; 
			
			parent_conn.StartExec ();
			
			string msg = "";

			try {
				if (want_results) {
					IntPtr pVm = IntPtr.Zero;
					IntPtr pzTail = IntPtr.Zero;
					if (parent_conn.Version == 3)
						err = Sqlite.sqlite3_prepare (parent_conn.Handle, sql, sql.Length, out pVm, out pVm);
					else
						err = Sqlite.sqlite_compile (parent_conn.Handle, sql, out pzTail, out pVm, out errMsg);
					if (err == SqliteError.OK)
						reader = new SqliteDataReader (this, pVm, parent_conn.Version);
					if (parent_conn.Version == 3)
						err = Sqlite.sqlite3_finalize (pVm, out errMsg);
					else
						err = Sqlite.sqlite_finalize (pVm, out errMsg);
				} else {
					if (parent_conn.Version == 3)
						err = Sqlite.sqlite3_exec (parent_conn.Handle, sql, IntPtr.Zero, IntPtr.Zero, out errMsg);
					else
						err = Sqlite.sqlite_exec (parent_conn.Handle, sql, IntPtr.Zero, IntPtr.Zero, out errMsg);
				}
			} finally {			
				parent_conn.EndExec ();
			}

			if (err != SqliteError.OK) {
				if (errMsg != IntPtr.Zero) {
					msg = Marshal.PtrToStringAnsi (errMsg);
					if (parent_conn.Version == 3)
						Sqlite.sqlite3Free (errMsg);
					else
						Sqlite.sqliteFree (errMsg);
				}
				throw new ApplicationException ("Sqlite error " + msg);
			}
			
			rows_affected = NumChanges ();
			
			return reader;
		}
		
		public int LastInsertRowID () 
		{
			if (parent_conn.Version == 3)
				return Sqlite.sqlite3_last_insert_rowid(parent_conn.Handle);
			else
				return Sqlite.sqlite_last_insert_rowid(parent_conn.Handle);
		}
		
		#endregion

	}
}
