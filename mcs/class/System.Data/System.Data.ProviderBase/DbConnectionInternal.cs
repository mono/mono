//
// System.Data.ProviderBase.DbConnectionInternal
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
	public abstract class DbConnectionInternal 
	{
		#region Fields
		
		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected DbConnectionInternal ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		[MonoTODO]
		protected bool IsEnlistedInDistributedTransaction {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal object Owner {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected internal DbReferenceCollection ReferenceCollection {
			get { throw new NotImplementedException (); }
		}

		public abstract string ServerVersion { get; }

		[MonoTODO]
		public virtual string ServerVersionNormalized {
			get { throw new NotImplementedException (); }
		}

		public abstract ConnectionState State { get; }

		#endregion // Properties

		#region Methods

		protected abstract void Activate (bool isInTransaction);

		[MonoTODO]
		protected virtual IAsyncResult BeginOpen (DbConnectionBase outerConnection, AsyncCallback callback, object asyncStateObject)
		{
			throw new NotImplementedException ();
		}

		public abstract DbTransaction BeginTransaction (IsolationLevel il);

		[MonoTODO]
		public virtual void ChangeDatabase (string value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Close ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DbReferenceCollection CreateReferenceCollection ()
		{
			throw new NotImplementedException ();
		}

		protected abstract void Deactivate ();

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void DoomThisConnection ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndOpen (DbConnectionBase outerConnection, IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		public abstract void EnlistDistributedTransaction (ITransaction transaction);

		[MonoTODO]
		protected virtual DataTable GetSchemaInternal (DbConnectionBase outerConnection, string collectionName, string[] restrictions)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Open (DbConnectionBase outerConnection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void PurgeWeakReferences ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal void SetDistributedTransaction (ITransaction transaction, bool manualEnlistment)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
