//
// System.Runtime.Remoting.Contexts.SynchronizationAttribute.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
//

using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Contexts
{
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	public class SynchronizationAttribute: ContextAttribute, IContributeClientContextSink, IContributeServerContextSink
	{
		public const int NOT_SUPPORTED = 1;
		public const int SUPPORTED = 2;
		public const int REQUIRED = 4;
		public const int REQUIRES_NEW = 8;
		
		bool _isReentrant;
		bool _locked;
		int _flag;
		int _lockCount = 0;
		
		Mutex _mutex = new Mutex ();
		
		public SynchronizationAttribute ()
		: this (REQUIRES_NEW, false)
		{
		}
		
		public SynchronizationAttribute (bool reEntrant)
		: this (REQUIRES_NEW, reEntrant)
		{
		}
		
		public SynchronizationAttribute (int flag)
		: this (flag, false)
		{
		}
		
		public SynchronizationAttribute (int flag, bool reEntrant)
		: base ("Synchronization")
		{
			if (flag != NOT_SUPPORTED && flag != REQUIRED && flag != REQUIRES_NEW && flag != SUPPORTED)
				throw new ArgumentException ("flag");
				
			_isReentrant = reEntrant;
			_flag = flag;
		}
		
		public virtual bool IsReEntrant
		{
			get { return _isReentrant; }
		}
		
		public virtual bool Locked
		{
			get 
			{ 
				return _locked; 
			}
			
			set 
			{
				if (value)
				{
					_mutex.WaitOne ();
					lock (this)
					{
						if (_lockCount > 0)
							ReleaseLock (); // Thread already had the lock
						else
							_lockCount++;
					}
				}
				else
				{
					lock (this)
					{
						while (_lockCount > 0)
						{
							_lockCount--;
							_mutex.ReleaseMutex ();
						}
					}
				}
			}
		}
		
		internal void AcquireLock ()
		{
			_mutex.WaitOne ();
			
			lock (this)
			{
				_lockCount++;
			}
		}
		
		internal void ReleaseLock ()
		{
			lock (this)
			{
				if (_lockCount > 0) {
					_lockCount--;
					_mutex.ReleaseMutex ();
				}
			}
		}
		
		public override void GetPropertiesForNewContext (IConstructionCallMessage ctorMsg)
		{
			if (_flag != NOT_SUPPORTED)
				ctorMsg.ContextProperties.Add (this);
		}
		
		public virtual IMessageSink GetClientContextSink (IMessageSink nextSink)
		{
			return new SynchronizedClientContextSink (nextSink, this);
		}
		
		public virtual IMessageSink GetServerContextSink (IMessageSink nextSink)
		{
			return new SynchronizedServerContextSink (nextSink, this);
		}
		
		public override bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			SynchronizationAttribute prop = ctx.GetProperty ("Synchronization") as SynchronizationAttribute;
			switch (_flag)
			{
				case NOT_SUPPORTED: return (prop == null);
				case REQUIRED: return (prop != null);
				case REQUIRES_NEW: return false;
				case SUPPORTED: return true;
			}
			return false;
		}
		
		internal static void ExitContext ()
		{
			if (Thread.CurrentContext == Context.DefaultContext) return;
			SynchronizationAttribute prop = Thread.CurrentContext.GetProperty ("Synchronization") as SynchronizationAttribute;
			if (prop == null) return;
			prop.Locked = false;
		}
		
		internal static void EnterContext ()
		{
			if (Thread.CurrentContext == Context.DefaultContext) return;
			SynchronizationAttribute prop = Thread.CurrentContext.GetProperty ("Synchronization") as SynchronizationAttribute;
			if (prop == null) return;
			prop.Locked = true;
		}
	}
	
	internal class SynchronizedClientContextSink: IMessageSink
	{
		IMessageSink _next;
		SynchronizationAttribute _att;
		
		public SynchronizedClientContextSink (IMessageSink next, SynchronizationAttribute att)
		{
			_att = att;
			_next = next;
		}
		
		public IMessageSink NextSink 
		{
			get { return _next; }
		}
		
		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			if (_att.IsReEntrant)
			{
				_att.ReleaseLock();	// Unlock when leaving the context
				replySink = new SynchronizedContextReplySink (replySink, _att, true);
			}
			return _next.AsyncProcessMessage (msg, replySink);
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			if (_att.IsReEntrant) 
				_att.ReleaseLock ();	// Unlock when leaving the context
			
			try
			{
				return _next.SyncProcessMessage (msg);
			}
			finally
			{
				if (_att.IsReEntrant)
					_att.AcquireLock ();
			}
		}
	}
	
	internal class SynchronizedServerContextSink: IMessageSink
	{
		IMessageSink _next;
		SynchronizationAttribute _att;
		
		public SynchronizedServerContextSink (IMessageSink next, SynchronizationAttribute att)
		{
			_att = att;
			_next = next;
		}
		
		public IMessageSink NextSink 
		{
			get { return _next; }
		}
		
		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			_att.AcquireLock ();
			replySink = new SynchronizedContextReplySink (replySink, _att, false);
			return _next.AsyncProcessMessage (msg, replySink);
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			_att.AcquireLock ();
			try
			{
				return _next.SyncProcessMessage (msg);
			}
			finally
			{
				_att.ReleaseLock ();
			}
		}
	}
	
	internal class SynchronizedContextReplySink: IMessageSink
	{
		IMessageSink _next;
		bool _newLock;
		SynchronizationAttribute _att;
	
		public SynchronizedContextReplySink (IMessageSink next, SynchronizationAttribute att, bool newLock)
		{
			_newLock = newLock;
			_next = next;
			_att = att;
		}
		
		public IMessageSink NextSink 
		{
			get { return _next; }
		}
		
		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			// Never called
			throw new NotSupportedException ();
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			if (_newLock) _att.AcquireLock ();
			else _att.ReleaseLock ();

			try
			{
				return _next.SyncProcessMessage (msg);
			}
			finally
			{
				if (_newLock)
					_att.ReleaseLock ();
			}
		}
	}
}

