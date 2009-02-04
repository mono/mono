//
// Mono.Data.Sqlite.UnsafeNativeMethods.cs
//
// Author(s):
//   Robert Simpson (robert@blackcastlesoft.com)
//
// Adapted and modified for the Mono Project by
//   Marek Habersack (grendello@gmail.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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

/********************************************************
 * ADO.NET 2.0 Data Provider for Sqlite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
namespace Mono.Data.Sqlite
{
  using System;
  using System.Security;
  using System.Runtime.InteropServices;

#if !PLATFORM_COMPACTFRAMEWORK
  [SuppressUnmanagedCodeSecurity]
#endif
  internal sealed class UnsafeNativeMethods
  {
    private const string SQLITE_DLL = "sqlite3";

    private UnsafeNativeMethods()
    {
    }

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_sleep(uint dwMilliseconds);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_libversion();

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_free(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_open(byte[] utf8Filename, out IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_interrupt(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_close(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, out IntPtr errMsg, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_errmsg(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_changes(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_busy_timeout(IntPtr db, int ms);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_prepare_v2(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
	  
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_blob(IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_null(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_column_count(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_step(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern double sqlite3_column_double(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_column_int(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern Int64 sqlite3_column_int64(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_finalize(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_reset(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_create_collation(IntPtr db, byte[] strName, int eTextRep, IntPtr ctx, SqliteCollation fcompare);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_create_function(IntPtr db, byte[] strName, int nArgs, int eTextRep, IntPtr app, SqliteCallback func, SqliteCallback fstep, SqliteFinalCallback ffinal);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_aggregate_count(IntPtr context);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_blob(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_value_bytes(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern double sqlite3_value_double(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_value_int(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern Int64 sqlite3_value_int64(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_double(IntPtr context, double value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_int(IntPtr context, int value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_int64(IntPtr context, Int64 value);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_null(IntPtr context);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_table_column_metadata(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name16(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name16(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name16(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern int sqlite3_open16(string utf16Filename, out IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_errmsg16(IntPtr db);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern int sqlite3_prepare16_v2(IntPtr db, IntPtr pSql, int sqlLen, out IntPtr stmt, out IntPtr ptrRemain);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern int sqlite3_bind_text16(IntPtr stmt, int index, string value, int nlen, int nTransient);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name16(IntPtr stmt, int index);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype16(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern int sqlite3_create_collation16(IntPtr db, string strName, int eTextRep, IntPtr ctx, SqliteCollation fcompare);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern int sqlite3_create_function16(IntPtr db, string strName, int nArgs, int eTextRep, IntPtr app, SqliteCallback func, SqliteCallback funcstep, SqliteFinalCallback funcfinal);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text16(IntPtr p);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern void sqlite3_result_error16(IntPtr context, string strName, int nLen);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
    internal static extern void sqlite3_result_text16(IntPtr context, string strName, int nLen, IntPtr pvReserved);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_encryptfile(string fileName);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_decryptfile(string fileName);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_encryptedstatus(string fileName, out int fileStatus);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_compressfile(string fileName);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_decompressfile(string fileName);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_key(IntPtr db, byte[] key, int keylen);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_rekey(IntPtr db, byte[] key, int keylen);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_update_hook(IntPtr db, SqliteUpdateCallback func);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SqliteCommitCallback func);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SqliteRollbackCallback func);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_cursor_rowid(IntPtr stmt, int cursor, out long rowid);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_table_cursor(IntPtr stmt, int db, int tableRootPage);
    
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_last_insert_rowid(IntPtr db);
  }
}
#endif
