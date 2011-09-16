/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Data;
  using System.Runtime.InteropServices;
  using System.Collections.Generic;
  using System.Globalization;

  /// <summary>
  /// This class implements SQLiteBase completely, and is the guts of the code that interop's SQLite with .NET
  /// </summary>
  internal class SQLite3 : SQLiteBase
  {
    /// <summary>
    /// The opaque pointer returned to us by the sqlite provider
    /// </summary>
    protected SqliteConnectionHandle _sql;
    protected string _fileName;
    protected bool _usePool;
    protected int _poolVersion = 0;

#if !PLATFORM_COMPACTFRAMEWORK
    private bool _buildingSchema = false;
#endif
#if MONOTOUCH
    GCHandle gch;
#endif
    /// <summary>
    /// The user-defined functions registered on this connection
    /// </summary>
    protected SqliteFunction[] _functionsArray;

    internal SQLite3(SQLiteDateFormats fmt)
      : base(fmt)
    {
#if MONOTOUCH
      gch = GCHandle.Alloc (this);
#endif
    }

    protected override void Dispose(bool bDisposing)
    {
      if (bDisposing)
        Close();
#if MONOTOUCH
      if (gch.IsAllocated)
        gch.Free ();
#endif
    }

    // It isn't necessary to cleanup any functions we've registered.  If the connection
    // goes to the pool and is resurrected later, re-registered functions will overwrite the
    // previous functions.  The SqliteFunctionCookieHandle will take care of freeing unmanaged
    // resources belonging to the previously-registered functions.
    internal override void Close()
    {
      if (_sql != null)
      {
        if (_usePool)
        {
          SQLiteBase.ResetConnection(_sql);
          SqliteConnectionPool.Add(_fileName, _sql, _poolVersion);
        }
        else
          _sql.Dispose();
      }

      _sql = null;
    }

    internal override void Cancel()
    {
      UnsafeNativeMethods.sqlite3_interrupt(_sql);
    }

    internal override string Version
    {
      get
      {
        return SQLite3.SQLiteVersion;
      }
    }

    internal static string SQLiteVersion
    {
      get
      {
        return UTF8ToString(UnsafeNativeMethods.sqlite3_libversion(), -1);
      }
    }

    internal override int Changes
    {
      get
      {
        return UnsafeNativeMethods.sqlite3_changes(_sql);
      }
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
        int n = UnsafeNativeMethods.sqlite3_open_interop(ToUTF8(strFilename), (int)flags, out db);
#else
	// Compatibility with versions < 3.5.0
        int n;

	try {
		n = UnsafeNativeMethods.sqlite3_open_v2(ToUTF8(strFilename), out db, (int)flags, IntPtr.Zero);
	} catch (EntryPointNotFoundException ex) {
		Console.WriteLine ("Your sqlite3 version is old - please upgrade to at least v3.5.0!");
		n = UnsafeNativeMethods.sqlite3_open (ToUTF8 (strFilename), out db);
	}
	
#endif
        if (n > 0) throw new SqliteException(n, null);

        _sql = db;
      }
      // Bind functions to this connection.  If any previous functions of the same name
      // were already bound, then the new bindings replace the old.
      _functionsArray = SqliteFunction.BindFunctions(this);
      SetTimeout(0);
    }

    internal override void ClearPool()
    {
      SqliteConnectionPool.ClearPool(_fileName);
    }

    internal override void SetTimeout(int nTimeoutMS)
    {
      int n = UnsafeNativeMethods.sqlite3_busy_timeout(_sql, nTimeoutMS);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override bool Step(SqliteStatement stmt)
    {
      int n;
      Random rnd = null;
      uint starttick = (uint)Environment.TickCount;
      uint timeout = (uint)(stmt._command._commandTimeout * 1000);

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
            throw new SqliteException(n, SQLiteLastError());

          else if ((r == 6 || r == 5) && stmt._command != null) // SQLITE_LOCKED || SQLITE_BUSY
          {
            // Keep trying
            if (rnd == null) // First time we've encountered the lock
              rnd = new Random();

            // If we've exceeded the command's timeout, give up and throw an error
            if ((uint)Environment.TickCount - starttick > timeout)
            {
              throw new SqliteException(r, SQLiteLastError());
            }
            else
            {
              // Otherwise sleep for a random amount of time up to 150ms
              System.Threading.Thread.CurrentThread.Join(rnd.Next(1, 150));
            }
          }
        }
      }
    }

    internal override int Reset(SqliteStatement stmt)
    {
      int n;

#if !SQLITE_STANDARD
      n = UnsafeNativeMethods.sqlite3_reset_interop(stmt._sqlite_stmt);
#else
      n = UnsafeNativeMethods.sqlite3_reset(stmt._sqlite_stmt);
#endif

      // If the schema changed, try and re-prepare it
      if (n == 17) // SQLITE_SCHEMA
      {
        // Recreate a dummy statement
        string str;
        using (SqliteStatement tmp = Prepare(null, stmt._sqlStatement, null, (uint)(stmt._command._commandTimeout * 1000), out str))
        {
          // Finalize the existing statement
          stmt._sqlite_stmt.Dispose();
          // Reassign a new statement pointer to the old statement and clear the temporary one
          stmt._sqlite_stmt = tmp._sqlite_stmt;
          tmp._sqlite_stmt = null;

          // Reapply parameters
          stmt.BindParameters();
        }
        return -1; // Reset was OK, with schema change
      }
      else if (n == 6 || n == 5) // SQLITE_LOCKED || SQLITE_BUSY
        return n;

      if (n > 0)
        throw new SqliteException(n, SQLiteLastError());

      return 0; // We reset OK, no schema changes
    }

    internal override string SQLiteLastError()
    {
      return SQLiteBase.SQLiteLastError(_sql);
    }

    internal override SqliteStatement Prepare(SqliteConnection cnn, string strSql, SqliteStatement previous, uint timeoutMS, out string strRemain)
    {
      IntPtr stmt = IntPtr.Zero;
      IntPtr ptr = IntPtr.Zero;
      int len = 0;
      int n = 17;
      int retries = 0;
      byte[] b = ToUTF8(strSql);
      string typedefs = null;
      SqliteStatement cmd = null;
      Random rnd = null;
      uint starttick = (uint)Environment.TickCount;

      GCHandle handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      IntPtr psql = handle.AddrOfPinnedObject();
      try
      {
        while ((n == 17 || n == 6 || n == 5) && retries < 3)
        {
#if !SQLITE_STANDARD
          n = UnsafeNativeMethods.sqlite3_prepare_interop(_sql, psql, b.Length - 1, out stmt, out ptr, out len);
#else
          n = UnsafeNativeMethods.sqlite3_prepare(_sql, psql, b.Length - 1, out stmt, out ptr);
          len = -1;
#endif

          if (n == 17)
            retries++;
          else if (n == 1)
          {
            if (String.Compare(SQLiteLastError(), "near \"TYPES\": syntax error", StringComparison.OrdinalIgnoreCase) == 0)
            {
              int pos = strSql.IndexOf(';');
              if (pos == -1) pos = strSql.Length - 1;

              typedefs = strSql.Substring(0, pos + 1);
              strSql = strSql.Substring(pos + 1);

              strRemain = "";

              while (cmd == null && strSql.Length > 0)
              {
                cmd = Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                strSql = strRemain;
              }

              if (cmd != null)
                cmd.SetTypes(typedefs);

              return cmd;
            }
#if !PLATFORM_COMPACTFRAMEWORK
            else if (_buildingSchema == false && String.Compare(SQLiteLastError(), 0, "no such table: TEMP.SCHEMA", 0, 26, StringComparison.OrdinalIgnoreCase) == 0)
            {
              strRemain = "";
              _buildingSchema = true;
              try
              {
                ISQLiteSchemaExtensions ext = ((IServiceProvider)SqliteFactory.Instance).GetService(typeof(ISQLiteSchemaExtensions)) as ISQLiteSchemaExtensions;

                if (ext != null)
                  ext.BuildTempSchema(cnn);

                while (cmd == null && strSql.Length > 0)
                {
                  cmd = Prepare(cnn, strSql, previous, timeoutMS, out strRemain);
                  strSql = strRemain;
                }

                return cmd;
              }
              finally
              {
                _buildingSchema = false;
              }
            }
#endif
          }
          else if (n == 6 || n == 5) // Locked -- delay a small amount before retrying
          {
            // Keep trying
            if (rnd == null) // First time we've encountered the lock
              rnd = new Random();

            // If we've exceeded the command's timeout, give up and throw an error
            if ((uint)Environment.TickCount - starttick > timeoutMS)
            {
              throw new SqliteException(n, SQLiteLastError());
            }
            else
            {
              // Otherwise sleep for a random amount of time up to 150ms
              System.Threading.Thread.CurrentThread.Join(rnd.Next(1, 150));
            }
          }
        }

        if (n > 0) throw new SqliteException(n, SQLiteLastError());

        strRemain = UTF8ToString(ptr, len);

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
#if !PLATFORM_COMPACTFRAMEWORK
      int n = UnsafeNativeMethods.sqlite3_bind_double(stmt._sqlite_stmt, index, value);
#else
      int n = UnsafeNativeMethods.sqlite3_bind_double_interop(stmt._sqlite_stmt, index, ref value);
#endif
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_Int32(SqliteStatement stmt, int index, int value)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_int(stmt._sqlite_stmt, index, value);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_Int64(SqliteStatement stmt, int index, long value)
    {
#if !PLATFORM_COMPACTFRAMEWORK
      int n = UnsafeNativeMethods.sqlite3_bind_int64(stmt._sqlite_stmt, index, value);
#else
      int n = UnsafeNativeMethods.sqlite3_bind_int64_interop(stmt._sqlite_stmt, index, ref value);
#endif
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_Text(SqliteStatement stmt, int index, string value)
    {
      byte[] b = ToUTF8(value);
      int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, b, b.Length - 1, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt)
    {
      byte[] b = ToUTF8(dt);
      int n = UnsafeNativeMethods.sqlite3_bind_text(stmt._sqlite_stmt, index, b, b.Length - 1, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_blob(stmt._sqlite_stmt, index, blobData, blobData.Length, (IntPtr)(-1));
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void Bind_Null(SqliteStatement stmt, int index)
    {
      int n = UnsafeNativeMethods.sqlite3_bind_null(stmt._sqlite_stmt, index);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override int Bind_ParamCount(SqliteStatement stmt)
    {
      return UnsafeNativeMethods.sqlite3_bind_parameter_count(stmt._sqlite_stmt);
    }

    internal override string Bind_ParamName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_bind_parameter_name(stmt._sqlite_stmt, index), -1);
#endif
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
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override TypeAffinity ColumnAffinity(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_type(stmt._sqlite_stmt, index);
    }

    internal override string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity)
    {
      int len;
#if !SQLITE_STANDARD
      IntPtr p = UnsafeNativeMethods.sqlite3_column_decltype_interop(stmt._sqlite_stmt, index, out len);
#else
      len = -1;
      IntPtr p = UnsafeNativeMethods.sqlite3_column_decltype(stmt._sqlite_stmt, index);
#endif
      nAffinity = ColumnAffinity(stmt, index);

      if (p != IntPtr.Zero) return UTF8ToString(p, len);
      else
      {
        string[] ar = stmt.TypeDefinitions;
        if (ar != null)
        {
          if (index < ar.Length && ar[index] != null)
            return ar[index];
        }
        return String.Empty;

        //switch (nAffinity)
        //{
        //  case TypeAffinity.Int64:
        //    return "BIGINT";
        //  case TypeAffinity.Double:
        //    return "DOUBLE";
        //  case TypeAffinity.Blob:
        //    return "BLOB";
        //  default:
        //    return "TEXT";
        //}
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
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_origin_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnDatabaseName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_database_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override string ColumnTableName(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_table_name(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement)
    {
      IntPtr dataTypePtr;
      IntPtr collSeqPtr;
      int nnotNull;
      int nprimaryKey;
      int nautoInc;
      int n;
      int dtLen;
      int csLen;

#if !SQLITE_STANDARD
      n = UnsafeNativeMethods.sqlite3_table_column_metadata_interop(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column), out dataTypePtr, out collSeqPtr, out nnotNull, out nprimaryKey, out nautoInc, out dtLen, out csLen);
#else
      dtLen = -1;
      csLen = -1;
      n = UnsafeNativeMethods.sqlite3_table_column_metadata(_sql, ToUTF8(dataBase), ToUTF8(table), ToUTF8(column), out dataTypePtr, out collSeqPtr, out nnotNull, out nprimaryKey, out nautoInc);
#endif
      if (n > 0) throw new SqliteException(n, SQLiteLastError());

      dataType = UTF8ToString(dataTypePtr, dtLen);
      collateSequence = UTF8ToString(collSeqPtr, csLen);

      notNull = (nnotNull == 1);
      primaryKey = (nprimaryKey == 1);
      autoIncrement = (nautoInc == 1);
    }

    internal override double GetDouble(SqliteStatement stmt, int index)
    {
      double value;
#if !PLATFORM_COMPACTFRAMEWORK
      value = UnsafeNativeMethods.sqlite3_column_double(stmt._sqlite_stmt, index);
#else
      UnsafeNativeMethods.sqlite3_column_double_interop(stmt._sqlite_stmt, index, out value);
#endif
      return value;
    }

    internal override int GetInt32(SqliteStatement stmt, int index)
    {
      return UnsafeNativeMethods.sqlite3_column_int(stmt._sqlite_stmt, index);
    }

    internal override long GetInt64(SqliteStatement stmt, int index)
    {
      long value;
#if !PLATFORM_COMPACTFRAMEWORK
      value = UnsafeNativeMethods.sqlite3_column_int64(stmt._sqlite_stmt, index);
#else
      UnsafeNativeMethods.sqlite3_column_int64_interop(stmt._sqlite_stmt, index, out value);
#endif
      return value;
    }

    internal override string GetText(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index), -1);
#endif
    }

    internal override DateTime GetDateTime(SqliteStatement stmt, int index)
    {
#if !SQLITE_STANDARD
      int len;
      return ToDateTime(UnsafeNativeMethods.sqlite3_column_text_interop(stmt._sqlite_stmt, index, out len), len);
#else
      return ToDateTime(UnsafeNativeMethods.sqlite3_column_text(stmt._sqlite_stmt, index), -1);
#endif
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

    internal override void CreateFunction(string strFunction, int nArgs, bool needCollSeq, SQLiteCallback func, SQLiteCallback funcstep, SQLiteFinalCallback funcfinal)
    {
      int n;

#if !SQLITE_STANDARD
      n = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
      if (n == 0) n = UnsafeNativeMethods.sqlite3_create_function_interop(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal, (needCollSeq == true) ? 1 : 0);
#else
      n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 4, IntPtr.Zero, func, funcstep, funcfinal);
      if (n == 0) n = UnsafeNativeMethods.sqlite3_create_function(_sql, ToUTF8(strFunction), nArgs, 1, IntPtr.Zero, func, funcstep, funcfinal);
#endif
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void CreateCollation(string strCollation, SQLiteCollation func, SQLiteCollation func16)
    {
      int n = UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 2, IntPtr.Zero, func16);
      if (n == 0) UnsafeNativeMethods.sqlite3_create_collation(_sql, ToUTF8(strCollation), 1, IntPtr.Zero, func);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, string s1, string s2)
    {
#if !SQLITE_STANDARD
      byte[] b1;
      byte[] b2;
      System.Text.Encoding converter = null;

      switch (enc)
      {
        case CollationEncodingEnum.UTF8:
          converter = System.Text.Encoding.UTF8;
          break;
        case CollationEncodingEnum.UTF16LE:
          converter = System.Text.Encoding.Unicode;
          break;
        case CollationEncodingEnum.UTF16BE:
          converter = System.Text.Encoding.BigEndianUnicode;
          break;
      }

      b1 = converter.GetBytes(s1);
      b2 = converter.GetBytes(s2);

      return UnsafeNativeMethods.sqlite3_context_collcompare(context, b1, b1.Length, b2, b2.Length);
#else
      throw new NotImplementedException();
#endif
    }

    internal override int ContextCollateCompare(CollationEncodingEnum enc, IntPtr context, char[] c1, char[] c2)
    {
#if !SQLITE_STANDARD
      byte[] b1;
      byte[] b2;
      System.Text.Encoding converter = null;

      switch (enc)
      {
        case CollationEncodingEnum.UTF8:
          converter = System.Text.Encoding.UTF8;
          break;
        case CollationEncodingEnum.UTF16LE:
          converter = System.Text.Encoding.Unicode;
          break;
        case CollationEncodingEnum.UTF16BE:
          converter = System.Text.Encoding.BigEndianUnicode;
          break;
      }

      b1 = converter.GetBytes(c1);
      b2 = converter.GetBytes(c2);

      return UnsafeNativeMethods.sqlite3_context_collcompare(context, b1, b1.Length, b2, b2.Length);
#else
      throw new NotImplementedException();
#endif
    }

    internal override CollationSequence GetCollationSequence(SqliteFunction func, IntPtr context)
    {
#if !SQLITE_STANDARD
      CollationSequence seq = new CollationSequence();
      int len;
      int type;
      int enc;
      IntPtr p = UnsafeNativeMethods.sqlite3_context_collseq(context, out type, out enc, out len);

      if (p != null) seq.Name = UTF8ToString(p, len);
      seq.Type = (CollationTypeEnum)type;
      seq._func = func;
      seq.Encoding = (CollationEncodingEnum)enc;

      return seq;
#else
      throw new NotImplementedException();
#endif
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
      double value;
#if !PLATFORM_COMPACTFRAMEWORK
      value = UnsafeNativeMethods.sqlite3_value_double(ptr);
#else
      UnsafeNativeMethods.sqlite3_value_double_interop(ptr, out value);
#endif
      return value;
    }

    internal override int GetParamValueInt32(IntPtr ptr)
    {
      return UnsafeNativeMethods.sqlite3_value_int(ptr);
    }

    internal override long GetParamValueInt64(IntPtr ptr)
    {
      Int64 value;
#if !PLATFORM_COMPACTFRAMEWORK
      value = UnsafeNativeMethods.sqlite3_value_int64(ptr);
#else
      UnsafeNativeMethods.sqlite3_value_int64_interop(ptr, out value);
#endif
      return value;
    }

    internal override string GetParamValueText(IntPtr ptr)
    {
#if !SQLITE_STANDARD
      int len;
      return UTF8ToString(UnsafeNativeMethods.sqlite3_value_text_interop(ptr, out len), len);
#else
      return UTF8ToString(UnsafeNativeMethods.sqlite3_value_text(ptr), -1);
#endif
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
#if !PLATFORM_COMPACTFRAMEWORK
      UnsafeNativeMethods.sqlite3_result_double(context, value);
#else
      UnsafeNativeMethods.sqlite3_result_double_interop(context, ref value);
#endif
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
#if !PLATFORM_COMPACTFRAMEWORK
      UnsafeNativeMethods.sqlite3_result_int64(context, value);
#else
      UnsafeNativeMethods.sqlite3_result_int64_interop(context, ref value);
#endif
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

    internal override void SetPassword(byte[] passwordBytes)
    {
      int n = UnsafeNativeMethods.sqlite3_key(_sql, passwordBytes, passwordBytes.Length);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }

    internal override void ChangePassword(byte[] newPasswordBytes)
    {
      int n = UnsafeNativeMethods.sqlite3_rekey(_sql, newPasswordBytes, (newPasswordBytes == null) ? 0 : newPasswordBytes.Length);
      if (n > 0) throw new SqliteException(n, SQLiteLastError());
    }
		
#if MONOTOUCH
    SQLiteUpdateCallback update_callback;
    SQLiteCommitCallback commit_callback;
    SQLiteRollbackCallback rollback_callback;
		
    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteUpdateCallback))]
    static void update (IntPtr puser, int type, IntPtr database, IntPtr table, Int64 rowid)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      instance.update_callback (puser, type, database, table, rowid);
    }
			
    internal override void SetUpdateHook (SQLiteUpdateCallback func)
    {
      update_callback = func;
      if (func == null)
        UnsafeNativeMethods.sqlite3_update_hook (_sql, null, IntPtr.Zero);
      else
        UnsafeNativeMethods.sqlite3_update_hook (_sql, update, GCHandle.ToIntPtr (gch));
    }

    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteCommitCallback))]
    static int commit (IntPtr puser)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      return instance.commit_callback (puser);
    }
		
    internal override void SetCommitHook (SQLiteCommitCallback func)
    {
      commit_callback = func;
      if (func == null)
        UnsafeNativeMethods.sqlite3_commit_hook (_sql, null, IntPtr.Zero);
      else
        UnsafeNativeMethods.sqlite3_commit_hook (_sql, commit, GCHandle.ToIntPtr (gch));
    }

    [MonoTouch.MonoPInvokeCallback (typeof (SQLiteRollbackCallback))]
    static void rollback (IntPtr puser)
    {
      SQLite3 instance = GCHandle.FromIntPtr (puser).Target as SQLite3;
      instance.rollback_callback (puser);
    }

    internal override void SetRollbackHook (SQLiteRollbackCallback func)
    {
      rollback_callback = func;
      if (func == null)
        UnsafeNativeMethods.sqlite3_rollback_hook (_sql, null, IntPtr.Zero);
      else
        UnsafeNativeMethods.sqlite3_rollback_hook (_sql, rollback, GCHandle.ToIntPtr (gch));
    }
#else
    internal override void SetUpdateHook(SQLiteUpdateCallback func)
    {
      UnsafeNativeMethods.sqlite3_update_hook(_sql, func, IntPtr.Zero);
    }

    internal override void SetCommitHook(SQLiteCommitCallback func)
    {
      UnsafeNativeMethods.sqlite3_commit_hook(_sql, func, IntPtr.Zero);
    }

    internal override void SetRollbackHook(SQLiteRollbackCallback func)
    {
      UnsafeNativeMethods.sqlite3_rollback_hook(_sql, func, IntPtr.Zero);
    }
#endif
    /// <summary>
    /// Helper function to retrieve a column of data from an active statement.
    /// </summary>
    /// <param name="stmt">The statement being step()'d through</param>
    /// <param name="index">The column index to retrieve</param>
    /// <param name="typ">The type of data contained in the column.  If Uninitialized, this function will retrieve the datatype information.</param>
    /// <returns>Returns the data in the column</returns>
    internal override object GetValue(SqliteStatement stmt, int index, SQLiteType typ)
    {
      if (IsNull(stmt, index)) return DBNull.Value;
      TypeAffinity aff = typ.Affinity;
      Type t = null;

      if (typ.Type != DbType.Object)
      {
        t = SqliteConvert.SQLiteTypeToType(typ);
        aff = TypeToAffinity(t);
      }

      switch (aff)
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
          if (t == null) return GetDouble(stmt, index);
          else
            return Convert.ChangeType(GetDouble(stmt, index), t, null);
        case TypeAffinity.Int64:
          if (t == null) return GetInt64(stmt, index);
          else
            return Convert.ChangeType(GetInt64(stmt, index), t, null);
        default:
          return GetText(stmt, index);
      }
    }

    internal override int GetCursorForTable(SqliteStatement stmt, int db, int rootPage)
    {
#if !SQLITE_STANDARD
      return UnsafeNativeMethods.sqlite3_table_cursor(stmt._sqlite_stmt, db, rootPage);
#else
      return -1;
#endif
    }

    internal override long GetRowIdForCursor(SqliteStatement stmt, int cursor)
    {
#if !SQLITE_STANDARD
      long rowid;
      int rc = UnsafeNativeMethods.sqlite3_cursor_rowid(stmt._sqlite_stmt, cursor, out rowid);
      if (rc == 0) return rowid;

      return 0;
#else
      return 0;
#endif
    }

    internal override void GetIndexColumnExtendedInfo(string database, string index, string column, out int sortMode, out int onError, out string collationSequence)
    {
#if !SQLITE_STANDARD
      IntPtr coll;
      int colllen;
      int rc;

      rc = UnsafeNativeMethods.sqlite3_index_column_info_interop(_sql, ToUTF8(database), ToUTF8(index), ToUTF8(column), out sortMode, out onError, out coll, out colllen);
      if (rc != 0) throw new SqliteException(rc, "");

      collationSequence = UTF8ToString(coll, colllen);
#else
      sortMode = 0;
      onError = 2;
      collationSequence = "BINARY";
#endif
    }
  }
}
