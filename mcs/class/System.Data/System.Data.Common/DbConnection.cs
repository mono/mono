//
// System.Data.Common.DbConnection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.ComponentModel;
using System.Data;
using System.EnterpriseServices;

namespace System.Data.Common {
	public abstract class DbConnection : Component, IDbConnection, IDisposable
	{
		#region Constructors

		protected DbConnection ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract string ConnectionString { get; set; }
		public abstract int ConnectionTimeout { get; }
		public abstract string Database { get; }
		public abstract string DataSource { get; }
		public abstract string ServerVersion { get; }
		public abstract ConnectionState State { get; }

		#endregion // Properties

		#region Methods

		protected abstract DbTransaction BeginDbTransaction (IsolationLevel isolationLevel);

		[MonoTODO]
		public DbTransaction BeginTransaction ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbTransaction BeginTransaction (IsolationLevel isolationLevel)
		{
			throw new NotImplementedException ();
		}

		public abstract void ChangeDatabase (string databaseName);
		public abstract void Close ();

		public DbCommand CreateCommand ()
		{
			return CreateDbCommand ();
		}

		protected abstract DbCommand CreateDbCommand ();

		[MonoTODO]
		public virtual void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema (string collectionName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DataTable GetSchema (string collectionName, string[] restrictionValues)
		{
			throw new NotImplementedException ();
		}

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}
		
		public abstract void Open ();

		#endregion // Methods

	}
}

#endif
