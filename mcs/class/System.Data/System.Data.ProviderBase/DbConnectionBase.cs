//
// System.Data.ProviderBase.DbConnectionBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.Common;
using System.EnterpriseServices;

namespace System.Data.ProviderBase {
	public abstract class DbConnectionBase : DbConnection
	{
		#region Fields
		
		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected DbConnectionBase (DbConnectionBase connection)
		{
		}

		[MonoTODO]
		protected DbConnectionBase (DbConnectionFactory connectionFactory)
		{
		}
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected int CloseCount {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal DbConnectionFactory ConnectionFactory {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal DbConnectionString ConnectionOptions {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ConnectionString {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override int ConnectionTimeout {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual int ConnectionTimeoutInternal {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string Database {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string DataSource {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal DbConnectionInternal InnerConnection {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string ServerVersion {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override ConnectionState State {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Events

		public event StateChangeEventHandler StateChange;

		#endregion // Events

		#region Methods

		[MonoTODO]
		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void ChangeDatabase (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbCommand CreateDbCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbMetaDataFactory GetMetaDataFactory (DbConnectionInternal internalConnection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void OnStateChange (ConnectionState originalState, ConnectionState currentState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Open ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
