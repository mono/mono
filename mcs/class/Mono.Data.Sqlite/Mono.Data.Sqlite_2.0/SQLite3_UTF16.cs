/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Alternate SQLite3 object, overriding many text behaviors to support UTF-16 (Unicode)
  /// </summary>
  internal class SQLite3_UTF16 : SQLite3
  {
    internal SQLite3_UTF16(SQLiteDateFormats fmt)
      : base(fmt)
    {
    }

    /// <summary>
    /// Overrides SqliteConvert.ToString() to marshal UTF-16 strings instead of UTF-8
    /// </summary>
    /// <param name="b">A pointer to a UTF-16 string</param>
    /// <param name="nbytelen">The length (IN BYTES) of the string</param>
    /// <returns>A .NET string</returns>
    public override string ToString(IntPtr b, int nbytelen)
    {
      return UTF16ToString(b, nbytelen);
    }

    public static string UTF16ToString(IntPtr b, int nbytelen)
    {
      if (nbytelen == 0 || b == IntPtr.Zero) return "";

      if (nbytelen == -1)
        return Marshal.PtrToStringUni(b);
      else
        return Marshal.PtrToStringUni(b, nbytelen / 2);
    }

    internal override void Open(string strFilename, SQLiteOpenFlagsEnum flags, int maxPoolSize, bool usePool)
    {
      if (_sql != null) return;

      _usePool = usePool;
      if (usePool)
      {
        _fileName = strFilename;
        _sql = SqliteConnectionPool.Remove(strFilename, maxPoolSize, out _poolVersion);
      }

      if (_sql == null)
      {
        IntPtr db;

#if !SQLITE_STANDARD
        int n = UnsafeNativeMethods.sqlite3_open16_interop(ToUTF8(strFilename), (int)flags, out db);
#else
        if ((flags & SQLiteOpenFlagsEnum.Create) == 0 && System.IO.File.Exists(strFilename) == false)
          throw new SqliteException((int)SQLiteErrorCode.CantOpen, strFilename);

        int n = UnsafeNativeMethods.sqlite3_open16(strFilename, out db);
#endif
        if (n > 0) throw new SqliteException(n, null);

        _sql = db;
      }
      _functionsArray = SqliteFunction.BindFunctions(this);
    }

    internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
    {
      Bind_Text(stmt, index, ToString(dt));
    }

    internal override void Bind_Text(SqliteStatement stmt, int index, string value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_text16(stmt._sqlite_stmt, index, value, value.Length * 2, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override DateTime GetDateTime(SqliteStatement stmt, int index)
    {
      return ToDateTime(GetText(stmt, index));
    }

    internal override string ColumnName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string GetText(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_text16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnOriginalName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_origin_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_database_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnTableName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_column_table_name16(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16_interop(ptr, out len), len);
#else
      return UTF16ToString(UnsafeNativeMethods.sqlite3_value_text16(ptr), -1);
#endif
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length * 2);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length * 2, (IntPtr)(-1));
    }
  }
}
