using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstancePersistenceContext
	{
		internal InstancePersistenceContext (InstanceHandle handle, InstanceView view)
		{
			if (handle == null)
				throw new ArgumentNullExcepion ("handle");
			if (view == null)
				throw new ArgumentNullExcepion ("view");

			InstanceHandle = handle;
			InstanceView = view;
		}
		
		public InstanceHandle InstanceHandle { get; private set; }
		public long InstanceVersion { get { throw new NotImplementedException (); } }
		public InstanceView InstanceView { get; private set; }
		public Guid LockToken { get { throw new NotImplementedException (); } }
		public object UserContext { get { throw new NotImplementedException (); } }

		public void AssociatedInstanceKey (Guid key)
		{
			throw new NotImplementedException ();
		}

		public void BindAcquiredLock (long instanceVersion)
		{
			throw new NotImplementedException ();
		}
		public void BindEvent (InstancePersistenceEvent persistenceEvent)
		{
			throw new NotImplementedException ();
		}
		public void BindInstance (Guid instanceId)
		{
			throw new NotImplementedException ();
		}
		public void BindInstanceOwner (Guid instanceOwnerId, Guid lockToken)
		{
			throw new NotImplementedException ();
		}
		public void BindReclaimedLock (long instanceVersion, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void CompletedInstance ()
		{
			throw new NotImplementedException ();
		}
		public void CompletedInstanceKey (Guid key)
		{
			throw new NotImplementedException ();
		}
		public Exception CreateBindReclaimedLockException (long instanceVersion)
		{
			throw new NotImplementedException ();
		}

		public void Execute (InstancePersistenceCommand command, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		public void LoadedInstance (InstanceState state, IDictionary<XName, InstanceValue> instanceData, IDictionary<XName, InstanceValue> instanceMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> associatedInstanceKeyMetadata, IDictionary<Guid, IDictionary<XName, InstanceValue>> completedInstanceKeyMetadata)
		{
			throw new NotImplementedException ();
		}
		public void PersistedInstance (IDictionary<XName, InstanceValue> data)
		{
			throw new NotImplementedException ();
		}
		public void QueriedInstanceStore (InstanceStoreQueryResult queryResult)
		{
			throw new NotImplementedException ();
		}
		public void ReadInstanceKeyMetadata (Guid key, IDictionary<XName, InstanceValue> metadata, bool complete)
		{
			throw new NotImplementedException ();
		}
		public void SetCancellationHandler (Action<InstancePersistenceContext> cancellationHandler)
		{
			throw new NotImplementedException ();
		}
		public void UnassociatedInstanceKey (Guid key)
		{
			throw new NotImplementedException ();
		}
		public void WroteInstanceKeyMetadataValue (Guid key, XName name, InstanceValue value)
		{
			throw new NotImplementedException ();
		}
		public void WroteInstanceMetadataValue (XName name, InstanceValue value)
		{
			throw new NotImplementedException ();
		}
		public void WroteInstanceOwnerMetadataValue (XName name, InstanceValue value)
		{
			throw new NotImplementedException ();
		}

		// async operations

		Action<long, TimeSpan> bind_reclaimed_lock_delegate;
		
		public IAsyncResult BeginBindReclaimedLock (long instanceVersion, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (bind_reclaimed_lock_delegate == null)
				bind_reclaimed_lock_delegate = new Action<long, TimeSpan> (BindReclaimedLock);
			return bind_reclaimed_lock_delegate.BeginInvoke (instanceVersion, timeout, callback, state);
		}

		public void EndBindReclaimedLock (IAsyncResult result)
		{
			if (bind_reclaimed_lock_delegate == null)
				throw new InvalidOperationException ("Async BindReclaimedLock operation has not started");
			bind_reclaimed_lock_delegate.EndInvoke (result);
		}

		Action<InstancePersistenceCommand, TimeSpan> execute_delegate;
		
		public IAsyncResult BeginExecute (InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (execute_delegate == null)
				execute_delegate = new Action<InstancePersistenceCommand, TimeSpan> (Execute);
			return execute_delegate.BeginInvoke (command, timeout, callback, state);
		}
		
		public void EndExecute (IAsyncResult result)
		{
			if (execute_delegate == null)
				throw new InvalidOperationException ("Async Execute operation has not started");
			execute_delegate.EndInvoke (result);
		}
	}
}
