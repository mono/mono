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
		IsolationLevel isolationLevel;
		IntPtr gdaTransaction;
		int depth;

		#endregion // Fields

		#region Constructors

		internal OleDbTransaction (OleDbConnection connection, int depth)
		{
			this.connection = connection;
			isolationLevel = IsolationLevel.ReadCommitted;

			gdaTransaction = libgda.gda_transaction_new (depth.ToString ());
			libgda.gda_connection_begin_transaction (connection.GdaConnection, gdaTransaction);
		}

		internal OleDbTransaction (OleDbConnection connection)
			: this (connection, 1)
		{
		}

		internal OleDbTransaction (OleDbConnection connection, int depth, IsolationLevel isolevel) 
			: this (connection, depth)
		{
			isolationLevel = isolevel;
		}

		internal OleDbTransaction (OleDbConnection connection, IsolationLevel isolevel) 
			: this (connection, 1, isolevel)
		{
		}


		#endregion // Constructors

		#region Properties

		public OleDbConnection Connection {
			get { return connection; }
		}

		IDbConnection IDbTransaction.Connection {
			get { return connection; }
		}
		
		public IsolationLevel IsolationLevel {
			get { return isolationLevel; }
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
			if (!libgda.gda_connection_commit_transaction (connection.GdaConnection, gdaTransaction))
				throw new InvalidOperationException ();
		}

		[MonoTODO]
		~OleDbTransaction ()
		{
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Rollback ()
	        {
			if (!libgda.gda_connection_rollback_transaction (connection.GdaConnection, gdaTransaction))
				throw new InvalidOperationException ();
		}

		#endregion // Methods
	}
}
