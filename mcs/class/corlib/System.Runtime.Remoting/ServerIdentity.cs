//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting
{
	internal abstract class ServerIdentity : Identity
	{
		protected Type _objectType;

		protected MarshalByRefObject _serverObject;

		// Message sink used in the server to dispatch a message
		// to the server object
		protected IMessageSink _serverSink = null;

		protected Context _context;
		protected Lease _lease;

		public ServerIdentity (string objectUri, Context context, Type objectType): base (objectUri)
		{
			_objectType = objectType;
			_context = context;
		}

		public Type ObjectType
		{
			get { return _objectType; }
		}

		public void StartTrackingLifetime (ILease lease)
		{
			// Adds this identity to the LeaseManager. 
			// _serverObject must be set.

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

			if (_envoySink != null && !(_envoySink is EnvoyTerminatorSink))
				_objRef.EnvoyInfo = new EnvoyInfo (_envoySink);

			return _objRef;
		}

		public void AttachServerObject (MarshalByRefObject serverObject, Context context)
		{
			_context = context;
			_serverObject = serverObject;
			
			if (RemotingServices.IsTransparentProxy (serverObject))
			{
				RealProxy rp = RemotingServices.GetRealProxy (serverObject);
				rp.ObjectIdentity = this;
			}
			else
			{
				if (_objectType.IsContextful)
					_envoySink = context.CreateEnvoySink (serverObject);
	
				_serverObject.ObjectIdentity = this;
			}
		}

		public Lease Lease
		{
			get { return _lease; }
		}

		public Context Context
		{
			get { return _context; }
			set { _context = value; }
		}

		public abstract IMessage SyncObjectProcessMessage (IMessage msg);
		public abstract IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink);

		protected void DisposeServerObject()
		{
			// Detach identity from server object to avoid problems if the
			// object is marshalled again.
			
			if (_serverObject != null) {
				_serverObject.ObjectIdentity = null;
				_serverObject = null;
			}
		}
	}

	internal class ClientActivatedIdentity : ServerIdentity
	{
		public ClientActivatedIdentity (string objectUri, Type objectType): base (objectUri, null, objectType)
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
				if (_serverObject == null) {
					MarshalByRefObject server = (MarshalByRefObject) Activator.CreateInstance (_objectType, true);
					AttachServerObject (server, Context.DefaultContext);
					StartTrackingLifetime ((ILease)server.InitializeLifetimeService ());
				}
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

			MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance (_objectType, true);
			obj.ObjectIdentity = this;
			IMessageSink serverSink = _context.CreateServerObjectSinkChain(obj);
			IMessage result = serverSink.SyncProcessMessage (msg);
			if (obj is IDisposable) ((IDisposable)obj).Dispose();
			return result;
		}

		public override IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink)
		{
			MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance (_objectType, true);
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
