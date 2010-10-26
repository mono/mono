//
// System.Runtime.Remoting.Proxies.RemotingProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
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
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Threading;


namespace System.Runtime.Remoting.Proxies
{

	internal class RemotingProxy : RealProxy, IRemotingTypeInfo
	{
		static MethodInfo _cache_GetTypeMethod = typeof(System.Object).GetMethod("GetType");
		static MethodInfo _cache_GetHashCodeMethod = typeof(System.Object).GetMethod("GetHashCode");

		IMessageSink _sink;
		bool _hasEnvoySink;
		ConstructionCall _ctorCall;

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
			IMethodCallMessage mm = request as IMethodCallMessage;

			if (mm != null) {
				if (mm.MethodBase == _cache_GetHashCodeMethod)
					return new MethodResponse(ObjectIdentity.GetHashCode(), null, null, mm);
	
				if (mm.MethodBase == _cache_GetTypeMethod)
					return new MethodResponse(GetProxiedType(), null, null, mm);
			}
			
			IInternalMessage im = request as IInternalMessage;
			if (im != null) {
				if (im.Uri == null) im.Uri = _targetUri;
				im.TargetIdentity = _objectIdentity;
			}

			_objectIdentity.NotifyClientDynamicSinks (true, request, true, false);

			IMessage response;
			IMessageSink sink;

			// Needs to go through the client context sink?
			if (Thread.CurrentContext.HasExitSinks && !_hasEnvoySink)
				sink = Thread.CurrentContext.GetClientContextSinkChain ();
			else
				sink = _sink;

			MonoMethodMessage mMsg = request as MonoMethodMessage;
			if (mMsg == null || mMsg.CallType == CallType.Sync)
				response = sink.SyncProcessMessage (request);
			else
			{
				AsyncResult ares = mMsg.AsyncResult;
				IMessageCtrl mctrl = sink.AsyncProcessMessage (request, ares);
				if (ares != null) ares.SetMessageCtrl (mctrl);
				response = new ReturnMessage (null, new object[0], 0, null, mMsg);
			}

			_objectIdentity.NotifyClientDynamicSinks (false, request, true, false);

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
				cai.SetClientProxy ((MarshalByRefObject) GetTransparentProxy());
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

		internal IMessage ActivateRemoteObject (IMethodMessage request)
		{
			if (_ctorCall == null)	// It must be a WKO
				return new ConstructionResponse (this, null, (IMethodCallMessage) request);	// Ignore constructor call for WKOs

			_ctorCall.CopyFrom (request);
			return ActivationServices.Activate (this, _ctorCall);
		}

		public string TypeName 
		{ 
			get
			{
				if (_objectIdentity is ClientIdentity) {
					ObjRef oref = _objectIdentity.CreateObjRef (null);
					if (oref.TypeInfo != null) return oref.TypeInfo.TypeName;
				}
				return GetProxiedType().AssemblyQualifiedName;
			}
			
			set
			{
				throw new NotSupportedException ();
			}
		}
		
		public bool CanCastTo (Type fromType, object o)
		{
			if (_objectIdentity is ClientIdentity) {
				ObjRef oref = _objectIdentity.CreateObjRef (null);
				if (oref.IsReferenceToWellKnow && (fromType.IsInterface || GetProxiedType() == typeof(MarshalByRefObject))) return true;
				if (oref.TypeInfo != null) return oref.TypeInfo.CanCastTo (fromType, o);
			}
			return fromType.IsAssignableFrom (GetProxiedType());
		}
		
		~RemotingProxy()
		{
			if (_objectIdentity != null)
			{
				#if !DISABLE_REMOTING
				if (!(_objectIdentity is ClientActivatedIdentity))	// Local CBO proxy?
					RemotingServices.DisposeIdentity (_objectIdentity);
				#endif
			}
		}
		
	}
}
