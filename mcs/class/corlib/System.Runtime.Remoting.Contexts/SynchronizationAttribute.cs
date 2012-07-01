//
// System.Runtime.Remoting.Contexts.SynchronizationAttribute.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Contexts
{
	[AttributeUsage(AttributeTargets.Class)]
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public class SynchronizationAttribute: ContextAttribute, IContributeClientContextSink, IContributeServerContextSink
	{
		public const int NOT_SUPPORTED = 1;
		public const int SUPPORTED = 2;
		public const int REQUIRED = 4;
		public const int REQUIRES_NEW = 8;
		
		bool _bReEntrant;
		int _flavor;

		[NonSerialized]
		bool _locked;
		[NonSerialized]
		int _lockCount;
		
		[NonSerialized]
		Mutex _mutex = new Mutex (false);
		[NonSerialized]
		Thread _ownerThread;
		
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
				
			_bReEntrant = reEntrant;
			_flavor = flag;
		}
		
		public virtual bool IsReEntrant
		{
			get { return _bReEntrant; }
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
						_lockCount++;
						if (_lockCount > 1)
							ReleaseLock (); // Thread already had the lock
							
						_ownerThread = Thread.CurrentThread;
					}
				}
				else
				{
					lock (this)
					{
						while (_lockCount > 0 && _ownerThread == Thread.CurrentThread)
						{
							_lockCount--;
							_mutex.ReleaseMutex ();
							_ownerThread = null;
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
				_ownerThread = Thread.CurrentThread;
				_lockCount++;
			}
		}
		
		internal void ReleaseLock ()
		{
			lock (this)
			{
				if (_lockCount > 0 && _ownerThread == Thread.CurrentThread) {
					_lockCount--;
					_mutex.ReleaseMutex ();
					_ownerThread = null;
				}
			}
		}
		
		[System.Runtime.InteropServices.ComVisible (true)]
		public override void GetPropertiesForNewContext (IConstructionCallMessage ctorMsg)
		{
			if (_flavor != NOT_SUPPORTED) {
				ctorMsg.ContextProperties.Add (this);
			}
		}
		
		public virtual IMessageSink GetClientContextSink (IMessageSink nextSink)
		{
			return new SynchronizedClientContextSink (nextSink, this);
		}
		
		public virtual IMessageSink GetServerContextSink (IMessageSink nextSink)
		{
			return new SynchronizedServerContextSink (nextSink, this);
		}
		
		[System.Runtime.InteropServices.ComVisible (true)]
		public override bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			SynchronizationAttribute prop = ctx.GetProperty ("Synchronization") as SynchronizationAttribute;
			switch (_flavor)
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
			if (Thread.CurrentContext.IsDefaultContext) return;
			SynchronizationAttribute prop = Thread.CurrentContext.GetProperty ("Synchronization") as SynchronizationAttribute;
			if (prop == null) return;
			prop.Locked = false;
		}
		
		internal static void EnterContext ()
		{
			if (Thread.CurrentContext.IsDefaultContext) return;
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

