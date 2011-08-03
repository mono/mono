//
// System.Runtime.Remoting.Proxies.RealProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez (lsg@ctv.es)
//   Patrik Torstensson
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Proxies
{
#pragma warning disable 169, 649
	internal class TransparentProxy {
		public RealProxy _rp;
		IntPtr _class;
		bool _custom_type_info;
	}
#pragma warning restore 169, 649
	
	[ComVisible (true)]
	public abstract class RealProxy {
		// other classes visible to the runtime 
		// derive from this class so keep these locals
		// in sync with the definition RealProxy 
		// in object-internals.h
		
#pragma warning disable 169, 414		
		#region Sync with object-internals.h
		Type class_to_proxy;
		internal Context _targetContext;
		MarshalByRefObject _server;
		int _targetDomainId = -1;
		internal string _targetUri;
		internal Identity _objectIdentity;
		Object _objTP;
		object _stubData;
        #endregion
#pragma warning restore 169, 414

		protected RealProxy ()
		{
		}

		protected RealProxy (Type classToProxy) : this(classToProxy, IntPtr.Zero, null)
		{
		}

		internal RealProxy (Type classToProxy, ClientIdentity identity) : this(classToProxy, IntPtr.Zero, null)
		{
			_objectIdentity = identity;
		}

		protected RealProxy (Type classToProxy, IntPtr stub, object stubData)
		{
			if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
				throw new ArgumentException("object must be MarshalByRef");

			this.class_to_proxy = classToProxy;

			if (stub != IntPtr.Zero)
				throw new NotSupportedException ("stub is not used in Mono");
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static Type InternalGetProxyType (object transparentProxy);
		
		public Type GetProxiedType() 
		{
			if (_objTP == null) {
				if (class_to_proxy.IsInterface) return typeof(MarshalByRefObject);
				else return class_to_proxy;
			}
			return InternalGetProxyType (_objTP);
		}

		public virtual ObjRef CreateObjRef (Type requestedType)
		{
			return RemotingServices.Marshal ((MarshalByRefObject) GetTransparentProxy(), null, requestedType);
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			Object obj = GetTransparentProxy();
			RemotingServices.GetObjectData (obj, info, context);            
		}
		
		internal Identity ObjectIdentity
		{
			get { return _objectIdentity; }
			set { _objectIdentity = value; }
		}
		
		[MonoTODO]
		public virtual IntPtr GetCOMIUnknown (bool fIsMarshalled)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void SetCOMIUnknown (IntPtr i)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual IntPtr SupportsInterface (ref Guid iid)
		{
			throw new NotImplementedException ();
		}
		
		public static object GetStubData (RealProxy rp)
		{
			return rp._stubData;
		}
		
		public static void SetStubData (RealProxy rp, object stubData)
		{
			rp._stubData = stubData;
		}

		public abstract IMessage Invoke (IMessage msg);

		/* this is called from unmanaged code */
		internal static object PrivateInvoke (RealProxy rp, IMessage msg, out Exception exc,
						      out object [] out_args)
		{
			MonoMethodMessage mMsg = (MonoMethodMessage) msg;
			mMsg.LogicalCallContext = CallContext.CreateLogicalCallContext (true);
			CallType call_type = mMsg.CallType;
#if MOONLIGHT
			bool is_remproxy = false;
#else
			bool is_remproxy = (rp is RemotingProxy);
#endif

			out_args = null;
			IMethodReturnMessage res_msg = null;
			
			if (call_type == CallType.BeginInvoke) 
				// todo: set CallMessage in runtime instead
				mMsg.AsyncResult.CallMessage = mMsg;

			if (call_type == CallType.EndInvoke)
				res_msg = (IMethodReturnMessage)mMsg.AsyncResult.EndInvoke ();

			// Check for constructor msg
			if (mMsg.MethodBase.IsConstructor) 
			{
#if !MOONLIGHT
				if (is_remproxy) 
					res_msg = (IMethodReturnMessage) (rp as RemotingProxy).ActivateRemoteObject ((IMethodMessage) msg);
				else 
#endif
					msg = new ConstructionCall (rp.GetProxiedType ());
			}
				
			if (null == res_msg) 
			{
				bool failed = false;
				
				try {
					res_msg = (IMethodReturnMessage)rp.Invoke (msg);
				} catch (Exception ex) {
					failed = true;
					if (call_type == CallType.BeginInvoke) {
						// If async dispatch crashes, don't propagate the exception.
						// The exception will be raised when calling EndInvoke.
						mMsg.AsyncResult.SyncProcessMessage (new ReturnMessage (ex, msg as IMethodCallMessage));
						res_msg = new ReturnMessage (null, null, 0, null, msg as IMethodCallMessage);
					} else
						throw;
				}
				
				// Note, from begining this code used AsyncResult.IsCompleted for
				// checking if it was a remoting or custom proxy, but in some
				// cases the remoting proxy finish before the call returns
				// causing this method to be called, therefore causing all kind of bugs.
				if ((!is_remproxy) && call_type == CallType.BeginInvoke && !failed)
				{
					IMessage asyncMsg = null;

					// allow calltype EndInvoke to finish
					asyncMsg = mMsg.AsyncResult.SyncProcessMessage (res_msg as IMessage);
					out_args = res_msg.OutArgs;
					res_msg = new ReturnMessage (asyncMsg, null, 0, null, res_msg as IMethodCallMessage);
				}
			}
			
			if (res_msg.LogicalCallContext != null && res_msg.LogicalCallContext.HasInfo)
				CallContext.UpdateCurrentCallContext (res_msg.LogicalCallContext);

			exc = res_msg.Exception;

			// todo: remove throw exception from the runtime invoke
			if (null != exc) {
				out_args = null;
				throw exc.FixRemotingException();
			}
			else if (res_msg is IConstructionReturnMessage) {
				if (out_args == null)
					out_args = res_msg.OutArgs;
			}
			else if (mMsg.CallType == CallType.BeginInvoke) {
				// We don't have OutArgs in this case.
			}
			else if (mMsg.CallType == CallType.Sync) {
				out_args = ProcessResponse (res_msg, mMsg);
			}
			else if (mMsg.CallType == CallType.EndInvoke) {
				out_args = ProcessResponse (res_msg, mMsg.AsyncResult.CallMessage);
			}
			else {
				if (out_args == null)
					out_args = res_msg.OutArgs;
			}

			return res_msg.ReturnValue;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern virtual object InternalGetTransparentProxy (string className);

		public virtual object GetTransparentProxy () 
		{
			if (_objTP == null) 
			{
				string name;
				IRemotingTypeInfo rti = this as IRemotingTypeInfo;
				
				if (rti != null) {
					name = rti.TypeName;
					if (name == null || name == typeof(MarshalByRefObject).AssemblyQualifiedName)
						name = class_to_proxy.AssemblyQualifiedName;
				}
				else
					name = class_to_proxy.AssemblyQualifiedName;
					
				_objTP = InternalGetTransparentProxy (name);
			}
			return _objTP;
		}

		[MonoTODO]
		[ComVisible (true)]
		public IConstructionReturnMessage InitializeServerObject(IConstructionCallMessage ctorMsg)
		{
			throw new NotImplementedException();
		}

		protected void AttachServer(MarshalByRefObject s)
		{
			_server = s;
		}

		protected MarshalByRefObject DetachServer()
		{
			MarshalByRefObject ob = _server;
			_server = null;
			return ob;
		}

		protected MarshalByRefObject GetUnwrappedServer()
		{
			return _server;
		}
		
		internal void SetTargetDomain (int domainId)
		{
			_targetDomainId = domainId;
		}
		
		// Called by the runtime
		internal object GetAppDomainTarget ()
		{
			if (_server == null) {
				ClientActivatedIdentity identity = RemotingServices.GetIdentityForUri (_targetUri) as ClientActivatedIdentity;
				if (identity == null) throw new RemotingException ("Server for uri '" + _targetUri + "' not found");
				_server = identity.GetServerObject ();
			}
			return _server;
		}

		static object[] ProcessResponse (IMethodReturnMessage mrm, MonoMethodMessage call)
		{
			// Check return type

			MethodInfo mi = (MethodInfo) call.MethodBase;
			if (mrm.ReturnValue != null && !mi.ReturnType.IsInstanceOfType (mrm.ReturnValue))
				throw new InvalidCastException ("Return value has an invalid type");

			// Check out parameters

			
			int no;
			
			if (call.NeedsOutProcessing (out no))
			{
				ParameterInfo[] parameters = mi.GetParameters();
				object[] outArgs = new object [no];
				int narg = 0;
	
				foreach (ParameterInfo par in parameters)
				{
					if (par.IsOut && !par.ParameterType.IsByRef)
					{
						// Special marshalling required
						object outArg = par.Position < mrm.ArgCount ? mrm.GetArg (par.Position) : null;
						if (outArg != null) {
							object local = call.GetArg (par.Position);
							if (local == null) throw new RemotingException ("Unexpected null value in local out parameter '" + par.Name + "'");
							RemotingServices.UpdateOutArgObject (par, local, outArg);
						}
					}
					else if (par.ParameterType.IsByRef)
					{
						object outArg = par.Position < mrm.ArgCount ? mrm.GetArg (par.Position) : null;
						if (outArg != null && !par.ParameterType.GetElementType ().IsInstanceOfType (outArg))
						{
							throw new InvalidCastException ("Return argument '" + par.Name + "' has an invalid type");
						}
						outArgs [narg++] = outArg;
					}
				}
				return outArgs;
			}
			else
				return new object [0];
		}
	}
}
