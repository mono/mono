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
		bool _hasEnvoySink;
		ConstructionCall _ctorCall;
		string _targetUri;

		internal RemotingProxy (Type type, ClientIdentity identity) : base (type, identity)
		{
			_sink = identity.ChannelSink;
			_hasEnvoySink = false;
			_targetUri = identity.TargetUri;
		}

		internal RemotingProxy (Type type, string activationUrl, object[] activationAttributes) : base (type)
		{
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

			mMsg.Uri = _targetUri;
			((IInternalMessage)mMsg).TargetIdentity = _objectIdentity;

			_objectIdentity.NotifyClientDynamicSinks (true, request, true, false);

			IMessage response;
			IMessageSink sink;

			// Needs to go through the client context sink?
			if (Thread.CurrentContext.HasExitSinks && !_hasEnvoySink)
				sink = Thread.CurrentContext.GetClientContextSinkChain ();
			else
				sink = _sink;

			if (RemotingServices.IsAsyncMessage (request))
			{
				AsyncResult ares = ((MonoMethodMessage)request).AsyncResult;
				IMessageCtrl mctrl = sink.AsyncProcessMessage (request, ares);
				if (ares != null) ares.SetMessageCtrl (mctrl);
				response = new ReturnMessage (null, new object[0], 0, null, mMsg);
			}
			else
				response = sink.SyncProcessMessage (request);
			_objectIdentity.NotifyClientDynamicSinks (false, request, true, false);

			if (!(response is IConstructionReturnMessage))
				CheckResponse (response, request);

			return response;
		}

		internal void AttachIdentity (Identity identity)
		{
			_objectIdentity = identity;

			if (identity is ClientActivatedIdentity)	// It is a CBO
			{
				ClientActivatedIdentity cai = (ClientActivatedIdentity)identity;
				_targetContext = cai.Context;
				AttachServer (cai.GetServerObject ());
			}

			if (identity is ClientIdentity)
			{
				((ClientIdentity)identity).ClientProxy = (MarshalByRefObject) GetTransparentProxy();
				_targetUri = ((ClientIdentity)identity).TargetUri;
			}
			else
				_targetUri = identity.ObjectUri;

			if (_objectIdentity.EnvoySink != null) 
			{
				_sink = _objectIdentity.EnvoySink;
				_hasEnvoySink = true;
			}
			else 
				_sink = _objectIdentity.ChannelSink;

			_ctorCall = null;	// Object already constructed
		}

		IMessage ActivateRemoteObject (IMethodMessage request)
		{
			if (_ctorCall == null)	// It must be a WKO
				return new ReturnMessage (this, new object[0], 0, null, (IMethodCallMessage) request);	// Ignore constructor call for WKOs

			_ctorCall.CopyFrom (request);
			return ActivationServices.Activate (this, _ctorCall);
		}

		void CheckResponse (IMessage response, IMessage call)
		{
			IMethodReturnMessage mrm = (IMethodReturnMessage) response;
			if (mrm.Exception != null) return;

			// Check return type

			MethodInfo mi = (MethodInfo) mrm.MethodBase;
			if (mrm.ReturnValue != null && !mi.ReturnType.IsInstanceOfType (mrm.ReturnValue))
				throw new RemotingException ("Return value has an invalid type");

			// Check out parameters

			ParameterInfo[] parameters = mi.GetParameters();
			int narg = 0;

			foreach (ParameterInfo pi in parameters)
				if(pi.ParameterType.IsByRef) {
					object pval = mrm.GetOutArg (narg++);
					if (pval != null && !pi.ParameterType.IsInstanceOfType (pval))
						throw new RemotingException ("Return argument has an invalid type");
				}
		}
	}
}
