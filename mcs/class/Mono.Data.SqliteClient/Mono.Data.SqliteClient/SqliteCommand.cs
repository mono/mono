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
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Data;
using System.Diagnostics; 

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
		private IntPtr pStmt;

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
			get { return sql_params; }
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
			SqliteError err = SqliteError.OK;
			IntPtr pzTail = IntPtr.Zero;
			pStmt = IntPtr.Zero;	
			if (parent_conn.Version == 3)  
			{
				err = Sqlite.sqlite3_prepare (parent_conn.Handle, sql, sql.Length, out pStmt, out pzTail);
				if (err != SqliteError.OK)
					throw new ApplicationException ("Sqlite error in prepare " + err);
				int pcount = Sqlite.sqlite3_bind_parameter_count (pStmt);

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
			else 
			{
				IntPtr errMsg = IntPtr.Zero;
				string msg = "";
				string sqlData = sql;
				if (Parameters.Count > 0)
				{
					sqlData = ProcessParameters();
				}
				err = Sqlite.sqlite_compile (parent_conn.Handle, sqlData, out pzTail, out pStmt, out errMsg);
				
				if (err != SqliteError.OK) 
				{
					if (errMsg != IntPtr.Zero) 
					{
						msg = Marshal.PtrToStringAnsi (errMsg);
						Sqlite.sqliteFree (errMsg);
					}
					throw new ApplicationException ("Sqlite error " + msg);
				}
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
		  
			string msg = "";
			try 
			{
				if (!prepared)
				{
					Prepare ();
				}
				if (want_results) 
				{
					reader = new SqliteDataReader (this, pStmt, parent_conn.Version);
				} 
				else 
				{
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
				}
			}
			finally 
			{	
				if (parent_conn.Version == 3) 
				{}
				else
				{
					err = Sqlite.sqlite_finalize (pStmt, out errMsg);
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
 					//msg = Marshal.PtrToStringAnsi (errMsg);
					if (parent_conn.Version == 3)
					{
						err = Sqlite.sqlite3_finalize (pStmt, out errMsg);
					}
					else
					{
						err = Sqlite.sqlite_finalize (pStmt, out errMsg);
						Sqlite.sqliteFree (errMsg);
					}
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
