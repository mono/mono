//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2002, Lluis Sanchez Gual
//

using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting
{
	enum ServiceType { ClientActivated, SingleCall, Singleton, ClientProxy };

	internal class Identity
	{
		// An Identity object holds remoting information about
		// an object. It can be used to store client side information
		// (information about how to reach the remote server),
		// and also to store server side information (information
		// about how to dispatch messages to the object in the server).

		// The object that this identity represents. Can be a MarshalByRefObject
		// (if it is a server object) or a transparent proxy (if it is a client
		// proxy to a remote object).
		MarshalByRefObject _realObject;

		Type _objectType;

		// URI of the object
		string _objectUri;

		// Message sink to use to send a message to the remote server
		IMessageSink _clientSink = null;

		// Message sink used in the server to dispatch a message
		// to the server object
		IMessageSink _serverSink = null;
		Context _context;

		ObjRef _objRef = null;

		ServiceType _serviceType;

		public Identity(string objectUri, Context context, Type objectType, ServiceType serviceType)
		{
			_objectUri = objectUri;
			_context = context;
			_objectType = objectType;
			_serviceType = serviceType;
		}

		public ObjRef CreateObjRef (Type requestedType)
		{
			if (requestedType == null) requestedType = _objectType;

			ObjRef res = new ObjRef ();
			res.TypeInfo = new TypeInfo(requestedType);
			res.URI = _objectUri;
			_objRef = res;
			return res;
		}

		public ServiceType IdentityServiceType
		{
			get { return _serviceType; }
		}

		public bool IsFromThisAppDomain
		{
			get
			{
				// fixme: what if it is contextbound?
				return (_clientSink == null);
			}
		}

		public MarshalByRefObject RealObject
		{
			get	
			{ 
				if (_realObject != null) return _realObject;

				if (_serviceType == ServiceType.Singleton) {
					lock (this) 
					{
						if (_realObject == null) {
							_realObject = (MarshalByRefObject) Activator.CreateInstance (_objectType);
							LifetimeServices.TrackLifetime (this);
						}
						return _realObject;
					}
				}
				else if (_serviceType == ServiceType.SingleCall) {
					return (MarshalByRefObject) Activator.CreateInstance (_objectType);
				}
				return null;
			}
			set 
			{ 
				_realObject = value; 
			}
		}

		public string ObjectUri
		{
			get { return _objectUri; }
		}

		public IMessageSink ClientSink
		{
			get { return _clientSink; }
			set { _clientSink = value; }
		}

		public Type ObjectType
		{
			get { return _objectType; }
		}

		public Context Context
		{
			get { return _context; }
		}


		public IMessage SyncObjectProcessMessage (IMessage msg)
		{
			if (_serviceType == ServiceType.SingleCall)
			{
				MarshalByRefObject obj = (MarshalByRefObject)RealObject;
				IMessageSink serverSink = _context.CreateServerObjectSinkChain(obj);
				IMessage result = serverSink.SyncProcessMessage (msg);
				if (obj is IDisposable) ((IDisposable)obj).Dispose();
				return result;
			}
			else
			{
				if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (RealObject);
				return _serverSink.SyncProcessMessage (msg);
			}
		}

		public IMessageCtrl AsyncObjectProcessMessage (IMessage msg, IMessageSink replySink)
		{
			if (_serviceType == ServiceType.SingleCall)
			{
				MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance (_objectType);
				IMessageSink serverSink = _context.CreateServerObjectSinkChain(obj);
				if (obj is IDisposable) replySink = new DisposerReplySink(replySink, ((IDisposable)obj));
				return serverSink.AsyncProcessMessage (msg, replySink);
			}
			else
			{
				if (_serverSink == null) _serverSink = _context.CreateServerObjectSinkChain (RealObject);
				return _serverSink.AsyncProcessMessage (msg, replySink);
			}
		}
	}

	public class DisposerReplySink : IMessageSink
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
