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

namespace System.Runtime.Remoting
{
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
		object _realObject;

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

		public Identity(string objectUri, Context context, Type objectType)
		{
			_objectUri = objectUri;
			_context = context;
			_objectType = objectType;
		}

		public ObjRef CreateObjRef (Type requestedType)
		{
			// fixme: handle requested_type		
			if (requestedType == null) requestedType = _objectType;
			ObjRef res = new ObjRef ((MarshalByRefObject)_realObject, requestedType);
			res.URI = _objectUri;
			_objRef = res;
			return res;
		}

		public bool IsFromThisAppDomain
		{
			get
			{
				// fixme: what if it is contextbound?
				return (_clientSink == null);
			}
		}

		public object RealObject
		{
			get	{ return _realObject; }
			set { _realObject = value; }
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

		public IMessageSink ServerSink
		{
			get 
			{ 
				if (_serverSink == null) {
					_serverSink = _context.CreateServerObjectSinkChain((MarshalByRefObject)_realObject);
				}
				return _serverSink; 
			}
		}

		public Context Context
		{
			get { return _context; }
		}
	}
}
