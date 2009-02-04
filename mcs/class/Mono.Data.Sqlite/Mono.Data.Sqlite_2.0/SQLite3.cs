//
// Mono.Data.Sqlite.SQLite3.cs
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
  using System.Data;
  using System.Runtime.InteropServices;
  using System.Collections.Generic;
  using System.Globalization;

  /// <summary>
  /// This class implements SqliteBase completely, and is the guts of the code that interop's Sqlite with .NET
  /// </summary>
  internal class Sqlite3 : SqliteBase
  {
    /// <summary>
    /// The opaque pointer returned to us by the sqlite provider
    /// </summary>
    protected IntPtr              _sql;
    /// <summary>
    /// The user-defined functions registered on this connection
    /// </summary>
    protected SqliteFunction[] _functionsArray;

    internal Sqlite3(SqliteDateFormats fmt)
      : base(fmt)
    {
    }

    protected override void Dispose(bool bDisposing)
    {
      Close();
    }

    internal override void Close()
    {
      if (_sql != IntPtr.Zero)
      {
        int n = UnsafeNativeMethods.sqlite3_close(_sql);
        if (n > 0) throw new SqliteException(n, SqliteLastError());
      }
      _sql = IntPtr.Zero;
    }

    internal override void Cancel()
    {
      UnsafeNativeMethods.sqlite3_interrupt(_sql);
    }

    internal override string Version
    {
      get
      {
	      return ToString (UnsafeNativeMethods.sqlite3_libversion());
      }
    }

    internal override int Changes
    {
      get
      {
        return UnsafeNativeMethods.sqlite3_changes(_sql);
      }
    }

    internal override void Open(string strFilename)
    {
      if (_sql != IntPtr.Zero) return;
      int n = UnsafeNativeMethods.sqlite3_open(ToUTF8(strFilename), out _sql);
      if (n > 0) throw new SqliteException(n, SqliteLastError());

      _functionsArray = SqliteFunction.BindFunctions(this);
    }

    internal override void SetTimeout(int nTimeoutMS)
    {
      int n = UnsafeNativeMethods.sqlite3_busy_timeout(_sql, nTimeoutMS);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override bool Step(SqliteStatement stmt)
    {
      int n;
      long timeout = 0;
      Random rnd = null;

      while (true)
      {
        n = UnsafeNativeMethods.sqlite3_step(stmt._sqlite_stmt);

        if (n == 100) return true;
        if (n == 101) return false;

        if (n > 0)
        {
          int r;

          // An error occurred, attempt to reset the statement.  If the reset worked because the
          // schema has changed, re-try the step again.  If it errored our because the database
          // is locked, then keep retrying until the command timeout occurs.
          r = Reset(stmt);

          if (r == 0)
            throw new SqliteException(n, SqliteLastError());

          else if (r == 6 && stmt._command != null) // SQLITE_LOCKED
          {
            // Keep trying
            if (timeout == 0) // First time we've encountered the lock
            {
              timeout = Environment.TickCount + (stmt._command._commandTimeout * 1000);
              rnd = new Random();
            }
            // If we've exceeded the command's timeout, give up and throw an error
            if (Environment.TickCount - timeout > 0)
            {
              throw new SqliteException(r, SqliteLastError());
            }
            else
            {
              // Otherwise sleep for a random amount of time up to 250ms
              UnsafeNativeMethods.sqlite3_sleep((uint)rnd.Next(1, 250));
            }
          }

        }
      }
    }

    internal override void FinalizeStatement(SqliteStatement stmt)
    {
      if (stmt._sqlite_stmt != IntPtr.Zero)
      {
        int n = UnsafeNativeMethods.sqlite3_finalize(stmt._sqlite_stmt);
        if (n > 0) throw new SqliteException(n, SqliteLastError());
      }
      stmt._sqlite_stmt = IntPtr.Zero;
    }

    internal override int Reset(SqliteStatement stmt)
    {
      int n;

      n = UnsafeNativeMethods.sqlite3_reset(stmt._sqlite_stmt);

      // If the schema changed, try and re-prepare it
      if (n == 17) // SQLITE_SCHEMA
      {
        // Recreate a dummy statement
        string str;
        using (SqliteStatement tmp = Prepare(stmt._sqlStatement, null, out str))
        {
          // Finalize the existing statement
          FinalizeStatement(stmt);

          // Reassign a new statement pointer to the old statement and clear the temporary one
          stmt._sqlite_stmt = tmp._sqlite_stmt;
          tmp._sqlite_stmt = IntPtr.Zero;

          // Reapply parameters
          stmt.BindParameters();
        }
        return -1; // Reset was OK, with schema change
      }
      else if (n == 6) // SQLITE_LOCKED
        return n;

      if (n > 0)
        throw new SqliteException(n, SqliteLastError());

      return 0; // We reset OK, no schema changes
    }

    internal override string SqliteLastError()
    {
      return ToString(UnsafeNativeMethods.sqlite3_errmsg(_sql));
    }

    internal override SqliteStatement Prepare(string strSql, SqliteStatement previous, out string strRemain)
    {
      IntPtr stmt = IntPtr.Zero;
      IntPtr ptr = IntPtr.Zero;
      int n = 17;
      int retries = 0;
      byte[] b = ToUTF8(strSql);
      string typedefs = null;
      SqliteStatement cmd = null;
      GCHandle handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      IntPtr psql = handle.AddrOfPinnedObject();

      try
      {
        while (n == 17 && retries < 3)
        {
		try {
			n = UnsafeNativeMethods.sqlite3_prepare_v2(_sql, psql, b.Length - 1, out stmt, out ptr);
		} catch (EntryPointNotFoundException) {
			n = UnsafeNativeMethods.sqlite3_prepare (_sql, psql, b.Length - 1, out stmt, out ptr);
		}
		
          retries++;

          if (n == 1)
          {
            if (String.Compare(SqliteLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) == 0)
            {
              int pos = strSql.IndexOf(';');
              if (pos == -1) pos = strSql.Length - 1;

              typedefs = strSql.Substring(0, pos + 1);
              strSql = strSql.Substring(pos + 1);

              strRemain = "";

              while (cmd == null && strSql.Length > 0)
              {
                cmd = Prepare(strSql, previous, out strRemain);
                strSql = strRemain;
              }

              if (cmd != null)
                cmd.SetTypes(typedefs);

              return cmd;
            }
          }
        }

        if (n > 0) throw new SqliteException(n, SqliteLastError());

        strRemain = UTF8ToString(ptr);

        if (stmt != IntPtr.Zero) cmd = new SqliteStatement(this, stmt, strSql.Substring(0, strSql.Length - strRemain.Length), previous);

        return cmd;
      }
      finally
      {
        handle.Free();
      }
    }

    internal override void Bind_Double(SqliteStatement stmt, int index, double value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_double(stmt._sqlite_stmt, index, value);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_Int32(SqliteStatement stmt, int index, int value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_int(stmt._sqlite_stmt, index, value);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_Int64(SqliteStatement stmt, int index, long value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_int64(stmt._sqlite_stmt, index, value);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_Text(SqliteStatement stmt, int index, string value)
    {
      byte[] b = ToUTF8(value);
      int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, b, b.Length - 1, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
    {
      byte[] b = ToUTF8(dt);
      int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, b, b.Length - 1, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_blob(stmt._sqlite_stmt, index, blobData, blobData.Length, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void Bind_Null(SqliteStatement stmt, int index)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_null(stmt._sqlite_stmt, index);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override int Bind_ParamCount(SqliteStatement stmt)
    {
      return UnsafeNativeMethods.sqlite3_bind_parameter_count(stmt._sqlite_stmt);
    }

    internal override string Bind_ParamName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name(stmt._sqlite_stmt, index));
    }

    internal override int Bind_ParamIndex(SqliteStatement stmt, string paramName)
    {
      return UnsafeNativeMethods.sqlite3_bind_parameter_index(stmt._sqlite_stmt, ToUTF8(paramName));
    }

    internal override int ColumnCount(SqliteStatement stmt)
    {
      return UnsafeNativeMethods.sqlite3_column_count(stmt._sqlite_stmt);
    }

    internal override string ColumnName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_name(stmt._sqlite_stmt, index));
    }

    internal override TypeAffinity ColumnAffinity(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
    }

    internal override string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity)
    {
      IntPtr p = UnsafeNativeMethods.sqlite3_column_decltype(stmt._sqlite_stmt, index);
      nAffinity = ColumnAffinity(stmt, index);

      if (p != IntPtr.Zero) return base.ToString(p);
      else
      {
        string[] ar = stmt.TypeDefinitions;
        if (ar != null)
        {
          if (index < ar.Length)
            return ar[index];
        }

        switch (nAffinity)
        {
          case TypeAffinity.Int64:
            return "BIGINT";
          case TypeAffinity.Double:
            return "DOUBLE";
          case TypeAffinity.Blob:
            return "BLOB";
          default:
            return "TEXT";
        }
      }
    }

    internal override int ColumnIndex(SqliteStatement stmt, string columnName)
    {
      int x = ColumnCount(stmt);

      for (int n = 0; n < x; n++)
      {
        if (String.Compare(columnName, ColumnName(stmt, n), true, CultureInfo.InvariantCulture) == 0)
          return n;
      }
      return -1;
    }

    internal override string ColumnOriginalName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_origin_name(stmt._sqlite_stmt, index));
    }

    internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_database_name(stmt._sqlite_stmt, index));
    }

    internal override string ColumnTableName(SqliteStatement stmt, int index)
    {
      return ToString(UnsafeNativeMethods.sqlite3_column_table_name(stmt._sqlite_stmt, index));
    }

    internal override void ColumnMetaData(string dataBase, string table, string column, out string dataType,
					  out string collateSequence, out bool notNull, out bool primaryKey,
					  out bool autoIncrement)
    {
      IntPtr dataTypePtr;
      IntPtr collSeqPtr;
      int nnotNull;
      int nprimaryKey;
      int nautoInc;
      int n;

      n = UnsafeNativeMethods.sqlite3_table_column_metadata(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column),
							    out dataTypePtr, out collSeqPtr, out nnotNull,
							    out nprimaryKey, out nautoInc);
      if (n > 0) throw new SqliteException(n, SqliteLastError());

      dataType = base.ToString(dataTypePtr);
      collateSequence = base.ToString(collSeqPtr);

      notNull = (nnotNull == 1);
      primaryKey = (nprimaryKey == 1);
      autoIncrement = (nautoInc == 1);
    }

    internal override double GetDouble(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
    }

    internal override int GetInt32(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
    }

    internal override long GetInt64(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
    }

    internal override string GetText(SqliteStatement stmt, int index)
    {
	    return ToString (UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index));
    }

    internal override DateTime GetDateTime(SqliteStatement stmt, int index)
    {
	    return ToDateTime(GetText (stmt, index));
    }

    internal override long GetBytes(SqliteStatement stmt, int index, int nDataOffset, byte[] bDest, int nStart, int nLength)
    {
      IntPtr ptr;
      int nlen;
      int nCopied = nLength;

      nlen = UnsafeNativeMethods.sqlite3_column_bytes(stmt._sqlite_stmt, index);
      ptr = UnsafeNativeMethods.sqlite3_column_blob(stmt._sqlite_stmt, index);

      if (bDest == null) return nlen;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

	  unsafe {
		  if (nCopied > 0)
			  Marshal.Copy((IntPtr)((byte*)ptr + nDataOffset), bDest, nStart, nCopied);
		  else nCopied = 0;
	  }

      return nCopied;
    }

    internal override long GetChars(SqliteStatement stmt, int index, int nDataOffset, char[] bDest, int nStart, int nLength)
    {
      int nlen;
      int nCopied = nLength;

      string str = GetText(stmt, index);
      nlen = str.Length;

      if (bDest == null) return nlen;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

      if (nCopied > 0)
        str.CopyTo(nDataOffset, bDest, nStart, nCopied);
      else nCopied = 0;

      return nCopied;
    }

    internal override bool IsNull(SqliteStatement stmt, int index)
    {
      return (ColumnAffinity(stmt, index) == TypeAffinity.Null);
    }

    internal override int AggregateCount(IntPtr context)
    {
      return UnsafeNativeMethods.sqlite3_aggregate_count(context);
    }

    internal override void CreateFunction(string strFunction, int nArgs, SqliteCallback func, SqliteCallback funcstep, SqliteFinalCallback funcfinal)
    {
      int n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override void CreateCollation(string strCollation, SqliteCollation func)
    {
      int n = UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 1, IntPtr.Zero, func);
      if (n > 0) throw new SqliteException(n, SqliteLastError());
    }

    internal override long GetParamValueBytes(IntPtr p, int nDataOffset, byte[] bDest, int nStart, int nLength)
    {
      IntPtr ptr;
      int nlen;
      int nCopied = nLength;

      nlen = UnsafeNativeMethods.sqlite3_value_bytes(p);
      ptr = UnsafeNativeMethods.sqlite3_value_blob(p);

      if (bDest == null) return nlen;

      if (nCopied + nStart > bDest.Length) nCopied = bDest.Length - nStart;
      if (nCopied + nDataOffset > nlen) nCopied = nlen - nDataOffset;

	  unsafe {
		  if (nCopied > 0)
			  Marshal.Copy((IntPtr)((byte*)ptr + nDataOffset), bDest, nStart, nCopied);
		  else nCopied = 0;
	  }

      return nCopied;
    }

    internal override double GetParamValueDouble(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_double(ptr);
    }

    internal override int GetParamValueInt32(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_int(ptr);
    }

    internal override long GetParamValueInt64(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_int64(ptr);
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
      return ToString(UnsafeNativeMethods.sqlite3_value_text(ptr));
    }

    internal override TypeAffinity GetParamValueType(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_type(ptr);
    }

    internal override void ReturnBlob(IntPtr context, byte[] value)
    {
      UnsafeNativeMethods.sqlite3_result_blob(context, value, value.Length, (IntPtr)(-1));
    }

    internal override void ReturnDouble(IntPtr context, double value)
    {
      UnsafeNativeMethods.sqlite3_result_double(context, value);
    }

    internal override void ReturnError(IntPtr context, string value)
    {
      UnsafeNativeMethods.sqlite3_result_error(context, ToUTF8(value), value.Length);
    }

    internal override void ReturnInt32(IntPtr context, int value)
    {
      UnsafeNativeMethods.sqlite3_result_int(context, value);
    }

    internal override void ReturnInt64(IntPtr context, long value)
    {
      UnsafeNativeMethods.sqlite3_result_int64(context, value);
    }

    internal override void ReturnNull(IntPtr context)
    {
      UnsafeNativeMethods.sqlite3_result_null(context);
    }

    internal override void ReturnText(IntPtr context, string value)
    {
      byte[] b = ToUTF8(value);
      UnsafeNativeMethods.sqlite3_result_text(context, ToUTF8(value), b.Length - 1, (IntPtr)(-1));
    }

    internal override IntPtr AggregateContext(IntPtr context)
    {
      return UnsafeNativeMethods.sqlite3_aggregate_context(context, 1);
    }

    internal override void SetUpdateHook(SqliteUpdateCallback func)
    {
      UnsafeNativeMethods.sqlite3_update_hook(_sql, func);
    }

    internal override void SetCommitHook(SqliteCommitCallback func)
    {
      UnsafeNativeMethods.sqlite3_commit_hook(_sql, func);
    }

    internal override void SetRollbackHook(SqliteRollbackCallback func)
    {
      UnsafeNativeMethods.sqlite3_rollback_hook(_sql, func);
    }

    /// <summary>
    /// Helper function to retrieve a column of data from an active statement.
    /// </summary>
    /// <param name="stmt">The statement being step()'d through</param>
    /// <param name="index">The column index to retrieve</param>
    /// <param name="typ">The type of data contained in the column.  If Uninitialized, this function will retrieve the datatype information.</param>
    /// <returns>Returns the data in the column</returns>
    internal override object GetValue(SqliteStatement stmt, int index, ref SqliteType typ)
    {
      if (typ.Affinity == 0) typ = SqliteConvert.ColumnToType(stmt, index);
      if (IsNull(stmt, index)) return DBNull.Value;

      Type t = SqliteConvert.SqliteTypeToType(typ);

      switch (TypeToAffinity(t))
      {
        case TypeAffinity.Blob:
          if (typ.Type == DbType.Guid && typ.Affinity == TypeAffinity.Text)
            return new Guid(GetText(stmt, index));

          int n = (int)GetBytes(stmt, index, 0, null, 0, 0);
          byte[] b = new byte[n];
          GetBytes(stmt, index, 0, b, 0, n);

          if (typ.Type == DbType.Guid && n == 16)
            return new Guid(b);

          return b;
        case TypeAffinity.DateTime:
          return GetDateTime(stmt, index);
        case TypeAffinity.Double:
          return Convert.ChangeType(GetDouble(stmt, index), t, null);
        case TypeAffinity.Int64:
          return Convert.ChangeType(GetInt64(stmt, index), t, null);
        default:
          return GetText(stmt, index);
      }
    }

    internal override int GetLastInsertRowId ()
    {
	    return UnsafeNativeMethods.sqlite3_last_insert_rowid (_sql);
    }
  }
}
#endif
