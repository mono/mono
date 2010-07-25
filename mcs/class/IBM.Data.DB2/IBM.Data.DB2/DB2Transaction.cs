
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
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace IBM.Data.DB2
{
	public sealed class DB2Transaction : MarshalByRefObject, IDbTransaction
	{
		private enum TransactionState
		{
			Open,
			Committed,
			Rolledback,
		}
		IsolationLevel isolationLevel;
		DB2Connection db2Conn;
		TransactionState state;
		


		internal DB2Transaction(DB2Connection con, IsolationLevel isoL)
		{
			long db2IsoL;
			db2Conn = con;
			short sqlRet;

			isolationLevel = isoL;

			switch (isoL) 
			{
				default:
				case System.Data.IsolationLevel.Chaos:				//No DB2equivalent, default to SQL_TXN_READ_COMMITTED
				case System.Data.IsolationLevel.ReadCommitted:		//SQL_TXN_READ_COMMITTED
					db2IsoL = DB2Constants.SQL_TXN_READ_COMMITTED;
					break;
				case System.Data.IsolationLevel.ReadUncommitted:	//SQL_TXN_READ_UNCOMMITTED
					db2IsoL = DB2Constants.SQL_TXN_READ_UNCOMMITTED;
					break;
				case System.Data.IsolationLevel.RepeatableRead:		//SQL_TXN_REPEATABLE_READ
					db2IsoL = DB2Constants.SQL_TXN_REPEATABLE_READ;
					break;
				case System.Data.IsolationLevel.Serializable:		//SQL_TXN_SERIALIZABLE_READ
					db2IsoL = DB2Constants.SQL_TXN_SERIALIZABLE_READ;
					break;
			}

			if(db2Conn.openConnection.autoCommit)
			{
				sqlRet = DB2CLIWrapper.SQLSetConnectAttr(db2Conn.DBHandle, DB2Constants.SQL_ATTR_AUTOCOMMIT, new IntPtr(DB2Constants.SQL_AUTOCOMMIT_OFF), 0);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting AUTOCOMMIT OFF in transaction CTOR.", db2Conn);
				db2Conn.openConnection.autoCommit = false;
			}
			sqlRet = DB2CLIWrapper.SQLSetConnectAttr(db2Conn.DBHandle, DB2Constants.SQL_ATTR_TXN_ISOLATION, new IntPtr(db2IsoL), 0);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting isolation level.", db2Conn);

			state = TransactionState.Open;
		}

		/// <summary>
		/// DB2Connection associated with this transaction
		/// </summary>
		public IDbConnection Connection
		{
			get
			{
				return db2Conn;
			}
		}
		/// <summary>
		/// IsolationLevel property
		/// </summary>
		/// 
		public IsolationLevel IsolationLevel
		{
			get 
			{
				CheckStateOpen();
				return isolationLevel;
			}
		}

		internal void CheckStateOpen()
		{
			if(state == TransactionState.Committed)
				throw new InvalidOperationException("Transaction was already committed. It is no longer usable.");
			if(state == TransactionState.Rolledback)
				throw new InvalidOperationException("Transaction was already rolled back. It is no longer usable.");
		}

		public void Commit() 
		{
			CheckStateOpen();
			DB2CLIWrapper.SQLEndTran(DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, DB2Constants.SQL_COMMIT);
			this.state = TransactionState.Committed;
			this.db2Conn.openConnection.transactionOpen = false;
			this.db2Conn.WeakRefTransaction = null;
			this.db2Conn = null;
		}

		public void Rollback() 
		{
			CheckStateOpen();
			DB2CLIWrapper.SQLEndTran(DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, DB2Constants.SQL_ROLLBACK);
			this.db2Conn.openConnection.transactionOpen = false;
			this.state = TransactionState.Rolledback;
			this.db2Conn.WeakRefTransaction = null;
			this.db2Conn = null;
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			if (state != TransactionState.Open) 
				return;

			Rollback();
		}
	}
}
