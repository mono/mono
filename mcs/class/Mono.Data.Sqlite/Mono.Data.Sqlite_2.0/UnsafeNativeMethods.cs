/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Security;
  using System.Runtime.InteropServices;
#if WINDOWS_PHONE
  using Mono.Data.Sqlite.DllImport;
#endif

#if !PLATFORM_COMPACTFRAMEWORK && !WINDOWS_STORE_APP
  [SuppressUnmanagedCodeSecurity]
#endif
  internal static class UnsafeNativeMethods
  {
#if !WINDOWS_PHONE
#if !SQLITE_STANDARD

#if !USE_INTEROP_DLL

#if !PLATFORM_COMPACTFRAMEWORK
    private const string SQLITE_DLL = "Mono.Data.Sqlite.DLL";
#else
    internal const string SQLITE_DLL = "SQLite.Interop.061.DLL";
#endif // PLATFORM_COMPACTFRAMEWORK

#else
    private const string SQLITE_DLL = "SQLite.Interop.DLL";
#endif // USE_INTEROP_DLL

#else
    private const string SQLITE_DLL = "sqlite3";
#endif

    // This section uses interop calls that also fetch text length to optimize conversion.  
    // When using the standard dll, we can replace these calls with normal sqlite calls and do unoptimized conversions instead afterwards
    #region interop added textlength calls

#if !SQLITE_STANDARD
    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_bind_parameter_name_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_database_name16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_decltype16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_name16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_origin_name16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_table_name16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_column_text16_interop(IntPtr stmt, int index, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_errmsg_interop(IntPtr db, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_prepare_interop(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain, out int nRemain);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_table_column_metadata_interop(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc, out int dtLen, out int csLen);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text_interop(IntPtr p, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_value_text16_interop(IntPtr p, out int len);
#endif

    #endregion

    // These functions add existing functionality on top of SQLite and require a little effort to
    // get working when using the standard SQLite library.
    #region interop added functionality

#if !SQLITE_STANDARD
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_close_interop(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_create_function_interop(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, int needCollSeq);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_finalize_interop(IntPtr stmt);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_open_interop(byte[] utf8Filename, int flags, out IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_open16_interop(byte[] utf8Filename, int flags, out IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_reset_interop(IntPtr stmt);

#endif

    #endregion

    // The standard api call equivalents of the above interop calls
    #region standard versions of interop functions

#if SQLITE_STANDARD
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_close(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_close_v2(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_create_function(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal);

#if !PLATFORM_COMPACTFRAMEWORK
		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport(SQLITE_DLL)]
#endif
		internal static extern int sqlite3_create_function_v2(IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal, SQLiteFinalCallback fdestroy);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_finalize(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_open_v2(byte[] utf8Filename, out IntPtr db, int flags, IntPtr vfs);

    // Compatibility with versions < 3.5.0
#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_open(byte[] utf8Filename, out IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern int sqlite3_open16 (byte[] fileName, out IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_reset(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_database_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_decltype16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_origin_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_table_name16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_text16(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_errmsg(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_table_column_metadata(IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_text16(IntPtr p);
#endif

    #endregion

    // These functions are custom and have no equivalent standard library method.
    // All of them are "nice to haves" and not necessarily "need to haves".
    #region no equivalent standard method

#if !SQLITE_STANDARD
    [DllImport(SQLITE_DLL)]
    internal static extern IntPtr sqlite3_context_collseq(IntPtr context, out int type, out int enc, out int len);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_context_collcompare(IntPtr context, byte[] p1, int p1len, byte[] p2, int p2len);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_cursor_rowid(IntPtr stmt, int cursor, out long rowid);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_index_column_info_interop(IntPtr db, byte[] catalog, byte[] IndexName, byte[] ColumnName, out int sortOrder, out int onError, out IntPtr Collation, out int colllen);

    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_resetall_interop(IntPtr db);

    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_table_cursor(IntPtr stmt, int db, int tableRootPage);
#endif

    #endregion

    // These are obsolete and will be removed in the future 
    #region windows ntfs filesystem only

#if !SQLITE_STANDARD
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_compressfile(string fileName);

    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int sqlite3_decompressfile(string fileName);
#endif

    #endregion

    // Standard API calls global across versions.  There are a few instances of interop calls
    // scattered in here, but they are only active when PLATFORM_COMPACTFRAMEWORK is declared.
    #region standard sqlite api calls

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_libversion();

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_interrupt(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_changes(IntPtr db);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_busy_timeout(IntPtr db, int ms);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_blob(IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_double_interop(IntPtr stmt, int index, ref double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern int sqlite3_bind_int64_interop(IntPtr stmt, int index, ref long value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_null(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_parameter_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_bind_parameter_index(IntPtr stmt, byte[] strName);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_count(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_step(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern double sqlite3_column_double(IntPtr stmt, int index);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_double_interop(IntPtr stmt, int index, out double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_int(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_column_int64(IntPtr stmt, int index);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_column_int64_interop(IntPtr stmt, int index, out long value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_column_blob(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_column_bytes(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern TypeAffinity sqlite3_column_type(IntPtr stmt, int index);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_create_collation(IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_aggregate_count(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_value_blob(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_value_bytes(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern double sqlite3_value_double(IntPtr p);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_double_interop(IntPtr p, out double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_value_int(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long sqlite3_value_int64(IntPtr p);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_value_int64_interop(IntPtr p, out Int64 value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern TypeAffinity sqlite3_value_type(IntPtr p);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int nSize, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sqlite3_result_double(IntPtr context, double value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_double_interop(IntPtr context, ref double value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_error(IntPtr context, byte[] strErr, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_int(IntPtr context, int value);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void sqlite3_result_int64(IntPtr context, long value);
#else
    [DllImport(SQLITE_DLL)]
    internal static extern void sqlite3_result_int64_interop(IntPtr context, ref Int64 value);
#endif

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_null(IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern void sqlite3_result_text(IntPtr context, byte[] value, int nLen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int nBytes);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern int sqlite3_bind_text16 (IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern void sqlite3_result_error16 (IntPtr context, byte[] strName, int nLen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
#else
    [DllImport(SQLITE_DLL, CharSet = CharSet.Unicode)]
#endif
    internal static extern void sqlite3_result_text16 (IntPtr context, byte[] strName, int nLen, IntPtr pvReserved);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_key(IntPtr db, byte[] key, int keylen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_rekey(IntPtr db, byte[] key, int keylen);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_update_hook(IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_commit_hook(IntPtr db, SQLiteCommitCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_rollback_hook(IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_db_handle(IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern IntPtr sqlite3_next_stmt(IntPtr db, IntPtr stmt);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_exec(IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, out IntPtr errMsg);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_config (SQLiteConfig config);

#if !PLATFORM_COMPACTFRAMEWORK
		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport(SQLITE_DLL)]
#endif
		internal static extern IntPtr sqlite3_user_data (IntPtr context);

#if !PLATFORM_COMPACTFRAMEWORK
    [DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
#else
    [DllImport(SQLITE_DLL)]
#endif
    internal static extern int sqlite3_free (IntPtr ptr);

    #endregion
#else
    internal static int sqlite3_close (IntPtr db)
    {
        return NativeWrapper.sqlite3_close (db.ToInt64 ());
    }
    internal static int sqlite3_close_v2 (IntPtr db)
    {
        return NativeWrapper.sqlite3_close_v2 (db.ToInt64 ());
    }
    internal static int sqlite3_create_function (IntPtr db, byte[] strName, int nArgs, int nType, IntPtr pvUser, SQLiteCallback func, SQLiteCallback fstep, SQLiteFinalCallback ffinal)
    {
        return NativeWrapper.sqlite3_create_function (db.ToInt64 (), strName, nArgs, nType, pvUser.ToInt64 (), GetFunctionPtr (func), GetFunctionPtr (fstep), GetFunctionPtr (ffinal));
    }
    internal static int sqlite3_finalize (IntPtr stmt)
    {
        return NativeWrapper.sqlite3_finalize (stmt.ToInt64 ());
    }
    internal static int sqlite3_open_v2 (byte[] utf8Filename, out IntPtr db, int flags, IntPtr vfs)
    {
      long dbPtr;
      int res = NativeWrapper.sqlite3_open_v2 (utf8Filename, out dbPtr, flags, vfs.ToInt64 ());
      db = new IntPtr (dbPtr);
      return res;
    }
    internal static int sqlite3_open (byte[] utf8Filename, out IntPtr db)
    {
      long dbPtr;
      int res = NativeWrapper.sqlite3_open (utf8Filename, out dbPtr);
      db = new IntPtr (dbPtr);
      return res;
    }
    internal static int sqlite3_open16 (byte[] fileName, out IntPtr db)
    {
      long dbPtr;
      int res = NativeWrapper.sqlite3_open16 (fileName, out dbPtr);
      db = new IntPtr (dbPtr);
      return res;
    }
    internal static int sqlite3_reset (IntPtr stmt)
    {
        return NativeWrapper.sqlite3_reset (stmt.ToInt64 ());
    }
    internal static IntPtr sqlite3_bind_parameter_name (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_bind_parameter_name (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_database_name (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_database_name (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_database_name16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_database_name16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_decltype (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_decltype (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_decltype16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_decltype16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_name (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_name (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_name16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_name16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_origin_name (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_origin_name (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_origin_name16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_origin_name16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_table_name (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_table_name (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_table_name16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_table_name16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_text (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_text (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_text16 (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_text16 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_errmsg (IntPtr db)
    {
        return (IntPtr)NativeWrapper.sqlite3_errmsg (db.ToInt64 ());
    }
    internal static int sqlite3_prepare (IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain)
    {
        long stmtPtr;
        long ptrRemainPtr;
        int res = NativeWrapper.sqlite3_prepare (db.ToInt64 (), pSql.ToInt64 (), nBytes, out stmtPtr, out ptrRemainPtr);
        stmt = new IntPtr (stmtPtr);
        ptrRemain = new IntPtr (ptrRemainPtr);
        return res;
    }
    internal static int sqlite3_table_column_metadata (IntPtr db, byte[] dbName, byte[] tblName, byte[] colName, out IntPtr ptrDataType, out IntPtr ptrCollSeq, out int notNull, out int primaryKey, out int autoInc)
    {
        long ptrDataTypePtr;
        long ptrCollSeqPtr;
        int res = NativeWrapper.sqlite3_table_column_metadata (db.ToInt64 (), dbName, tblName, colName, out ptrDataTypePtr, out ptrCollSeqPtr, out notNull, out primaryKey, out autoInc);
        ptrDataType = new IntPtr (ptrDataTypePtr);
        ptrCollSeq = new IntPtr (ptrCollSeqPtr);
        return res;
    }
    internal static IntPtr sqlite3_value_text (IntPtr p)
    {
        return (IntPtr)NativeWrapper.sqlite3_value_text (p.ToInt64 ());
    }
    internal static IntPtr sqlite3_value_text16 (IntPtr p)
    {
        return (IntPtr)NativeWrapper.sqlite3_value_text16 (p.ToInt64 ());
    }
    internal static IntPtr sqlite3_libversion ()
    {
        return (IntPtr)NativeWrapper.sqlite3_libversion ();
    }
    internal static void sqlite3_interrupt (IntPtr db)
    {
        NativeWrapper.sqlite3_interrupt (db.ToInt64 ());
    }
    internal static int sqlite3_changes (IntPtr db)
    {
        return NativeWrapper.sqlite3_changes (db.ToInt64 ());
    }
    internal static int sqlite3_busy_timeout (IntPtr db, int ms)
    {
        return NativeWrapper.sqlite3_busy_timeout (db.ToInt64 (), ms);
    }
    internal static int sqlite3_bind_blob (IntPtr stmt, int index, Byte[] value, int nSize, IntPtr nTransient)
    {
        return NativeWrapper.sqlite3_bind_blob (stmt.ToInt64 (), index, value, nSize, nTransient.ToInt64 ());
    }
    internal static int sqlite3_bind_double (IntPtr stmt, int index, double value)
    {
        return NativeWrapper.sqlite3_bind_double (stmt.ToInt64 (), index, value);
    }
    internal static int sqlite3_bind_int (IntPtr stmt, int index, int value)
    {
        return NativeWrapper.sqlite3_bind_int (stmt.ToInt64 (), index, value);
    }
    internal static int sqlite3_bind_int64 (IntPtr stmt, int index, long value)
    {
        return NativeWrapper.sqlite3_bind_int64 (stmt.ToInt64 (), index, value);
    }
    internal static int sqlite3_bind_null (IntPtr stmt, int index)
    {
        return NativeWrapper.sqlite3_bind_null (stmt.ToInt64 (), index);
    }
    internal static int sqlite3_bind_text (IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved)
    {
        return NativeWrapper.sqlite3_bind_text (stmt.ToInt64 (), index, value, nlen, pvReserved.ToInt64 ());
    }
    internal static int sqlite3_bind_parameter_count (IntPtr stmt)
    {
        return NativeWrapper.sqlite3_bind_parameter_count (stmt.ToInt64 ());
    }
    internal static int sqlite3_bind_parameter_index (IntPtr stmt, byte[] strName)
    {
        return NativeWrapper.sqlite3_bind_parameter_index (stmt.ToInt64 (), strName);
    }
    internal static int sqlite3_column_count (IntPtr stmt)
    {
        return NativeWrapper.sqlite3_column_count (stmt.ToInt64 ());
    }
    internal static int sqlite3_step (IntPtr stmt)
    {
        return NativeWrapper.sqlite3_step (stmt.ToInt64 ());
    }
    internal static double sqlite3_column_double (IntPtr stmt, int index)
    {
        return NativeWrapper.sqlite3_column_double (stmt.ToInt64 (), index);
    }
    internal static int sqlite3_column_int (IntPtr stmt, int index)
    {
        return NativeWrapper.sqlite3_column_int (stmt.ToInt64 (), index);
    }
    internal static long sqlite3_column_int64 (IntPtr stmt, int index)
    {
        return NativeWrapper.sqlite3_column_int64 (stmt.ToInt64 (), index);
    }
    internal static IntPtr sqlite3_column_blob (IntPtr stmt, int index)
    {
        return (IntPtr)NativeWrapper.sqlite3_column_blob (stmt.ToInt64 (), index);
    }
    internal static int sqlite3_column_bytes (IntPtr stmt, int index)
    {
        return NativeWrapper.sqlite3_column_bytes (stmt.ToInt64 (), index);
    }
    internal static TypeAffinity sqlite3_column_type (IntPtr stmt, int index)
    {
        return (TypeAffinity)NativeWrapper.sqlite3_column_type (stmt.ToInt64 (), index);
    }
    internal static int sqlite3_create_collation (IntPtr db, byte[] strName, int nType, IntPtr pvUser, SQLiteCollation func)
    {
        return NativeWrapper.sqlite3_create_collation (db.ToInt64 (), strName, nType, pvUser.ToInt64 (), GetFunctionPtr (func));
    }
    internal static int sqlite3_aggregate_count (IntPtr context)
    {
        return NativeWrapper.sqlite3_aggregate_count (context.ToInt64 ());
    }
    internal static IntPtr sqlite3_value_blob (IntPtr p)
    {
        return (IntPtr)NativeWrapper.sqlite3_value_blob (p.ToInt64 ());
    }
    internal static int sqlite3_value_bytes (IntPtr p)
    {
        return NativeWrapper.sqlite3_value_bytes (p.ToInt64 ());
    }
    internal static double sqlite3_value_double (IntPtr p)
    {
        return NativeWrapper.sqlite3_value_double (p.ToInt64 ());
    }
    internal static int sqlite3_value_int (IntPtr p)
    {
        return NativeWrapper.sqlite3_value_int (p.ToInt64 ());
    }
    internal static long sqlite3_value_int64 (IntPtr p)
    {
        return NativeWrapper.sqlite3_value_int64 (p.ToInt64 ());
    }
    internal static TypeAffinity sqlite3_value_type (IntPtr p)
    {
        return (TypeAffinity)NativeWrapper.sqlite3_value_type (p.ToInt64 ());
    }
    internal static void sqlite3_result_blob (IntPtr context, byte[] value, int nSize, IntPtr pvReserved)
    {
        NativeWrapper.sqlite3_result_blob (context.ToInt64 (), value, nSize, pvReserved.ToInt64 ());
    }
    internal static void sqlite3_result_double (IntPtr context, double value)
    {
        NativeWrapper.sqlite3_result_double (context.ToInt64 (), value);
    }
    internal static void sqlite3_result_error (IntPtr context, byte[] strErr, int nLen)
    {
        NativeWrapper.sqlite3_result_error (context.ToInt64 (), strErr, nLen);
    }
    internal static void sqlite3_result_int (IntPtr context, int value)
    {
        NativeWrapper.sqlite3_result_int (context.ToInt64 (), value);
    }
    internal static void sqlite3_result_int64 (IntPtr context, long value)
    {
        NativeWrapper.sqlite3_result_int64 (context.ToInt64 (), value);
    }
    internal static void sqlite3_result_null (IntPtr context)
    {
        NativeWrapper.sqlite3_result_null (context.ToInt64 ());
    }
    internal static void sqlite3_result_text (IntPtr context, byte[] value, int nLen, IntPtr pvReserved)
    {
        NativeWrapper.sqlite3_result_text (context.ToInt64 (), value, nLen, pvReserved.ToInt64 ());
    }
    internal static IntPtr sqlite3_aggregate_context (IntPtr context, int nBytes)
    {
        return (IntPtr)NativeWrapper.sqlite3_aggregate_context (context.ToInt64 (), nBytes);
    }
    internal static int sqlite3_bind_text16 (IntPtr stmt, int index, byte[] value, int nlen, IntPtr pvReserved)
    {
        return NativeWrapper.sqlite3_bind_text16 (stmt.ToInt64 (), index, value, nlen, pvReserved.ToInt64 ());
    }
    internal static void sqlite3_result_error16 (IntPtr context, byte[] strName, int nLen)
    {
        NativeWrapper.sqlite3_result_error16 (context.ToInt64 (), strName, nLen);
    }
    internal static void sqlite3_result_text16 (IntPtr context, byte[] strName, int nLen, IntPtr pvReserved)
    {
        NativeWrapper.sqlite3_result_text16 (context.ToInt64 (), strName, nLen, pvReserved.ToInt64 ());
    }
    internal static int sqlite3_key (IntPtr db, byte[] key, int keylen)
    {
        return NativeWrapper.sqlite3_key (db.ToInt64 (), key, keylen);
    }
    internal static int sqlite3_rekey (IntPtr db, byte[] key, int keylen)
    {
        return NativeWrapper.sqlite3_rekey (db.ToInt64 (), key, keylen);
    }
    internal static IntPtr sqlite3_update_hook (IntPtr db, SQLiteUpdateCallback func, IntPtr pvUser)
    {
        return (IntPtr)NativeWrapper.sqlite3_update_hook (db.ToInt64 (), GetFunctionPtr (func), pvUser.ToInt64 ());
    }
    internal static IntPtr sqlite3_commit_hook (IntPtr db, SQLiteCommitCallback func, IntPtr pvUser)
    {
        return (IntPtr)NativeWrapper.sqlite3_commit_hook (db.ToInt64 (), GetFunctionPtr (func), pvUser.ToInt64 ());
    }
    internal static IntPtr sqlite3_rollback_hook (IntPtr db, SQLiteRollbackCallback func, IntPtr pvUser)
    {
        return (IntPtr)NativeWrapper.sqlite3_rollback_hook (db.ToInt64 (), GetFunctionPtr (func), pvUser.ToInt64 ());
    }
    internal static IntPtr sqlite3_db_handle (IntPtr stmt)
    {
        return (IntPtr)NativeWrapper.sqlite3_db_handle (stmt.ToInt64 ());
    }
    internal static IntPtr sqlite3_next_stmt (IntPtr db, IntPtr stmt)
    {
        return (IntPtr)NativeWrapper.sqlite3_next_stmt (db.ToInt64 (), stmt.ToInt64 ());
    }
    internal static int sqlite3_exec (IntPtr db, byte[] strSql, IntPtr pvCallback, IntPtr pvParam, out IntPtr errMsg)
    {
        long errMsgPtr;
        int res = NativeWrapper.sqlite3_exec (db.ToInt64 (), strSql, pvCallback.ToInt64 (), pvParam.ToInt64 (), out errMsgPtr);
        errMsg = new IntPtr (errMsgPtr);
        return res;
    }
    internal static int sqlite3_config (SQLiteConfig config)
    {
        return NativeWrapper.sqlite3_config ((int)config);
    }
    internal static int sqlite3_free (IntPtr ptr)
    {
        NativeWrapper.sqlite3_free (ptr.ToInt64 ());
        return 0;
    }
    private static long GetFunctionPtr (Delegate d)
    {
        if (d == null)
        {
            return IntPtr.Zero.ToInt64 ();
        }
        return Marshal.GetFunctionPointerForDelegate (d).ToInt64 ();
    }
#endif
  }

#if PLATFORM_COMPACTFRAMEWORK
  internal abstract class CriticalHandle : IDisposable
  {
    private bool _isClosed;
    protected IntPtr handle;
    
    protected CriticalHandle(IntPtr invalidHandleValue)
    {
      handle = invalidHandleValue;
      _isClosed = false;
    }

    ~CriticalHandle()
    {
      Dispose(false);
    }

    private void Cleanup()
    {
      if (!IsClosed)
      {
        this._isClosed = true;
        if (!IsInvalid)
        {
          ReleaseHandle();
          GC.SuppressFinalize(this);
        }
      }
    }

    public void Close()
    {
      Dispose(true);
    }

    public void Dispose()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      Cleanup();
    }

    protected abstract bool ReleaseHandle();

    protected void SetHandle(IntPtr value)
    {
      handle = value;
    }

    public void SetHandleAsInvalid()
    {
      _isClosed = true;
      GC.SuppressFinalize(this);
    }

    public bool IsClosed
    {
      get { return _isClosed; }
    }

    public abstract bool IsInvalid
    {
      get;
    }

  }

#endif

  // Handles the unmanaged database pointer, and provides finalization support for it.
  internal class SqliteConnectionHandle : CriticalHandle
  {
    public static implicit operator IntPtr(SqliteConnectionHandle db)
    {
      return db.handle;
    }

    public static implicit operator SqliteConnectionHandle(IntPtr db)
    {
      return new SqliteConnectionHandle(db);
    }

    private SqliteConnectionHandle(IntPtr db)
      : this()
    {
      SetHandle(db);
    }

    internal SqliteConnectionHandle()
      : base(IntPtr.Zero)
    {
    }

    protected override bool ReleaseHandle()
    {
      try
      {
        SQLiteBase.CloseConnection(this);
      }
      catch (SqliteException)
      {
      }
      return true;
    }

    public override bool IsInvalid
    {
      get { return (handle == IntPtr.Zero); }
    }
  }

  // Provides finalization support for unmanaged SQLite statements.
  internal class SqliteStatementHandle : CriticalHandle
  {
    public static implicit operator IntPtr(SqliteStatementHandle stmt)
    {
      return stmt.handle;
    }

    public static implicit operator SqliteStatementHandle(IntPtr stmt)
    {
      return new SqliteStatementHandle(stmt);
    }

    private SqliteStatementHandle(IntPtr stmt)
      : this()
    {
      SetHandle(stmt);
    }

    internal SqliteStatementHandle()
      : base(IntPtr.Zero)
    {
    }

    protected override bool ReleaseHandle()
    {
      try
      {
        SQLiteBase.FinalizeStatement(this);
      }
      catch (SqliteException)
      {
      }
      return true;
    }

    public override bool IsInvalid
    {
      get { return (handle == IntPtr.Zero); }
    }
  }
}
