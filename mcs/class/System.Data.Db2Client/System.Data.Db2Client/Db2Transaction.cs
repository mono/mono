using System;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.Db2Client
{
	public class Db2Transaction : IDbTransaction
	{
		IsolationLevel IL = IsolationLevel.Unspecified;
		Db2Connection db2Conn;
		public Db2Transaction(Db2Connection con, IsolationLevel isoL)
		{
			long db2IsoL = Db2Constants.SQL_TXN_READ_COMMITTED;
			db2Conn = con;
			short sqlRet;

			switch (isoL) 
			{
				case System.Data.IsolationLevel.Chaos:			//No Db2equivalent, default to SQL_TXN_READ_COMMITTED
					break;
				case System.Data.IsolationLevel.ReadCommitted:		//SQL_TXN_READ_COMMITTED
					db2IsoL = Db2Constants.SQL_TXN_READ_COMMITTED;
					break;
				case System.Data.IsolationLevel.ReadUncommitted:		//SQL_TXN_READ_UNCOMMITTED
					db2IsoL = Db2Constants.SQL_TXN_READ_UNCOMMITTED;
					break;
				case System.Data.IsolationLevel.RepeatableRead:		//SQL_TXN_REPEATABLE_READ
					db2IsoL = Db2Constants.SQL_TXN_REPEATABLE_READ;
					break;
				case System.Data.IsolationLevel.Serializable:			//SQL_TXN_SERIALIZABLE_READ
					db2IsoL = Db2Constants.SQL_TXN_SERIALIZABLE_READ;
					break;
			}

			IL = isoL;
			IntPtr iso = new IntPtr(db2IsoL);
			IntPtr attr = new IntPtr(Db2Constants.SQL_AUTOCOMMIT_OFF);
			sqlRet = Db2CLIWrapper.SQLSetConnectAttr(db2Conn.DBHandle, Db2Constants.SQL_AUTOCOMMIT, attr, 0);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting AUTOCOMMIT OFF in transaction CTOR.");
//	
			
		}

		/// <summary>
		/// Db2Connection associated with this transaction
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
				return IL;
			}
		}

		public void Commit() {
			Db2CLIWrapper.SQLEndTran(Db2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, Db2Constants.SQL_COMMIT);
			this.db2Conn.WeakRefTransaction = null;
			this.db2Conn = null;
		}

		public void Rollback() 
		{
			Db2CLIWrapper.SQLEndTran(Db2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, Db2Constants.SQL_ROLLBACK);
			this.db2Conn.WeakRefTransaction = null;
			this.db2Conn = null;
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		private bool Done = false;
		public void Dispose()
		{
			if (Done) 
				return;
			Rollback();
			IL = IsolationLevel.Unspecified;
			db2Conn = null;
			Done = true;
		}
	}
}
