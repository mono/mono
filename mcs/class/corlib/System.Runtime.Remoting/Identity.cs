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

		// URI of the object
		protected string _objectUri;

		// Message sink to use to send a message to the remote server
		protected IMessageSink _channelSink = null;

		protected IMessageSink _envoySink = null;

		DynamicPropertyCollection _clientDynamicProperties;
		DynamicPropertyCollection _serverDynamicProperties;

		// The ObjRef 
		protected ObjRef _objRef;
		
		// This flag is set when the identity is removed from the uri table.
		// It is needed because in some scenarios the runtime may try to
		// dispose the identity twice.
		bool _disposed = false;

		public Identity(string objectUri)
		{
			_objectUri = objectUri;
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

		public string ObjectUri
		{
			get { return _objectUri; }
			set { _objectUri = value; }
		}

		public bool IsConnected
		{
			get { return _objectUri != null; }
		}
		
		public bool Disposed
		{
			get { return _disposed; }
			set { _disposed = value; }
		}

		public DynamicPropertyCollection ClientDynamicProperties
		{
			get { 
				if (_clientDynamicProperties == null) _clientDynamicProperties = new DynamicPropertyCollection();
				return _clientDynamicProperties; 
			}
		}

		public DynamicPropertyCollection ServerDynamicProperties
		{
			get { 
				if (_serverDynamicProperties == null) _serverDynamicProperties = new DynamicPropertyCollection();
				return _serverDynamicProperties; 
			}
		}

		public bool HasClientDynamicSinks
		{
			get { return (_clientDynamicProperties != null && _clientDynamicProperties.HasProperties); }
		}

		public bool HasServerDynamicSinks
		{
			get { return (_serverDynamicProperties != null && _serverDynamicProperties.HasProperties); }
		}

		public void NotifyClientDynamicSinks  (bool start, IMessage req_msg, bool client_site, bool async)
		{
			if (_clientDynamicProperties != null && _clientDynamicProperties.HasProperties) 
				_clientDynamicProperties.NotifyMessage (start, req_msg, client_site, async);
		}

		public void NotifyServerDynamicSinks  (bool start, IMessage req_msg, bool client_site, bool async)
		{
			if (_serverDynamicProperties != null && _serverDynamicProperties.HasProperties) 
				_serverDynamicProperties.NotifyMessage (start, req_msg, client_site, async);
		}
	}

	internal class ClientIdentity : Identity
	{
		WeakReference _proxyReference;

		public ClientIdentity (string objectUri, ObjRef objRef): base (objectUri)
		{
			_objRef = objRef;
			_envoySink = (_objRef.EnvoyInfo != null) ? _objRef.EnvoyInfo.EnvoySinks : null;
		}

		public MarshalByRefObject ClientProxy
		{
			get	{ return (MarshalByRefObject) _proxyReference?.Target; }
			set { _proxyReference = new WeakReference (value); }
		}

		public override ObjRef CreateObjRef (Type requestedType)
		{
			return _objRef;
		}

		public string TargetUri
		{
			get { return _objRef.URI; }
		}
	}
}
