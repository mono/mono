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
	internal abstract class Identity
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
		protected IMessageSink _channelSink = null;

		protected IMessageSink _envoySink = null;

		// The ObjRef 
		protected ObjRef _objRef;

		public Identity(string objectUri, Type objectType)
		{
			_objectUri = objectUri;
			_objectType = objectType;
		}

		public abstract ObjRef CreateObjRef (Type requestedType);

		public bool IsFromThisAppDomain
		{
			get
			{
				return (_channelSink == null);
			}
		}

		public IMessageSink ChannelSink
		{
			get { return _channelSink; }
			set { _channelSink = value; }
		}

		public IMessageSink EnvoySink
		{
			get { return _envoySink; }
		}

		public Type ObjectType
		{
			get { return _objectType; }
		}

		public string ObjectUri
		{
			get { return _objectUri; }
			set { _objectUri = value; }
		}
	}

	internal class ClientIdentity : Identity
	{
		MarshalByRefObject _proxyObject;

		public ClientIdentity (string objectUri, ObjRef objRef): base (objectUri, Type.GetType (objRef.TypeInfo.TypeName,true))
		{
			_objRef = objRef;
			_envoySink = (_objRef.EnvoyInfo != null) ? _objRef.EnvoyInfo.EnvoySinks : null;
		}

		public MarshalByRefObject ClientProxy
		{
			get	{ return _proxyObject; }
			set { _proxyObject = value; }
		}	

		public override ObjRef CreateObjRef (Type requestedType)
		{
			return _objRef;
		}
	}
}
