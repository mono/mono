//
// System.Data.ProviderBase.DbConnectionFactory
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbConnectionFactory 
	{
		#region Fields
		
		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected DbConnectionFactory ()
		{
		}

		[MonoTODO]
		protected DbConnectionFactory (DbConnectionPoolCounters performanceCounters)
		{
		}
		
		#endregion // Constructors

		#region Properties

		public abstract DbProviderFactory ProviderFactory { get; }

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected virtual IAsyncResult BeginCreateConnection (DbConnectionBase owningObject, DbConnectionString connectionOptions, DbConnectionInternal connection, AsyncCallback callback, object asyncStateObject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ClearAllPools ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ClearPool (DbConnectionBase connection)
		{
			throw new NotImplementedException ();
		}

		protected abstract DbConnectionInternal CreateConnection (DbConnectionString options, DbConnectionBase owningObject);
		protected abstract DbConnectionString CreateConnectionOptions (string connectionString);
		protected abstract DbConnectionPoolOptions CreateConnectionPoolOptions (DbConnectionString options);

		[MonoTODO]
		protected virtual DbMetaDataFactory CreateMetaDataFactory (DbConnectionInternal internalConnection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbConnectionInternal EndCreateConnection (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal DbMetaDataFactory GetMetaDataFactory (DbConnectionString connectionOptions, DbConnectionInternal internalConnection)
		{
			throw new NotImplementedException ();
		}

		internal DbConnectionString CreateConnectionOptionsInternal (string connectionString)
		{
			return CreateConnectionOptions (connectionString);
		}

		[MonoTODO]
		public void SetConnectionPoolOptions (string connectionString, DbConnectionPoolOptions poolOptions)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
