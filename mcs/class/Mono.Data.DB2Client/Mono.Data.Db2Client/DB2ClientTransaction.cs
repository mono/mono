#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace DB2ClientCS
{
	/// <summary>
	/// DB2ClientTransaction.  DB2 requires nothing specific to be done to open a transaction, so we set the 
	/// isolation level, make sure AUTOCOMMIT is off.  DB2 Also allows you to set transaction isolation
	/// at the statement level, but I haven't worked that in here yet.
	/// NOTE:  AT THE MOMENT, SETTING OF THE ISOLATION LEVEL IS NOT WORKING
	/// </summary>
	unsafe public class DB2ClientTransaction : IDbTransaction
	{
		DB2ClientUtils util = new DB2ClientUtils();
		IsolationLevel IL = IsolationLevel.Unspecified;
		DB2ClientConnection db2Conn;
		public DB2ClientTransaction(DB2ClientConnection con, IsolationLevel isoL)
		{
			long db2IsoL = DB2ClientConstants.SQL_TXN_READ_COMMITTED;
			db2Conn = con;
			short sqlRet;

			switch (isoL) 
			{
				case System.Data.IsolationLevel.Chaos:			//No DB2equivalent, default to SQL_TXN_READ_COMMITTED
					break;
				case System.Data.IsolationLevel.ReadCommitted:		//SQL_TXN_READ_COMMITTED
					db2IsoL = DB2ClientConstants.SQL_TXN_READ_COMMITTED;
					break;
				case System.Data.IsolationLevel.ReadUncommitted:		//SQL_TXN_READ_UNCOMMITTED
					db2IsoL = DB2ClientConstants.SQL_TXN_READ_UNCOMMITTED;
					break;
				case System.Data.IsolationLevel.RepeatableRead:		//SQL_TXN_REPEATABLE_READ
					db2IsoL = DB2ClientConstants.SQL_TXN_REPEATABLE_READ;
					break;
				case System.Data.IsolationLevel.Serializable:			//SQL_TXN_SERIALIZABLE_READ
					db2IsoL = DB2ClientConstants.SQL_TXN_SERIALIZABLE_READ;
					break;
			}

			IL = isoL;
			IntPtr iso = new IntPtr(db2IsoL);
			IntPtr attr = new IntPtr(DB2ClientConstants.SQL_AUTOCOMMIT_OFF);
			sqlRet = DB2ClientPrototypes.SQLSetConnectAttr(db2Conn.DBHandle, DB2ClientConstants.SQL_AUTOCOMMIT, attr, 0);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting AUTOCOMMIT OFF in transaction CTOR.");
//			sqlRet = DB2ClientPrototypes.SQLSetConnectAttr(db2Conn.DBHandle, DB2ClientConstants.SQL_TXN_ISOLATION, iso, 0);
//			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting TRANSCTION ISOLATION in transaction CTOR.");
			
		}

		/// <summary>
		/// DB2ClientConnection associated with this transaction
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
			DB2ClientPrototypes.SQLEndTran(DB2ClientConstants.SQL_HANDLE_DBC, db2Conn.DBHandle, DB2ClientConstants.SQL_COMMIT);
		}
		public void Rollback() 
		{
			DB2ClientPrototypes.SQLEndTran(DB2ClientConstants.SQL_HANDLE_DBC, db2Conn.DBHandle, DB2ClientConstants.SQL_ROLLBACK);
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
