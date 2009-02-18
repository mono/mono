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

#if MONO_STRICT
using System.Data.Linq.Sql;
using System.Data.Linq;
#else
using DbLinq.Data.Linq.Sql;
#endif

namespace DbLinq.Data.Linq.Database.Implementation
{
    /// <summary>
    /// Transactional command
    /// </summary>
    public class TransactionalCommand : ITransactionalCommand
    {
        private readonly IDisposable _connection;
        /// <summary>
        /// Ambient transaction
        /// </summary>
        private readonly IDatabaseTransaction _transaction;

        private readonly IDbCommand _command;
        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>The command.</value>
        public IDbCommand Command
        {
            get
            {
                return _command;
            }
        }


        /// <summary>
        /// Commits current transaction.
        /// </summary>
        public virtual void Dispose()
        {
            Command.Dispose();
            if (_transaction != null)
                _transaction.Dispose();
            _connection.Dispose();
        }

        /// <summary>
        /// Commits the current transaction.
        /// throws NRE if _transaction is null. Behavior is intentional.
        /// </summary>
        public void Commit()
        {
            // TODO: do not commit if participating in a higher transaction
            _transaction.Commit();
        }

        public TransactionalCommand(string commandText, bool createTransaction, DataContext dataContext)
        {
            // TODO: check if all this stuff is necessary
            // the OpenConnection() checks that the connection is already open
            // TODO: see if we can move this here (in theory the final DataContext shouldn't use)
            _connection = dataContext.DatabaseContext.OpenConnection();
            // the transaction is optional
            if (createTransaction)
                _transaction = dataContext.DatabaseContext.Transaction();
            _command = dataContext.DatabaseContext.CreateCommand();
            Command.CommandText = commandText;
            if (createTransaction)
                Command.Transaction = _transaction.Transaction;
        }

    }
}
