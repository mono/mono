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

		OleDbConnection connection = null;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

		#endregion // Fields

		#region Constructors
		
		internal OleDbTransaction (OleDbConnection connection)
		{
			this.connection = connection;
			libgda.gda_connection_begin_transaction (connection.GdaConnection, IntPtr.Zero);
		}

		internal OleDbTransaction (OleDbConnection connection, IsolationLevel isolevel) 
			: this (connection)
		{
			isolationLevel = isolevel;
		}

		#endregion // Constructors

		#region Properties

		public OleDbConnection Connection {
			get { return connection; }
		}

		public IsolationLevel IsolationLevel
		{
			get { return isolationLevel; }
		}

		IDbConnection IDbTransaction.Connection {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public OleDbTransaction Begin ()
		{
			return new OleDbTransaction (connection);
		}

		public OleDbTransaction Begin (IsolationLevel isolevel)
		{
			return new OleDbTransaction (connection, isolevel);
		}

		public void Commit ()
		{
			if (!libgda.gda_connection_commit_transaction (connection.GdaConnection, IntPtr.Zero))
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
			if (!libgda.gda_connection_rollback_transaction (connection.GdaConnection, IntPtr.Zero))
				throw new InvalidOperationException ();
		}

		#endregion // Methods
	}
}
