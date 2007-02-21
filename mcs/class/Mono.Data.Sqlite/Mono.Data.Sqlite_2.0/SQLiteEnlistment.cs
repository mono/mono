//
// Mono.Data.Sqlite.SQLiteEnlistment.cs
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

/*******************************************************
 * ADO.NET 2.0 Data Provider for Sqlite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
#if !PLATFORM_COMPACTFRAMEWORK
namespace Mono.Data.Sqlite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Transactions;

  internal class SqliteEnlistment : IEnlistmentNotification
  {
    internal SqliteTransaction _transaction;
    internal Transaction _scope;
    internal bool _disposeConnection;

    internal SqliteEnlistment(SqliteConnection cnn, Transaction scope)
    {
      _transaction = cnn.BeginTransaction();
      _scope = scope;
      _disposeConnection = false;

      _scope.EnlistVolatile(this, System.Transactions.EnlistmentOptions.None);
    }

    private void Cleanup(SqliteConnection cnn)
    {
      if (_disposeConnection)
        cnn.Dispose();

      _transaction = null;
      _scope = null;
    }

    #region IEnlistmentNotification Members

    public void Commit(Enlistment enlistment)
    {
      SqliteConnection cnn = _transaction.Connection;
      cnn._enlistment = null;

      try
      {
        _transaction.IsValid(true);
        _transaction.Connection._transactionLevel = 1;
        _transaction.Commit();

        enlistment.Done();
      }
      finally
      {
        Cleanup(cnn);
      }
    }

    public void InDoubt(Enlistment enlistment)
    {
      enlistment.Done();
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
      if (_transaction.IsValid(false) == false)
        preparingEnlistment.ForceRollback();
      else
        preparingEnlistment.Prepared();
    }

    public void Rollback(Enlistment enlistment)
    {
      SqliteConnection cnn = _transaction.Connection;
      cnn._enlistment = null;

      try
      {
        _transaction.Rollback();
        enlistment.Done();
      }
      finally
      {
        Cleanup(cnn);
      }
    }

    #endregion
  }
}
#endif // !PLATFORM_COMPACT_FRAMEWORK
#endif
