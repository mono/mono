//
// System.Data.Common.DbTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Common {
	public abstract class DbTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Constructors

		[MonoTODO]
		protected DbTransaction ()
		{
		}

		#endregion // Constructors

		#region Properties

		public DbConnection Connection {
			get { return DbConnection; }
		}

		protected abstract DbConnection DbConnection { get; }

		IDbConnection IDbTransaction.Connection {
			get { return (IDbConnection) Connection; }
		}

		public abstract IsolationLevel IsolationLevel { get; }

		#endregion // Properties

		#region Methods

		public abstract void Commit ();
		public abstract void Dispose ();
		public abstract void Rollback ();

		#endregion // Methods
	}
}

#endif // NET_1_2
