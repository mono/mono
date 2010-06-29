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

		Func<InstanceHandle, InstancePersistenceCommand, TimeSpan, InstanceView> execute_delegate;

		public IAsyncResult BeginExecute (InstanceHandle handle, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (execute_delegate == null)
				execute_delegate = new Func<InstanceHandle, InstancePersistenceCommand, TimeSpan, InstanceView> (Execute);
			return execute_delegate.BeginInvoke (handle, command, timeout, callback, state);
		}

		Func<InstancePersistenceContext, InstancePersistenceCommand, TimeSpan, bool> try_command_delegate;

		protected internal virtual IAsyncResult BeginTryCommand (InstancePersistenceContext context, InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (try_command_delegate == null)
				try_command_delegate = new Func<InstancePersistenceContext, InstancePersistenceCommand, TimeSpan, bool> (TryCommand);
			return try_command_delegate.BeginInvoke (context, command, timeout, callback, state);
		}

		Func<InstanceHandle, TimeSpan, List<InstancePersistenceEvent>> wait_for_events_delegate;

		public IAsyncResult BeginWaitForEvents (InstanceHandle handle, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (wait_for_events_delegate == null)
				wait_for_events_delegate = new Func<InstanceHandle, TimeSpan, List<InstancePersistenceEvent>> (WaitForEvents);
			return wait_for_events_delegate.BeginInvoke (handle, timeout, callback, state);
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
			if (execute_delegate == null)
				throw new InvalidOperationException ("Async operation has not started");
			return execute_delegate.EndInvoke (result);
		}

		public bool EndTryCommand (IAsyncResult result)
		{
			if (try_command_delegate == null)
				throw new InvalidOperationException ("Async operation has not started");
			return try_command_delegate.EndInvoke (result);
		}

		public List<InstancePersistenceEvent> EndWaitForEvents (IAsyncResult result)
		{
			if (wait_for_events_delegate == null)
				throw new InvalidOperationException ("Async operation has not started");
			return wait_for_events_delegate.EndInvoke (result);
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
