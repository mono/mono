//
// System.Data.SqlClient.SqlTransaction.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//

// use #define DEBUG_SqlTransaction if you want to spew debug messages
// #define DEBUG_SqlTransaction


using System;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a transaction to be performed on a SQL database.
	/// </summary>
	// public sealed class SqlTransaction : MarshalByRefObject,
	//	IDbTransaction, IDisposable
	public sealed class SqlTransaction : IDbTransaction
	{
		#region Fields

		private bool doingTransaction = false;
		private SqlConnection conn = null;
		private IsolationLevel isolationLevel =	
			IsolationLevel.ReadCommitted;
		// There are only two IsolationLevel's for PostgreSQL:
		//    ReadCommitted and Serializable, 
		// but ReadCommitted is the default 
		
		#endregion
               
		#region Public Methods

		[MonoTODO]
		public void Commit ()
		{
			if(doingTransaction == false)
				throw new InvalidOperationException(
					"Begin transaction was not " +
					"done earlier " +
					"thus PostgreSQL can not " +
					"Commit transaction.");
			
			SqlCommand cmd = new SqlCommand("COMMIT", conn);
			cmd.ExecuteNonQuery();
						
			doingTransaction = false;
		}		

		[MonoTODO]
		public void Rollback()
		{
			if(doingTransaction == false)
				throw new InvalidOperationException(
					"Begin transaction was not " +
					"done earlier " +
					"thus PostgreSQL can not " +
					"Rollback transaction.");
			
			SqlCommand cmd = new SqlCommand("ROLLBACK", conn);
			cmd.ExecuteNonQuery();
						
			doingTransaction = false;
		}

		// For PostgreSQL, Rollback(string) will not be implemented
		// because PostgreSQL does not support Savepoints
		[Obsolete]
		public void Rollback(string transactionName) {
			// throw new NotImplementedException ();
			Rollback();
		}

		// For PostgreSQL, Save(string) will not be implemented
		// because PostgreSQL does not support Savepoints
		[Obsolete]
		public void Save (string savePointName) {
			// throw new NotImplementedException ();
		}

		#endregion // Public Methods

		#region Internal Methods to System.Data.dll Assembly

		internal void Begin()
		{
			if(doingTransaction == true)
				throw new InvalidOperationException(
					"Transaction has begun " +
					"and PostgreSQL does not " +
					"support nested transactions.");
			
			SqlCommand cmd = new SqlCommand("BEGIN", conn);
			cmd.ExecuteNonQuery();
						
			doingTransaction = true;
		}

		internal void SetIsolationLevel(IsolationLevel isoLevel)
		{
			String sSql = "SET TRANSACTION ISOLATION LEVEL ";
 
			switch (isoLevel) 
			{
				case IsolationLevel.ReadCommitted:
					sSql += "READ COMMITTED";
					break;

				case IsolationLevel.Serializable:
					sSql += "SERIALIZABLE";
					break;
				default:
					// FIXME: generate exception here
					// PostgreSQL only supports:
					//   ReadCommitted or Serializable
					break;
			}
			SqlCommand cmd = new SqlCommand(sSql, conn);
			cmd.ExecuteNonQuery();

			this.isolationLevel = isoLevel;
		}

		internal void SetConnection(SqlConnection connection)
		{
			this.conn = connection;
		}

		#endregion // Internal Methods to System.Data.dll Assembly

		#region Properties

		IDbConnection IDbTransaction.Connection	{
			get { 
				return Connection; 
			}
		}

		public SqlConnection Connection	{
			get { 
				return conn; 
			}
		}

		public IsolationLevel IsolationLevel {
			get { 
				return isolationLevel; 
			}
		}

		internal bool DoingTransaction {
			get {
				return doingTransaction;
			}
		}

		#endregion Properties

		#region Destructors

		// Destructors aka Finalize and Dispose

		[MonoTODO]
		public void Dispose()
		{
			// FIXME: need to properly release resources
			// Dispose(true);
		}

		// Destructor 
		[MonoTODO]
		// [Serializable]
		// [ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlTransaction() {
			// FIXME: need to properly release resources
			// Dispose(false);
		}

		#endregion // Destructors

	}
}
