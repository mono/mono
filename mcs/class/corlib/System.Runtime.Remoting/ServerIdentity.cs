//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting
{
	internal abstract class ServerIdentity : Identity
	{
		protected MarshalByRefObject _serverObject;

		// Message sink used in the server to dispatch a message
		// to the server object
		protected IMessageSink _serverSink = null;

		protected Context _context;
		protected Lease _lease;

		public ServerIdentity(string objectUri, Context context, Type objectType): base (objectUri, objectType)
		{
			_context = context;
		}

		public void StartTrackingLifetime ()
		{
			// Adds this identity to the LeaseManager. 
			// _serverObject must be set.

			ILease lease = (ILease) _serverObject.InitializeLifetimeService ();
			if (lease != null && lease.CurrentState == LeaseState.Null) lease = null;

			if (lease != null) 
			{
				if (! (lease is Lease)) lease = new Lease();  // This seems to be MS behavior
				_lease = (Lease) lease;
				LifetimeServices.TrackLifetime (this);
			}
		}

		public virtual void OnLifetimeExpired()
		{
			DisposeServerObject();
		}

		public override ObjRef CreateObjRef (Type requestedType)
		{
			if (_objRef != null)
			{
				// Just update channel info. It may have changed.
				_objRef.UpdateChannelInfo();
				return _objRef;
			}

			if (requestedType == null) requestedType = _objectType;
			_objRef = new ObjRef ();
			_objRef.TypeInfo = new TypeInfo(requestedType);
			_objRef.URI = _objectUri;
			return _objRef;
		}

		public void AttachServerObject (MarshalByRefObject serverObject)
		{
			_serverObject = serverObject;
			_serverObject.ObjectIdentity = this;
			StartTrackingLifetime ();
		}

		public Lease Lease
		{
			get { return _lease; }
		}

		public Context Context
		{
			get { return _context; }
		}

		public abstract IMessage SyncObjectProcessMessage (IMessage msg);
		public abstract IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink);

		protected void DisposeServerObject()
		{
			if (_serverObject != null)
			{
				IDisposable disp = _serverObject as IDisposable;
				if (disp != null) disp.Dispose ();
			}
			_serverObject = null;
		}
	}

	internal class ClientActivatedIdentity : ServerIdentity
	{
		public ClientActivatedIdentity (string objectUri, Context context, Type objectType): base (objectUri, context, objectType)
		{
		}
	
		public MarshalByRefObject GetServerObject ()
		{
			return _serverObject;
		}

		public override void OnLifetimeExpired()
		{
			base.OnLifetimeExpired();
			RemotingServices.DisposeIdentity (this);
		}

		public override IMessage SyncObjectProcessMessage (IMessage msg)
		{
			if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (_serverObject);
			return _serverSink.SyncProcessMessage (msg);
		}

		public override IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink)
		{
			if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (_serverObject);
			return _serverSink.AsyncProcessMessage (msg, replySink);
		}	
	}

	internal class SingletonIdentity : ServerIdentity
	{
		public SingletonIdentity (string objectUri, Context context, Type objectType): base (objectUri, context, objectType)
		{
		}
	
		public MarshalByRefObject GetServerObject ()
		{
			if (_serverObject != null) return _serverObject;

			lock (this) 
			{
				if (_serverObject == null)
					AttachServerObject ((MarshalByRefObject) Activator.CreateInstance (_objectType));
			}
			return _serverObject;
		}

		public override IMessage SyncObjectProcessMessage (IMessage msg)
		{
			MarshalByRefObject obj = GetServerObject ();
			if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (obj);
			return _serverSink.SyncProcessMessage (msg);
		}

		public override IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink)
		{
			MarshalByRefObject obj = GetServerObject ();
			if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (obj);
			return _serverSink.AsyncProcessMessage (msg, replySink);
		}	
	}

	internal class SingleCallIdentity : ServerIdentity
	{
		public SingleCallIdentity (string objectUri, Context context, Type objectType): base (objectUri, context, objectType)
		{
		}

		public override IMessage SyncObjectProcessMessage (IMessage msg)
		{
			// SingleCallIdentity creates and disposes an instance in each call

			MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance (_objectType);
			obj.ObjectIdentity = this;
			IMessageSink serverSink = _context.CreateServerObjectSinkChain(obj);
			IMessage result = serverSink.SyncProcessMessage (msg);
			if (obj is IDisposable) ((IDisposable)obj).Dispose();
			return result;
		}

		public override IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink)
		{
			MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance (_objectType);
			IMessageSink serverSink = _context.CreateServerObjectSinkChain(obj);
			if (obj is IDisposable) replySink = new DisposerReplySink(replySink, ((IDisposable)obj));
			return serverSink.AsyncProcessMessage (msg, replySink);
		}	
	}

	internal class DisposerReplySink : IMessageSink
	{
		IMessageSink _next;
		IDisposable _disposable;

		public DisposerReplySink (IMessageSink next, IDisposable disposable)
		{
			_next = next;
			_disposable = disposable;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			_disposable.Dispose();
			return _next.SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			throw new NotSupportedException();
		}

		public IMessageSink NextSink
		{
			get { return _next; }
		}
	}
}
