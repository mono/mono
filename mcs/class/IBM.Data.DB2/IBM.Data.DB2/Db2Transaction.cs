using System;
using System.Data;
using System.Runtime.InteropServices;

namespace IBM.Data.DB2
{
	public class DB2Transaction : IDbTransaction
	{
		IsolationLevel IL = IsolationLevel.Unspecified;
		DB2Connection DB2Conn;
		public DB2Transaction(DB2Connection con, IsolationLevel isoL)
		{
			long DB2IsoL = DB2Constants.SQL_TXN_READ_COMMITTED;
			DB2Conn = con;
			short sqlRet;

			switch (isoL) 
			{
				case System.Data.IsolationLevel.Chaos:			//No DB2equivalent, default to SQL_TXN_READ_COMMITTED
					break;
				case System.Data.IsolationLevel.ReadCommitted:		//SQL_TXN_READ_COMMITTED
					DB2IsoL = DB2Constants.SQL_TXN_READ_COMMITTED;
					break;
				case System.Data.IsolationLevel.ReadUncommitted:		//SQL_TXN_READ_UNCOMMITTED
					DB2IsoL = DB2Constants.SQL_TXN_READ_UNCOMMITTED;
					break;
				case System.Data.IsolationLevel.RepeatableRead:		//SQL_TXN_REPEATABLE_READ
					DB2IsoL = DB2Constants.SQL_TXN_REPEATABLE_READ;
					break;
				case System.Data.IsolationLevel.Serializable:			//SQL_TXN_SERIALIZABLE_READ
					DB2IsoL = DB2Constants.SQL_TXN_SERIALIZABLE_READ;
					break;
			}

			IL = isoL;
			IntPtr iso = new IntPtr(DB2IsoL);
			IntPtr attr = new IntPtr(DB2Constants.SQL_AUTOCOMMIT_OFF);
			sqlRet = DB2CLIWrapper.SQLSetConnectAttr(DB2Conn.DBHandle, DB2Constants.SQL_AUTOCOMMIT, attr, 0);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, DB2Conn.DBHandle, "Error setting AUTOCOMMIT OFF in transaction CTOR.");
//	
			
		}

		/// <summary>
		/// DB2Connection associated with this transaction
		/// </summary>
		public IDbConnection Connection
		{
			get
			{
				return DB2Conn;
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
			DB2CLIWrapper.SQLEndTran(DB2Constants.SQL_HANDLE_DBC, DB2Conn.DBHandle, DB2Constants.SQL_COMMIT);
			//this.DB2Conn.WeakRefTransaction = null;
			//this.DB2Conn = null;
		}

		public void Rollback() 
		{
			DB2CLIWrapper.SQLEndTran(DB2Constants.SQL_HANDLE_DBC, DB2Conn.DBHandle, DB2Constants.SQL_ROLLBACK);
			//this.DB2Conn.WeakRefTransaction = null;
			//this.DB2Conn = null;
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
			DB2Conn = null;
			Done = true;
		}
	}
}
