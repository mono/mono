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
	public abstract class InstanceStore
	{
		public InstanceOwner DefaultInstanceOwner { get; set; }
		
		public IAsyncResult BeginExecute (InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual IAsyncResult BeginTryCommand (InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginWaitForEvents (InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public InstanceHandle CreateInstanceHandle ()
		{
			throw new NotImplementedException ();
		}

		public InstanceHandle CreateInstanceHandle (Guid instanceId)
		{
			throw new NotImplementedException ();
		}

		public InstanceHandle CreateInstanceHandle (InstanceOwner owner)
		{
			throw new NotImplementedException ();
		}

		public InstanceHandle CreateInstanceHandle (InstanceOwner owner, Guid instanceId)
		{
			throw new NotImplementedException ();
		}

		public InstanceView EndExecute (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public List<InstancePersistenceEvent> EndWaitForEvents (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public InstanceView Execute (InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected InstancePersistenceEvent [] GetEvents (InstanceOwner owner)
		{
			throw new NotImplementedException ();
		}

		protected InstanceOwner [] GetInstanceOwners ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnFreeInstanceHandle (InstanceHandle instanceHandle, object userContext)
		{
			throw new NotImplementedException ();
		}

		protected virtual object OnNewInstanceHandle (InstanceHandle instanceHandle)
		{
			throw new NotImplementedException ();
		}

		protected void ResetEvent (InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
		{
			throw new NotImplementedException ();
		}

		protected void SignalEvent (InstancePersistenceEvent persistenceEvent, InstanceOwner owner)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual bool TryCommand (InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public List<InstancePersistenceEvent> WaitForEvents (InstanceHandle handle, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
