//
// Mono.Data.Sqlite.SQLite3_UTF16.cs
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
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
namespace Mono.Data.Sqlite
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// Alternate Sqlite3 object, overriding many text behaviors to support UTF-16 (Unicode)
  /// </summary>
  internal class Sqlite3_UTF16 : Sqlite3
  {
    internal Sqlite3_UTF16(SqliteDateFormats fmt)
      : base(fmt)
    {
    }

    /// <summary>
    /// Overrides SqliteConvert.ToString() to marshal UTF-16 strings instead of UTF-8
    /// </summary>
    /// <param name="b">A pointer to a UTF-16 string</param>
    /// <param name="nbytelen">The length (IN BYTES) of the string</param>
    /// <returns>A .NET string</returns>
    public override string ToString(IntPtr b)
    {
      return Marshal.PtrToStringUni(b);
    }

    internal override string Version
    {
      get
      {
        return base.ToString(UnsafeNativeMethods.sqlite3_libversion());
      }
    }

    internal override void Open(string strFilename)
    {
      if (_sql != IntPtr.Zero) return;
      int n = UnsafeNativeMethods.sqlite3_open16(strFilename, out _sql);
      if (n > 0) throw new SqliteException(n, SqliteLastError());

      _functionsArray = SqliteFunction.BindFunctions(this);
    }

    internal override string SqliteLastError()
    {
      return ToString(UnsafeNativeMethods.sqlite3_errmsg16(_sql));
    }

    internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
    {
      Bind_Text(stmt, index, ToString(dt));
    }

    internal override string Bind_ParamName(SqliteStatement stmt, int index)
    {
      return base.ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name(stmt._sqlite_stmt, index));
    }

    internal override void Bind_Text(SqliteStatement stmt, int index, string value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_text16(stmt._sqlite_stmt, index, value, value.Length * 2, -1);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override string ColumnName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_name16(stmt._sqlite_stmt, index));
    }

    internal override DateTime GetDateTime(SqliteStatement stmt, int index)
    {
      return ToDateTime(GetText(stmt, index));
    }
    internal override string GetText(SqliteStatement stmt, int index)
    {
	    return ToString (UnsafeNativeMethods.sqlite3_column_text16(stmt._sqlite_stmt, index));
    }

    internal override string ColumnOriginalName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_origin_name16(stmt._sqlite_stmt, index));
    }

    internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_database_name16(stmt._sqlite_stmt, index));
    }

    internal override string ColumnTableName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_table_name16(stmt._sqlite_stmt, index));
    }

    internal override void CreateFunction(string strFunction, int nArgs, SqliteCallback func, SqliteCallback funcstep, SqliteFinalCallback funcfinal)
    {
      int n = UnsafeNativeMethods.sqlite3_create_function16(_sql, strFunction, nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void CreateCollation(string strCollation, SqliteCollation func)
    {
      int n = UnsafeNativeMethods.sqlite3_create_collation16(_sql, strCollation, 4, IntPtr.Zero, func);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
      return ToString(UnsafeNativeMethods.sqlite3_value_text16(ptr));
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error16(context, value, value.Length);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_text16(context, value, value.Length, (IntPtr)(-1));
    }
  }
}
#endif
