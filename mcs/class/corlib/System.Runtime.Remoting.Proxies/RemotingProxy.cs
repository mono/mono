//
// System.Runtime.Remoting.Proxies.RemotingProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.CompilerServices;


namespace System.Runtime.Remoting.Proxies
{

	public class RemotingProxy : RealProxy 
	{
		static MethodInfo _cache_GetTypeMethod = typeof(System.Object).GetMethod("GetType");
		static MethodInfo _cache_GetHashCodeMethod = typeof(System.Object).GetMethod("GetHashCode");

		IMessageSink _sink;
		string _activationUrl;

		internal RemotingProxy (Type type, ClientIdentity identity) : base (type, identity)
		{
			_sink = identity.ChannelSink;
		}

		internal RemotingProxy (Type type, string activationUrl, IMessageSink activationSink) : base (type)
		{
			_sink = activationSink;
			_activationUrl = activationUrl;
		}

		public override IMessage Invoke (IMessage request)
		{
			MonoMethodMessage mMsg = (MonoMethodMessage) request;

			if (mMsg.MethodBase.IsConstructor)
				return ActivateRemoteObject (request);

			if (mMsg.MethodBase == _cache_GetHashCodeMethod)
				return new MethodResponse(ObjectIdentity.GetHashCode(), null, null, request as IMethodCallMessage);

			if (mMsg.MethodBase == _cache_GetTypeMethod)
				return new MethodResponse(GetProxiedType(), null, null, request as IMethodCallMessage);

			mMsg.Uri = _objectIdentity.ObjectUri;
			((IInternalMessage)mMsg).TargetIdentity = _objectIdentity;
			return _sink.SyncProcessMessage (request);
		}

		IMessage ActivateRemoteObject (IMessage request)
		{
			if (_activationUrl == null)
				return new ReturnMessage (this, new object[0], 0, null, (IMethodCallMessage) request);	// Ignore constructor call for WKOs

			IMethodReturnMessage response;

			ConstructionCall ctorCall = new ConstructionCall (request);

			if (_activationUrl == ChannelServices.CrossContextUrl)
			{
				// Cross context activation

				_objectIdentity = RemotingServices.CreateContextBoundObjectIdentity (ctorCall.ActivationType);
				RemotingServices.SetMessageTargetIdentity (ctorCall, _objectIdentity);
				response = (IConstructionReturnMessage) _sink.SyncProcessMessage (ctorCall);
			}
			else
			{
				// Remote activation

				MethodCall call = new MethodCall (_activationUrl, typeof(RemoteActivator).AssemblyQualifiedName, "Activate", new object[] {ctorCall} );
				response = (IMethodReturnMessage) _sink.SyncProcessMessage (call);
			
				if (response.Exception != null) return response;

				response = response.ReturnValue as IMethodReturnMessage;
				ObjRef objRef = (ObjRef) response.ReturnValue;
				_objectIdentity = RemotingServices.GetOrCreateClientIdentity (objRef, this);
			}

			if (_objectIdentity.EnvoySink != null) _sink = _objectIdentity.EnvoySink;
			else _sink = _objectIdentity.ChannelSink;

			_activationUrl = null;
			return response;
		}

	}
}
