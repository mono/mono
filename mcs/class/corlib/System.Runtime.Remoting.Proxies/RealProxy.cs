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

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace System.Runtime.Remoting.Proxies
{
	internal class TransparentProxy {
		public RealProxy _rp;
		IntPtr _class;
	}
	
	public abstract class RealProxy {

		Type class_to_proxy;
		internal Context _targetContext;
		MarshalByRefObject _server;
		internal Identity _objectIdentity;
		Object _objTP;

		protected RealProxy ()
		{
			throw new NotImplementedException ();
		}

		protected RealProxy (Type classToProxy) : this(classToProxy, (IntPtr) 0, null)
		{
		}

		internal RealProxy (Type classToProxy, ClientIdentity identity) : this(classToProxy, (IntPtr) 0, null)
		{
			_objectIdentity = identity;
		}

		protected RealProxy (Type classToProxy, IntPtr stub, object stubData)
		{
			if (!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
				throw new ArgumentException("object must be MarshalByRef");

			this.class_to_proxy = classToProxy;

			// TODO: Fix stub
			_objTP = InternalGetTransparentProxy();
		}

		public Type GetProxiedType() 
		{
			if (class_to_proxy.IsInterface) return typeof (MarshalByRefObject);
			else return class_to_proxy;
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
		}

		public abstract IMessage Invoke (IMessage msg);

		/* this is called from unmanaged code */
		internal static object PrivateInvoke (RealProxy rp, IMessage msg, out Exception exc,
						      out object [] out_args)
		{
			MonoMethodMessage mMsg = (MonoMethodMessage) msg;
			mMsg.LogicalCallContext = CallContext.CreateLogicalCallContext();
			
			if (mMsg.CallType == CallType.BeginInvoke) 
				mMsg.AsyncResult.CallMessage = mMsg;	// TODO: do this in the runtime

			IMethodReturnMessage res_msg = (IMethodReturnMessage)rp.Invoke (msg);

			if (res_msg.LogicalCallContext != null && res_msg.LogicalCallContext.HasInfo)
				CallContext.UpdateCurrentCallContext (res_msg.LogicalCallContext);

			exc = res_msg.Exception;

			// todo: remove throw exception from the runtime invoke
			if (null != exc) {
				out_args = null;
				throw exc.FixRemotingException();
			}
			else if (res_msg is IConstructionReturnMessage || mMsg.CallType == CallType.BeginInvoke) {
				out_args = res_msg.OutArgs;
			}
			else if (mMsg.CallType == CallType.Sync) {
				out_args = ProcessResponse (res_msg, mMsg);
			}
			else if (mMsg.CallType == CallType.EndInvoke) {
				out_args = ProcessResponse (res_msg, mMsg.AsyncResult.CallMessage);
			}
			else {
				out_args = res_msg.OutArgs;
			}

			return res_msg.ReturnValue;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern virtual object InternalGetTransparentProxy ();

		public virtual object GetTransparentProxy () 
		{
			return _objTP;
		}

		[MonoTODO]
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

		static object[] ProcessResponse (IMethodReturnMessage mrm, IMethodCallMessage call)
		{
			// Check return type

			MethodInfo mi = (MethodInfo) call.MethodBase;
			if (mrm.ReturnValue != null && !mi.ReturnType.IsInstanceOfType (mrm.ReturnValue))
				throw new RemotingException ("Return value has an invalid type");

			// Check out parameters

			if (mrm.OutArgCount > 0)
			{
				ParameterInfo[] parameters = mi.GetParameters();
				int no = 0;
				foreach (ParameterInfo par in parameters)
					if (par.ParameterType.IsByRef) no++;
				
				object[] outArgs = new object [no];
				int narg = 0;
				int nout = 0;
	
				foreach (ParameterInfo par in parameters)
				{
					if (par.IsOut && !par.ParameterType.IsByRef)
					{
						// Special marshalling required
						
						object outArg = mrm.GetOutArg (nout++);
						if (outArg != null) {
							object local = call.GetArg (par.Position);
							if (local == null) throw new RemotingException ("Unexpected null value in local out parameter '" + par.Position + " " + par.Name + "'");
							RemotingServices.UpdateOutArgObject (par, local, outArg);
						}
					}
					else if (par.ParameterType.IsByRef)
					{
						object outArg = mrm.GetOutArg (nout++);
						if (outArg != null && !par.ParameterType.IsInstanceOfType (outArg))
						{
							throw new RemotingException ("Return argument '" + par.Name + "' has an invalid type");
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
