//
// System.Data.ProviderBase.AbstractTransaction
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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

using System.Data.Common;

namespace System.Data.ProviderBase
{

    using java.sql;

    using System.Data;

    public abstract class AbstractTransaction : DbTransaction
    {

        protected String _transactionName;
        protected AbstractDBConnection _connection;

        protected IsolationLevel _isolationLevel;

        public AbstractTransaction(
            IsolationLevel isolationLevel,
            AbstractDBConnection connection,
            String transactionName)
        {
			connection.ValidateBeginTransaction();
            _transactionName = transactionName;
            _connection = connection;
            _isolationLevel = isolationLevel;
            try
            {
                _connection.JdbcConnection.setAutoCommit(false);
                _connection.JdbcConnection.setTransactionIsolation(
                convertIsolationLevel(isolationLevel));
            }
            catch (SQLException exp)
            {
                throw new System.InvalidOperationException(exp.Message, exp);
            }
        }

        
        /**
         * @see System.Data.IDbTransaction#Connection
         */
        protected override DbConnection DbConnection
        {
            get
            {
                return _connection;
            }
        }

        /**
         * @see System.Data.IDbTransaction#IsolationLevel
         */
        public override IsolationLevel IsolationLevel
        {
            get
            {
                return _isolationLevel;
            }
        }

        /**
         * @see System.Data.IDbTransaction#Commit()
         */
        public override void Commit()
        {
			if (_connection == null)
				return;

            try
            {
                _connection.JdbcConnection.commit();
				_connection.JdbcConnection.setAutoCommit(true);
				_connection = null;
            }
            catch (SQLException exp)
            {
                throw new SystemException(exp.Message, exp);
            }
        }

        /**
         * @see System.Data.IDbTransaction#Rollback()
         */
        public override void Rollback()
        {
			if (_connection == null)
				return;

            try
            {
                _connection.JdbcConnection.rollback();
				_connection.JdbcConnection.setAutoCommit(true);
				_connection = null;
            }
            catch (SQLException exp)
            {
                throw new SystemException(exp.Message, exp);
            }
        }

		internal AbstractTransaction ActiveTransaction {
			get {
				// recoursively return parent transaction when nesting will
				// be implemented
				return _connection != null ? this : null;
			}
		}

        private int convertIsolationLevel(IsolationLevel isolationLevel)
        {
            if (isolationLevel == IsolationLevel.Unspecified)
                return vmw.@internal.sql.ConnectionUtils__Finals.TRANSACTION_NONE;
            if (isolationLevel == IsolationLevel.ReadCommitted)
                return vmw.@internal.sql.ConnectionUtils__Finals.TRANSACTION_READ_COMMITTED;
            if (isolationLevel == IsolationLevel.ReadUncommitted)
                return vmw.@internal.sql.ConnectionUtils__Finals.TRANSACTION_READ_UNCOMMITTED;
            if (isolationLevel == IsolationLevel.RepeatableRead)
                return vmw.@internal.sql.ConnectionUtils__Finals.TRANSACTION_REPEATABLE_READ;
            if (isolationLevel == IsolationLevel.Serializable)
                return vmw.@internal.sql.ConnectionUtils__Finals.TRANSACTION_SERIALIZABLE;

            throw new NotSupportedException("The Isolation level '" + isolationLevel + "' is not supported");
        }
    }
}