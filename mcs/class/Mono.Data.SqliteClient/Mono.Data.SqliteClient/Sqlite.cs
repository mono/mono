//
// Mono.Data.SqliteClient.Sqlite.cs
//
// Provides C# bindings to the library sqlite.dll
//
// Author(s): Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//
// Copyright (C) 2004  Everaldo Canuto
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
using System.Security;
using System.Runtime.InteropServices;

namespace Mono.Data.SqliteClient
{
	/// <summary>
	/// Represents the return values for sqlite_exec() and sqlite_step()
	/// </summary>
	internal enum SqliteError : int {
		/// <value>Successful result</value>
		OK        =  0,
		/// <value>SQL error or missing database</value>
		ERROR     =  1,
		/// <value>An internal logic error in SQLite</value>
		INTERNAL  =  2,
		/// <value>Access permission denied</value>
		PERM      =  3,
		/// <value>Callback routine requested an abort</value>
		ABORT     =  4,
		/// <value>The database file is locked</value>
		BUSY      =  5,
		/// <value>A table in the database is locked</value>
		LOCKED    =  6,
		/// <value>A malloc() failed</value>
		NOMEM     =  7,
		/// <value>Attempt to write a readonly database</value>
		READONLY  =  8,
		/// <value>Operation terminated by public const int interrupt()</value>
		INTERRUPT =  9,
		/// <value>Some kind of disk I/O error occurred</value>
		IOERR     = 10,
		/// <value>The database disk image is malformed</value>
		CORRUPT   = 11,
		/// <value>(Internal Only) Table or record not found</value>
		NOTFOUND  = 12,
		/// <value>Insertion failed because database is full</value>
		FULL      = 13,
		/// <value>Unable to open the database file</value>
		CANTOPEN  = 14,
		/// <value>Database lock protocol error</value>
		PROTOCOL  = 15,
		/// <value>(Internal Only) Database table is empty</value>
		EMPTY     = 16,
		/// <value>The database schema changed</value>
		SCHEMA    = 17,
		/// <value>Too much data for one row of a table</value>
		TOOBIG    = 18,
		/// <value>Abort due to contraint violation</value>
		CONSTRAINT= 19,
		/// <value>Data type mismatch</value>
		MISMATCH  = 20,
		/// <value>Library used incorrectly</value>
		MISUSE    = 21,
		/// <value>Uses OS features not supported on host</value>
		NOLFS     = 22,
		/// <value>Authorization denied</value>
		AUTH      = 23,
		/// <value>Auxiliary database format error</value>
		FORMAT    = 24,
		/// <value>2nd parameter to sqlite_bind out of range</value>
		RANGE     = 25,
		/// <value>File opened that is not a database file</value>
		NOTADB    = 26,
		/// <value>sqlite_step() has another row ready</value>
		ROW       = 100,
		/// <value>sqlite_step() has finished executing</value>
		DONE      = 101
	}

	/// <summary>
	/// Provides the core of C# bindings to the library sqlite.dll
	/// </summary>
	internal sealed class Sqlite {

		#region PInvoke Functions
		
		[DllImport("sqlite")]
		internal static extern IntPtr sqlite_open (string dbname, int db_mode, out IntPtr errstr);

		[DllImport("sqlite")]
		internal static extern void sqlite_close (IntPtr sqlite_handle);

		[DllImport("sqlite")]
		internal static extern int sqlite_changes (IntPtr handle);

		[DllImport("sqlite")]
		internal static extern int sqlite_last_insert_rowid (IntPtr sqlite_handle);

		[DllImport ("sqlite")]
		internal static extern void sqliteFree (IntPtr ptr);
		
		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_compile (IntPtr sqlite_handle, string zSql, out IntPtr pzTail, out IntPtr pVm, out IntPtr errstr);

		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_step (IntPtr pVm, out int pN, out IntPtr pazValue, out IntPtr pazColName);

		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_finalize (IntPtr pVm, out IntPtr pzErrMsg);

		[DllImport ("sqlite")]
                internal static extern SqliteError sqlite_exec (IntPtr handle, string sql, IntPtr callback, IntPtr user_data, out IntPtr errstr_ptr);
		
		[DllImport("sqlite3")]
		internal static extern int sqlite3_open (string dbname, out IntPtr handle);

		[DllImport("sqlite3")]
		internal static extern void sqlite3_close (IntPtr sqlite_handle);

		[DllImport("sqlite3")]
		internal static extern string sqlite3_errmsg (IntPtr sqlite_handle);

		[DllImport("sqlite3")]
		internal static extern int sqlite3_changes (IntPtr handle);

		[DllImport("sqlite3")]
		internal static extern int sqlite3_last_insert_rowid (IntPtr sqlite_handle);

		[DllImport ("sqlite3")]
		internal static extern void sqlite3Free (IntPtr ptr);
		
		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_prepare (IntPtr sqlite_handle, string zSql, int zSqllen, out IntPtr pVm, out IntPtr pzTail);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_step (IntPtr pVm);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_finalize (IntPtr pVm, out IntPtr pzErrMsg);

		[DllImport ("sqlite3")]
                internal static extern SqliteError sqlite3_exec (IntPtr handle, string sql, IntPtr callback, IntPtr user_data, out IntPtr errstr_ptr);
	
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_name (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_text (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_blob (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_bytes (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_count (IntPtr pVm);
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_type (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern Int64 sqlite3_column_int64 (IntPtr pVm, int col);
		[DllImport ("sqlite3")]
		internal static extern double sqlite3_column_double (IntPtr pVm, int col);
		#endregion
	}
}
