//
// Mono.Data.Sqlite.SQLiteBase.cs
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
  using System.Data;
  using System.Runtime.InteropServices;
  using System.Collections.Generic;

  /// <summary>
  /// This internal class provides the foundation of Sqlite support.  It defines all the abstract members needed to implement
  /// a Sqlite data provider, and inherits from SqliteConvert which allows for simple translations of string to and from Sqlite.
  /// </summary>
  internal abstract class SqliteBase : SqliteConvert, IDisposable
  {
    internal SqliteBase(SqliteDateFormats fmt)
      : base(fmt) {}

    /// <summary>
    /// Returns a string representing the active version of Sqlite
    /// </summary>
    internal abstract string Version { get; }
    /// <summary>
    /// Returns the number of changes the last executing insert/update caused.
    /// </summary>
    internal abstract int    Changes { get; }
    /// <summary>
    /// Opens a database.
    /// </summary>
    /// <remarks>
    /// Implementers should call SqliteFunction.BindFunctions() and save the array after opening a connection
    /// to bind all attributed user-defined functions and collating sequences to the new connection.
    /// </remarks>
    /// <param name="strFilename">The filename of the database to open.  Sqlite automatically creates it if it doesn't exist.</param>
    internal abstract void   Open(string strFilename);
    /// <summary>
    /// Closes the currently-open database.
    /// </summary>
    /// <remarks>
    /// After the database has been closed implemeters should call SqliteFunction.UnbindFunctions() to deallocate all interop allocated
    /// memory associated with the user-defined functions and collating sequences tied to the closed connection.
    /// </remarks>
    internal abstract void   Close();
    /// <summary>
    /// Sets the busy timeout on the connection.  SqliteCommand will call this before executing any command.
    /// </summary>
    /// <param name="nTimeoutMS">The number of milliseconds to wait before returning SQLITE_BUSY</param>
    internal abstract void   SetTimeout(int nTimeoutMS);
    /// <summary>
    /// Returns the text of the last error issued by Sqlite
    /// </summary>
    /// <returns></returns>
    internal abstract string SqliteLastError();

    /// <summary>
    /// Prepares a SQL statement for execution.
    /// </summary>
    /// <param name="strSql">The SQL command text to prepare</param>
    /// <param name="previous">The previous statement in a multi-statement command, or null if no previous statement exists</param>
    /// <param name="strRemain">The remainder of the statement that was not processed.  Each call to prepare parses the
    /// SQL up to to either the end of the text or to the first semi-colon delimiter.  The remaining text is returned
    /// here for a subsequent call to Prepare() until all the text has been processed.</param>
    /// <returns>Returns an initialized SqliteStatement.</returns>
    internal abstract SqliteStatement Prepare(string strSql, SqliteStatement previous, out string strRemain);
    /// <summary>
    /// Steps through a prepared statement.
    /// </summary>
    /// <param name="stmt">The SqliteStatement to step through</param>
    /// <returns>True if a row was returned, False if not.</returns>
    internal abstract bool Step(SqliteStatement stmt);
    /// <summary>
    /// Finalizes a prepared statement.
    /// </summary>
    /// <param name="stmt">The statement to finalize</param>
    internal abstract void FinalizeStatement(SqliteStatement stmt);
    /// <summary>
    /// Resets a prepared statement so it can be executed again.  If the error returned is SQLITE_SCHEMA, 
    /// transparently attempt to rebuild the SQL statement and throw an error if that was not possible.
    /// </summary>
    /// <param name="stmt">The statement to reset</param>
    /// <returns>Returns -1 if the schema changed while resetting, 0 if the reset was sucessful or 6 (SQLITE_LOCKED) if the reset failed due to a lock</returns>
    internal abstract int Reset(SqliteStatement stmt);

    internal abstract void Cancel();

    internal abstract void Bind_Double(SqliteStatement stmt, int index, double value);
    internal abstract void Bind_Int32(SqliteStatement stmt, int index, Int32 value);
    internal abstract void Bind_Int64(SqliteStatement stmt, int index, Int64 value);
    internal abstract void Bind_Text(SqliteStatement stmt, int index, string value);
    internal abstract void Bind_Blob(SqliteStatement stmt, int index, byte[] blobData);
    internal abstract void Bind_DateTime(SqliteStatement stmt, int index, DateTime dt);
    internal abstract void Bind_Null(SqliteStatement stmt, int index);

    internal abstract int    Bind_ParamCount(SqliteStatement stmt);
    internal abstract string Bind_ParamName(SqliteStatement stmt, int index);
    internal abstract int    Bind_ParamIndex(SqliteStatement stmt, string paramName);

    internal abstract int    ColumnCount(SqliteStatement stmt);
    internal abstract string ColumnName(SqliteStatement stmt, int index);
    internal abstract TypeAffinity ColumnAffinity(SqliteStatement stmt, int index);
    internal abstract string ColumnType(SqliteStatement stmt, int index, out TypeAffinity nAffinity);
    internal abstract int    ColumnIndex(SqliteStatement stmt, string columnName);
    internal abstract string ColumnOriginalName(SqliteStatement stmt, int index);
    internal abstract string ColumnDatabaseName(SqliteStatement stmt, int index);
    internal abstract string ColumnTableName(SqliteStatement stmt, int index);
    internal abstract void ColumnMetaData(string dataBase, string table, string column, out string dataType, out string collateSequence, out bool notNull, out bool primaryKey, out bool autoIncrement);

    internal abstract double   GetDouble(SqliteStatement stmt, int index);
    internal abstract Int32    GetInt32(SqliteStatement stmt, int index);
    internal abstract Int64    GetInt64(SqliteStatement stmt, int index);
    internal abstract string   GetText(SqliteStatement stmt, int index);
    internal abstract long     GetBytes(SqliteStatement stmt, int index, int nDataoffset, byte[] bDest, int nStart, int nLength);
    internal abstract long     GetChars(SqliteStatement stmt, int index, int nDataoffset, char[] bDest, int nStart, int nLength);
    internal abstract DateTime GetDateTime(SqliteStatement stmt, int index);
    internal abstract bool     IsNull(SqliteStatement stmt, int index);

    internal abstract void  CreateCollation(string strCollation, SqliteCollation func);
    internal abstract void  CreateFunction(string strFunction, int nArgs, SqliteCallback func, SqliteCallback funcstep, SqliteFinalCallback funcfinal);

    internal abstract int AggregateCount(IntPtr context);
    internal abstract IntPtr AggregateContext(IntPtr context);

    internal abstract long   GetParamValueBytes(IntPtr ptr, int nDataOffset, byte[] bDest, int nStart, int nLength);
    internal abstract double GetParamValueDouble(IntPtr ptr);
    internal abstract int    GetParamValueInt32(IntPtr ptr);
    internal abstract Int64  GetParamValueInt64(IntPtr ptr);
    internal abstract string GetParamValueText(IntPtr ptr);
    internal abstract TypeAffinity GetParamValueType(IntPtr ptr);

    internal abstract void ReturnBlob(IntPtr context, byte[] value);
    internal abstract void ReturnDouble(IntPtr context, double value);
    internal abstract void ReturnError(IntPtr context, string value);
    internal abstract void ReturnInt32(IntPtr context, Int32 value);
    internal abstract void ReturnInt64(IntPtr context, Int64 value);
    internal abstract void ReturnNull(IntPtr context);
    internal abstract void ReturnText(IntPtr context, string value);

    internal abstract void SetUpdateHook(SqliteUpdateCallback func);
    internal abstract void SetCommitHook(SqliteCommitCallback func);
    internal abstract void SetRollbackHook(SqliteRollbackCallback func);

    internal abstract int GetLastInsertRowId ();
    
    internal abstract object GetValue(SqliteStatement stmt, int index, ref SqliteType typ);

    protected virtual void Dispose(bool bDisposing)
    {
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}
#endif
