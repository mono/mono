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
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Threading;


namespace System.Runtime.Remoting.Proxies
{

	public class RemotingProxy : RealProxy 
	{
		static MethodInfo _cache_GetTypeMethod = typeof(System.Object).GetMethod("GetType");
		static MethodInfo _cache_GetHashCodeMethod = typeof(System.Object).GetMethod("GetHashCode");

		IMessageSink _sink;
		string _activationUrl;
		bool _hasEnvoySink;
		ConstructionCall _ctorCall;

		internal RemotingProxy (Type type, ClientIdentity identity) : base (type, identity)
		{
			_sink = identity.ChannelSink;
			_hasEnvoySink = false;
		}

		internal RemotingProxy (Type type, string activationUrl, object[] activationAttributes) : base (type)
		{
			_activationUrl = activationUrl;
			_hasEnvoySink = false;

			_ctorCall = ActivationServices.CreateConstructionCall (type, activationUrl, activationAttributes);
		}

		public override IMessage Invoke (IMessage request)
		{
			MonoMethodMessage mMsg = (MonoMethodMessage) request;

			if (mMsg.MethodBase.IsConstructor)
				return ActivateRemoteObject (mMsg);

			if (mMsg.MethodBase == _cache_GetHashCodeMethod)
				return new MethodResponse(ObjectIdentity.GetHashCode(), null, null, request as IMethodCallMessage);

			if (mMsg.MethodBase == _cache_GetTypeMethod)
				return new MethodResponse(GetProxiedType(), null, null, request as IMethodCallMessage);

			mMsg.Uri = _objectIdentity.ObjectUri;
			((IInternalMessage)mMsg).TargetIdentity = _objectIdentity;

			// Exiting from the context?
			if (!Thread.CurrentContext.IsDefaultContext && !_hasEnvoySink)
				return Thread.CurrentContext.GetClientContextSinkChain ().SyncProcessMessage (request);
			else
				return _sink.SyncProcessMessage (request);
		}

		IMessage ActivateRemoteObject (IMethodMessage request)
		{
			if (_activationUrl == null)
				return new ReturnMessage (this, new object[0], 0, null, (IMethodCallMessage) request);	// Ignore constructor call for WKOs

			IMethodReturnMessage response;

			_ctorCall.CopyFrom (request);

			if (_activationUrl == ChannelServices.CrossContextUrl)
			{
				// Cross context activation

				_objectIdentity = RemotingServices.CreateContextBoundObjectIdentity (_ctorCall.ActivationType);
				RemotingServices.SetMessageTargetIdentity (_ctorCall, _objectIdentity);
				response = _ctorCall.Activator.Activate (_ctorCall);
			}
			else
			{
				// Remote activation

				RemoteActivator remoteActivator = (RemoteActivator) RemotingServices.Connect (typeof (RemoteActivator), _activationUrl);

				try {
					response = remoteActivator.Activate (_ctorCall) as IMethodReturnMessage;
				}
				catch (Exception ex) {
					return new ReturnMessage (ex, (IMethodCallMessage)request);
				}

				ObjRef objRef = (ObjRef) response.ReturnValue;
				if (RemotingServices.GetIdentityForUri (objRef.URI) != null)
					throw new RemotingException("Inconsistent state during activation; there may be two proxies for the same object");

				_objectIdentity = RemotingServices.GetOrCreateClientIdentity (objRef, this);
			}

			if (_objectIdentity.EnvoySink != null) 
			{
				_sink = _objectIdentity.EnvoySink;
				_hasEnvoySink = true;
			}
			else 
				_sink = _objectIdentity.ChannelSink;

			_activationUrl = null;
			_ctorCall = null;
			return response;
		}
	}
}
