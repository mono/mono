//
// System.Runtime.Remoting.Identity.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting
{
	internal class Identity
	{
		// An Identity object holds remoting information about
		// an object. It can be used to store client side information
		// (information about how to reach the remote server),
		// and also to store server side information (information
		// about how to dispatch messages to the object in the server).

		protected Type _objectType;

		// URI of the object
		protected string _objectUri;

		// Message sink to use to send a message to the remote server
		protected IMessageSink _clientSink = null;

		public Identity(string objectUri, Type objectType)
		{
			_objectUri = objectUri;
			_objectType = objectType;
		}

		public ObjRef CreateObjRef (Type requestedType)
		{
			if (requestedType == null) requestedType = _objectType;

			ObjRef res = new ObjRef ();
			res.TypeInfo = new TypeInfo(requestedType);
			res.URI = _objectUri;
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

		public IMessageSink ClientSink
		{
			get { return _clientSink; }
			set { _clientSink = value; }
		}

		public Type ObjectType
		{
			get { return _objectType; }
		}

		public string ObjectUri
		{
			get { return _objectUri; }
		}
	}

	internal class ClientIdentity : Identity
	{
		MarshalByRefObject _proxyObject;

		public ClientIdentity (string objectUri, Type objectType): base (objectUri, objectType)
		{
		}

		public MarshalByRefObject ClientProxy
		{
			get	{ return _proxyObject; }
			set { _proxyObject = value; }
		}	
	}
}
