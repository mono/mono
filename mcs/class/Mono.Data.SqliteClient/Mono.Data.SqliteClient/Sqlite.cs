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
		internal static extern IntPtr sqlite_open (string dbname, int db_mode, out string errstr);

		[DllImport("sqlite")]
		internal static extern void sqlite_close (IntPtr sqlite_handle);

		[DllImport("sqlite")]
		internal unsafe static extern SqliteError sqlite_exec (IntPtr handle, string sql, SqliteCallbackFunction callback, IntPtr user_data, byte **errstr_ptr);

		[DllImport("sqlite")]
		internal static extern int sqlite_changes (IntPtr handle);

		[DllImport("sqlite")]
		internal static extern int sqlite_last_insert_rowid (IntPtr sqlite_handle);

		[DllImport ("sqlite")]
		internal unsafe static extern void sqliteFree (void *ptr);
		
		#endregion
		
		#region Delegates
		
		internal unsafe delegate int SqliteCallbackFunction (ref object o, int argc, sbyte **argv, sbyte **colnames);
		
		#endregion
		
	}
}
