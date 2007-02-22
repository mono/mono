//
// Mono.Data.Sqlite.SQLiteTransaction.cs
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
  using System.Data.Common;

  /// <summary>
  /// Sqlite implementation of DbTransaction.
  /// </summary>
  public sealed class SqliteTransaction : DbTransaction
  {
    /// <summary>
    /// The connection to which this transaction is bound
    /// </summary>
    internal SqliteConnection _cnn;
    internal long _version; // Matches the version of the connection

    /// <summary>
    /// Constructs the transaction object, binding it to the supplied connection
    /// </summary>
    /// <param name="connection">The connection to open a transaction on</param>
    /// <param name="deferredLock">TRUE to defer the writelock, or FALSE to lock immediately</param>
    internal SqliteTransaction(SqliteConnection connection, bool deferredLock)
    {
      _cnn = connection;
      _version = _cnn._version;

      if (_cnn._transactionLevel++ == 0)
      {
        try
        {
          using (SqliteCommand cmd = _cnn.CreateCommand())
          {
            if (!deferredLock)
              cmd.CommandText = "BEGIN IMMEDIATE";
            else
              cmd.CommandText = "BEGIN";

            cmd.ExecuteNonQuery();
          }
        }
        catch (SqliteException)
        {
          _cnn._transactionLevel--;
          _cnn = null;
          throw;
        }
      }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public override void Commit()
    {
      IsValid(true);

      if (--_cnn._transactionLevel == 0)
      {
        try
        {
          using (SqliteCommand cmd = _cnn.CreateCommand())
          {
            cmd.CommandText = "COMMIT";
            cmd.ExecuteNonQuery();
          }
        }
        finally
        {
          _cnn = null;
        }
      }
      else
      {
        _cnn = null;
      }
    }

    /// <summary>
    /// Returns the underlying connection to which this transaction applies.
    /// </summary>
    public new SqliteConnection Connection
    {
      get { return _cnn; }
    }

    /// <summary>
    /// Forwards to the local Connection property
    /// </summary>
    protected override DbConnection DbConnection
    {
      get { return Connection; }
    }

    /// <summary>
    /// Disposes the transaction.  If it is currently active, any changes are rolled back.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (IsValid(false))
        Rollback();

      _cnn = null;

      base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the isolation level of the transaction.  Sqlite only supports Serializable transactions.
    /// </summary>
    public override IsolationLevel IsolationLevel
    {
      get { return IsolationLevel.Serializable; }
    }

    /// <summary>
    /// Rolls back the active transaction.
    /// </summary>
    public override void Rollback()
    {
      IsValid(true);

      try
      {
        using (SqliteCommand cmd = _cnn.CreateCommand())
        {
          cmd.CommandText = "ROLLBACK";
          cmd.ExecuteNonQuery();
        }
        _cnn._transactionLevel = 0;
      }
      finally
      {
        _cnn = null;
      }
    }

    internal bool IsValid(bool throwError)
    {
      if (_cnn == null)
      {
        if (throwError == true) throw new ArgumentNullException("No connection associated with this transaction");
        else return false;
      }

      if (_cnn._transactionLevel == 0)
      {
        if (throwError == true) throw new SqliteException((int)SqliteErrorCode.Misuse, "No transaction is active on this connection");
        else return false;
      }
      if (_cnn._version != _version)
      {
        if (throwError == true) throw new SqliteException((int)SqliteErrorCode.Misuse, "The connection was closed and re-opened, changes were rolled back");
        else return false;
      }
      if (_cnn.State != ConnectionState.Open)
      {
        if (throwError == true) throw new SqliteException((int)SqliteErrorCode.Misuse, "Connection was closed");
        else return false;
      }

      return true;
    }
  }
}
#endif
