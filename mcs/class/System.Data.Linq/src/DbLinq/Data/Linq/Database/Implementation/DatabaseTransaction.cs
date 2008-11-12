
#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Data;

namespace DbLinq.Data.Linq.Database.Implementation
{
    /// <summary>
    /// Default database transaction implementation
    /// </summary>
    internal class DatabaseTransaction : IDatabaseTransaction
    {
        [ThreadStatic]
        private static DatabaseTransaction _currentTransaction;

        private IDbTransaction _transaction;

        /// <summary>
        /// Gets the current db transaction.
        /// </summary>
        /// <value>The current db transaction.</value>
        public static IDbTransaction currentTransaction
        {
            get
            {
                if (_currentTransaction != null)
                    return _currentTransaction._transaction;
                return null;
            }
        }

        /// <summary>
        /// Returns current transaction (if any)
        /// </summary>
        /// <value></value>
        public IDbTransaction Transaction { get { return currentTransaction; } }

        public DatabaseTransaction(IDbConnection connection)
        {
            if (_currentTransaction != null)
                return;

            _transaction = connection.BeginTransaction();
            _currentTransaction = this;
        }

        /// <summary>
        /// Call Commit() before Dispose() to save changes.
        /// All unCommit()ed changes will be rolled back
        /// </summary>
        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                // once the transaction committed, dispose it
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_transaction != null)
            {
                // if we are here, the Commit() was not called
                _transaction.Rollback();
                _transaction.Dispose();
            }
            if (_currentTransaction == this)
                _currentTransaction = null;
        }
    }
}
