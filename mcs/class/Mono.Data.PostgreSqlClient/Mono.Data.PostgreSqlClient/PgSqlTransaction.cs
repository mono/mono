//
// Mono.Data.PostgreSqlClient.PgSqlTransaction.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
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

// use #define DEBUG_SqlTransaction if you want to spew debug messages
// #define DEBUG_SqlTransaction


using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Represents a transaction to be performed on a SQL database.
	/// </summary>
	// public sealed class PgSqlTransaction : MarshalByRefObject,
	//	IDbTransaction, IDisposable
	public sealed class PgSqlTransaction : IDbTransaction
	{
		#region Fields

		private bool doingTransaction = false;
		private PgSqlConnection conn = null;
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
			
			PgSqlCommand cmd = new PgSqlCommand("COMMIT", conn);
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
			
			PgSqlCommand cmd = new PgSqlCommand("ROLLBACK", conn);
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

		#region Internal Methods to Mono.Data.PostgreSqlClient.dll Assembly

		internal void Begin()
		{
			if(doingTransaction == true)
				throw new InvalidOperationException(
					"Transaction has begun " +
					"and PostgreSQL does not " +
					"support nested transactions.");
			
			PgSqlCommand cmd = new PgSqlCommand("BEGIN", conn);
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
			PgSqlCommand cmd = new PgSqlCommand(sSql, conn);
			cmd.ExecuteNonQuery();

			this.isolationLevel = isoLevel;
		}

		internal void SetConnection(PgSqlConnection connection)
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

		public PgSqlConnection Connection	{
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
		~PgSqlTransaction() {
			// FIXME: need to properly release resources
			// Dispose(false);
		}

		#endregion // Destructors

	}
}
