//
// Mono.Data.SqliteClient.Sqlite.cs
//
// Provides C# bindings to the library sqlite.dll
//
//            	Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//			Chris Turchin <chris@turchin.net>
//			Jeroen Zwartepoorte <jeroen@xs4all.nl>
//			Thomas Zoechling <thomas.zoechling@gmx.at>
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
using System.Text;

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
		internal static extern SqliteError sqlite_compile (IntPtr sqlite_handle, IntPtr zSql, out IntPtr pzTail, out IntPtr pVm, out IntPtr errstr);

		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_step (IntPtr pVm, out int pN, out IntPtr pazValue, out IntPtr pazColName);

		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_finalize (IntPtr pVm, out IntPtr pzErrMsg);

		[DllImport ("sqlite")]
		internal static extern SqliteError sqlite_exec (IntPtr handle, string sql, IntPtr callback, IntPtr user_data, out IntPtr errstr_ptr);
		
		[DllImport ("sqlite")]
		internal static extern void sqlite_busy_timeout (IntPtr handle, int ms);

		[DllImport("sqlite3", CharSet = CharSet.Unicode)]
		internal static extern int sqlite3_open16 (string dbname, out IntPtr handle);

		[DllImport("sqlite3")]
		internal static extern void sqlite3_close (IntPtr sqlite_handle);

		[DllImport("sqlite3")]
		internal static extern IntPtr sqlite3_errmsg16 (IntPtr sqlite_handle);

		[DllImport("sqlite3")]
		internal static extern int sqlite3_changes (IntPtr handle);

		[DllImport("sqlite3")]
		internal static extern long sqlite3_last_insert_rowid (IntPtr sqlite_handle);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_prepare16 (IntPtr sqlite_handle, IntPtr zSql, int zSqllen, out IntPtr pVm, out IntPtr pzTail);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_step (IntPtr pVm);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_finalize (IntPtr pVm);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_exec (IntPtr handle, string sql, IntPtr callback, IntPtr user_data, out IntPtr errstr_ptr);
	
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_name16 (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_text16 (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_blob (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_bytes16 (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_count (IntPtr pVm);
		
		[DllImport ("sqlite3")]
		internal static extern int sqlite3_column_type (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern Int64 sqlite3_column_int64 (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern double sqlite3_column_double (IntPtr pVm, int col);
		
		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_column_decltype16 (IntPtr pVm, int col);

 		[DllImport ("sqlite3")]
		internal static extern int sqlite3_bind_parameter_count (IntPtr pStmt);

		[DllImport ("sqlite3")]
		internal static extern IntPtr sqlite3_bind_parameter_name (IntPtr pStmt, int n); // UTF-8 encoded return

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_bind_blob (IntPtr pStmt, int n, byte[] blob, int length, IntPtr freetype);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_bind_double (IntPtr pStmt, int n, double value);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_bind_int (IntPtr pStmt, int n, int value);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_bind_int64 (IntPtr pStmt, int n, long value);

		[DllImport ("sqlite3")]
		internal static extern SqliteError sqlite3_bind_null (IntPtr pStmt, int n);

		[DllImport ("sqlite3", CharSet = CharSet.Unicode)]
		internal static extern SqliteError sqlite3_bind_text16 (IntPtr pStmt, int n, string value, int length, IntPtr freetype);
		
		[DllImport ("sqlite3")]
		internal static extern void sqlite3_busy_timeout (IntPtr handle, int ms);

		#endregion
		
		// These are adapted from Mono.Unix.  When encoding is null,
		// use Ansi encoding, which is a superset of the default
		// expected encoding (ISO-8859-1).

		public static IntPtr StringToHeap (string s, Encoding encoding)
		{
			if (encoding == null)
				return Marshal.StringToHGlobalAnsi (s);
				
			int min_byte_count = encoding.GetMaxByteCount(1);
			char[] copy = s.ToCharArray ();
			byte[] marshal = new byte [encoding.GetByteCount (copy) + min_byte_count];

			int bytes_copied = encoding.GetBytes (copy, 0, copy.Length, marshal, 0);

			if (bytes_copied != (marshal.Length-min_byte_count))
				throw new NotSupportedException ("encoding.GetBytes() doesn't equal encoding.GetByteCount()!");

			IntPtr mem = Marshal.AllocHGlobal (marshal.Length);
			if (mem == IntPtr.Zero)
				throw new OutOfMemoryException ();

			bool copied = false;
			try {
				Marshal.Copy (marshal, 0, mem, marshal.Length);
				copied = true;
			}
			finally {
				if (!copied)
					Marshal.FreeHGlobal (mem);
			}

			return mem;
		}

		public static unsafe string HeapToString (IntPtr p, Encoding encoding)
		{
			if (encoding == null)
				return Marshal.PtrToStringAnsi (p);
		
			if (p == IntPtr.Zero)
				return null;
				
			// This assumes a single byte terminates the string.

			int len = 0;
			while (Marshal.ReadByte (p, len) != 0)
				checked {++len;}

			string s = new string ((sbyte*) p, 0, len, encoding);
			len = s.Length;
			while (len > 0 && s [len-1] == 0)
				--len;
			if (len == s.Length) 
				return s;
			return s.Substring (0, len);
		}



	}
}
