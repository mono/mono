//
// System.Data.OleDb.OleDbTransaction
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Fields

		OleDbConnection connection;
		IntPtr gdaTransaction;
		int depth;

		#endregion // Fields

		#region Constructors

		internal OleDbTransaction (OleDbConnection connection, int depth)
			: this (connection, depth, IsolationLevel.ReadCommitted)
		{
		}

		internal OleDbTransaction (OleDbConnection connection)
			: this (connection, 1)
		{
		}

		internal OleDbTransaction (OleDbConnection connection, int depth, IsolationLevel isolevel) 
		{
			this.connection = connection;

			gdaTransaction = libgda.gda_transaction_new (depth.ToString ());
			
			switch (isolevel) {
			case IsolationLevel.ReadCommitted :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.ReadCommitted);
				break;
			case IsolationLevel.ReadUncommitted :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.ReadUncommitted);
				break;
			case IsolationLevel.RepeatableRead :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.RepeatableRead);
				break;
			case IsolationLevel.Serializable :
				libgda.gda_transaction_set_isolation_level (gdaTransaction,
									    GdaTransactionIsolation.Serializable);
				break;
			}
			
			libgda.gda_connection_begin_transaction (connection.GdaConnection, gdaTransaction);
		}

		internal OleDbTransaction (OleDbConnection connection, IsolationLevel isolevel) 
			: this (connection, 1, isolevel)
		{
		}


		#endregion // Constructors

		#region Properties

		public OleDbConnection Connection {
			get {
				return connection;
			}
		}

		IDbConnection IDbTransaction.Connection {
			get {
				return connection;
			}
		}
		
		public IsolationLevel IsolationLevel {
			get {
				switch (libgda.gda_transaction_get_isolation_level (gdaTransaction)) {
				case GdaTransactionIsolation.ReadCommitted :
					return IsolationLevel.ReadCommitted;
				case GdaTransactionIsolation.ReadUncommitted :
					return IsolationLevel.ReadUncommitted;
				case GdaTransactionIsolation.RepeatableRead :
					return IsolationLevel.RepeatableRead;
				case GdaTransactionIsolation.Serializable :
					return IsolationLevel.Serializable;
				}

				return IsolationLevel.Unspecified;
			}
		}

		#endregion // Properties

		#region Methods

		public OleDbTransaction Begin () 
		{
			return new OleDbTransaction (connection, depth + 1);
		}

		public OleDbTransaction Begin (IsolationLevel isolevel) 
		{
			return new OleDbTransaction (connection, depth + 1, isolevel);
		}

		public void Commit ()
		{
			if (!libgda.gda_connection_commit_transaction (connection.GdaConnection,
								       gdaTransaction))
				throw new InvalidOperationException ();
		}

		[MonoTODO]
		~OleDbTransaction ()
		{
			libgda.FreeObject (gdaTransaction);
			gdaTransaction = IntPtr.Zero;
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Rollback ()
	        {
			if (!libgda.gda_connection_rollback_transaction (connection.GdaConnection,
									 gdaTransaction))
				throw new InvalidOperationException ();
		}

		#endregion // Methods
	}
}
