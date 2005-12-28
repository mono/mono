//
// Mono.Data.SqliteClient.SqliteCommand.cs
//
// Represents a Transact-SQL statement or stored procedure to execute against 
// a Sqlite database file.
//
// Author(s): 	Vladimir Vukicevic  <vladimir@pobox.com>
//		Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//		Chris Turchin <chris@turchin.net>
//		Jeroen Zwartepoorte <jeroen@xs4all.nl>
//		Thomas Zoechling <thomas.zoechling@gmx.at>
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
using System.Collections;
using System.Text;
using Mono.Unix;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Data;
using System.Diagnostics; 
using Group = System.Text.RegularExpressions.Group;

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
		private bool prepared = false;
		private ArrayList pStmts;

		#endregion

		#region Constructors and destructors
		
		public SqliteCommand ()
		{
			sql = "";
		}
				
		public SqliteCommand (string sqlText)
		{
			sql = sqlText;
		}
		
		public SqliteCommand (string sqlText, SqliteConnection dbConn)
		{
			sql = sqlText;
			parent_conn = dbConn;
		}
		
		public SqliteCommand (string sqlText, SqliteConnection dbConn, IDbTransaction trans)
		{
			sql = sqlText;
			parent_conn = dbConn;
			transaction = trans;
		}
		
		public void Dispose ()
		{
		}
		
		#endregion

		#region Properties
		
		public string CommandText 
		{
			get { return sql; }
			set { sql = value; }
		}
		
		public int CommandTimeout
		{
			get { return timeout; }
			set { timeout = value; }
		}
		
		public CommandType CommandType 
		{
			get { return type; }
			set { type = value; }
		}
		
		IDbConnection IDbCommand.Connection 
		{
			get 
			{ 
				return parent_conn; 
			}
			set 
			{
				if (!(value is SqliteConnection)) 
				{
					throw new InvalidOperationException ("Can't set Connection to something other than a SqliteConnection");
				}
				parent_conn = (SqliteConnection) value;
			}
		}
		
		public SqliteConnection Connection
		{
			get { return parent_conn; }
			set { parent_conn = value; }
		}
		
		IDataParameterCollection IDbCommand.Parameters 
		{
			get { return Parameters; }
		}
		
		public SqliteParameterCollection Parameters 
		{
			get
			{
				if (sql_params == null) sql_params = new SqliteParameterCollection();
				return sql_params;
			}
		}
		
		public IDbTransaction Transaction 
		{
			get { return transaction; }
			set { transaction = value; }
		}
		
		public UpdateRowSource UpdatedRowSource 
		{
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
		
		private string ReplaceParams(Match m)
		{
			string input = m.Value;                                                                                                                
			if (m.Groups["param"].Success)
			{
				Group g = m.Groups["param"];
				string find = g.Value;
				//FIXME: sqlite works internally only with strings, so this assumtion is mostly legit, but what about date formatting, etc?
				//Need to fix SqlLiteDataReader first to acurately describe the tables
				SqliteParameter sqlp = Parameters[find];
				string replace = Convert.ToString(sqlp.Value);
				if(sqlp.DbType == DbType.String)
				{
					replace =  "\"" + replace + "\"";
				}
				
				input = Regex.Replace(input,find,replace);
				return input;
			}
			else
			return m.Value;
		}
		
		#endregion

		#region Public Methods
		
		public void Cancel ()
		{
		}
		
		public string ProcessParameters()
		{
			string processedText = sql;

			//Regex looks odd perhaps, but it works - same impl. as in the firebird db provider
			//the named parameters are using the ADO.NET standard @-prefix but sqlite is considering ":" as a prefix for v.3...
			//ref: http://www.mail-archive.com/sqlite-users@sqlite.org/msg01851.html
			//Regex r = new Regex(@"(('[^']*?\@[^']*')*[^'@]*?)*(?<param>@\w+)+([^'@]*?('[^']*?\@[^']*'))*",RegexOptions.ExplicitCapture);
			
			//The above statement is true for the commented regEx, but I changed it to use the :-prefix, because now (12.05.2005 sqlite3) 
			//sqlite is using : as Standard Parameterprefix
			
			Regex r = new Regex(@"(('[^']*?\:[^']*')*[^':]*?)*(?<param>:\w+)+([^':]*?('[^']*?\:[^']*'))*",RegexOptions.ExplicitCapture);
			MatchEvaluator me = new MatchEvaluator(ReplaceParams);
			processedText = r.Replace(sql, me);
			return processedText;
		}
		
		public void Prepare ()
		{
			pStmts = new ArrayList();
			string sqlcmds = sql;
			
			if (Parameters.Count > 0 && parent_conn.Version == 2)
			{
				sqlcmds = ProcessParameters();
			}
			
			SqliteError err = SqliteError.OK;
			IntPtr psql = UnixMarshal.StringToHeap(sqlcmds);
			IntPtr pzTail = psql;
			try {
				do { // sql may contain multiple sql commands, loop until they're all processed
					IntPtr pStmt = IntPtr.Zero;
					if (parent_conn.Version == 3)
					{
						err = Sqlite.sqlite3_prepare (parent_conn.Handle, pzTail, sql.Length, out pStmt, out pzTail);
						if (err != SqliteError.OK) {
							string msg = Marshal.PtrToStringAnsi (Sqlite.sqlite3_errmsg (parent_conn.Handle));
							throw new ApplicationException (msg);
						}
					}
					else
					{
						IntPtr errMsg;
						err = Sqlite.sqlite_compile (parent_conn.Handle, pzTail, out pzTail, out pStmt, out errMsg);
						
						if (err != SqliteError.OK) 
						{
							string msg = "unknown error";
							if (errMsg != IntPtr.Zero) 
							{
								msg = Marshal.PtrToStringAnsi (errMsg);
								Sqlite.sqliteFree (errMsg);
							}
							throw new ApplicationException ("Sqlite error: " + msg);
						}
					}
						
					pStmts.Add(pStmt);
					
					if (parent_conn.Version == 3) 
					{
						int pcount = Sqlite.sqlite3_bind_parameter_count (pStmt);
						if (sql_params == null) pcount = 0;
		
						for (int i = 1; i <= pcount; i++) 
						{
							String name = Sqlite.sqlite3_bind_parameter_name (pStmt, i);
							SqliteParameter param = sql_params[name];
							Type ptype = param.Value.GetType ();
							
							if (ptype.Equals (typeof (String))) 
							{
								String s = (String)param.Value;
								err = Sqlite.sqlite3_bind_text (pStmt, i, s, s.Length, (IntPtr)(-1));
							} 
							else if (ptype.Equals (typeof (DBNull))) 
							{
								err = Sqlite.sqlite3_bind_null (pStmt, i);
							}
							else if (ptype.Equals (typeof (Boolean))) 
							{
								bool b = (bool)param.Value;
								err = Sqlite.sqlite3_bind_int (pStmt, i, b ? 1 : 0);
							} else if (ptype.Equals (typeof (Byte))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (Byte)param.Value);
							}
							else if (ptype.Equals (typeof (Char))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (Char)param.Value);
							} 
							else if (ptype.Equals (typeof (Int16))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (Int16)param.Value);
							} 
							else if (ptype.Equals (typeof (Int32))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (Int32)param.Value);
							}
							else if (ptype.Equals (typeof (SByte))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (SByte)param.Value);
							} 
							else if (ptype.Equals (typeof (UInt16))) 
							{
								err = Sqlite.sqlite3_bind_int (pStmt, i, (UInt16)param.Value);
							}
							else if (ptype.Equals (typeof (DateTime))) 
							{
								DateTime dt = (DateTime)param.Value;
								err = Sqlite.sqlite3_bind_int64 (pStmt, i, dt.ToFileTime ());
							} 
							else if (ptype.Equals (typeof (Double))) 
							{
								err = Sqlite.sqlite3_bind_double (pStmt, i, (Double)param.Value);
							}
							else if (ptype.Equals (typeof (Single))) 
							{
								err = Sqlite.sqlite3_bind_double (pStmt, i, (Single)param.Value);
							} 
							else if (ptype.Equals (typeof (UInt32))) 
							{
								err = Sqlite.sqlite3_bind_int64 (pStmt, i, (UInt32)param.Value);
							}
							else if (ptype.Equals (typeof (Int64))) 
							{
								err = Sqlite.sqlite3_bind_int64 (pStmt, i, (Int64)param.Value);
							} 
							else 
							{
								throw new ApplicationException("Unkown Parameter Type");
							}
							if (err != SqliteError.OK) 
							{
								throw new ApplicationException ("Sqlite error in bind " + err);
							}
						}
					}
				} while ((int)pzTail - (int)psql < sql.Length);
			} finally {
				UnixMarshal.FreeHeap(psql);
			}
			prepared=true;
		}
		
		
		IDbDataParameter IDbCommand.CreateParameter()
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
		  
			try 
			{
				if (!prepared)
				{
					Prepare ();
				}
				for (int i = 0; i < pStmts.Count; i++) {
					IntPtr pStmt = (IntPtr)pStmts[i];
					
					// If want_results, return the results of the last statement
					// via the SqliteDataReader, and execute but ignore the results
					// of the other statements.
					if (i == pStmts.Count-1 && want_results) 
					{
						reader = new SqliteDataReader (this, pStmt, parent_conn.Version);
						break;
					} 
					
					// Execute but ignore the results of these statements.
					if (parent_conn.Version == 3) 
					{
						err = Sqlite.sqlite3_step (pStmt);
					}
					else 
					{
						int cols;
						IntPtr pazValue = IntPtr.Zero;
						IntPtr pazColName = IntPtr.Zero;
						err = Sqlite.sqlite_step (pStmt, out cols, out pazValue, out pazColName);
					}
					// On error, misuse, or busy, don't bother with the rest of the statements.
					if (err != SqliteError.ROW && err != SqliteError.DONE) break;
				}
			}
			finally 
			{	
				foreach (IntPtr pStmt in pStmts) {
					if (parent_conn.Version == 3) 
					{
						err = Sqlite.sqlite3_finalize (pStmt);
					}
					else
					{
						err = Sqlite.sqlite_finalize (pStmt, out errMsg);
					}
				}
				parent_conn.EndExec ();
				prepared = false;
			}
			
			if (err != SqliteError.OK &&
			    err != SqliteError.DONE &&
			    err != SqliteError.ROW) 
			{
 				if (errMsg != IntPtr.Zero) 
				{
					// TODO: Get the message text
				}
				throw new ApplicationException ("Sqlite error");
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
