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

		public virtual Type GetProxiedType() 
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
			IMethodReturnMessage res_msg = (IMethodReturnMessage)rp.Invoke (msg);

			exc = res_msg.Exception;
			out_args = res_msg.OutArgs;

			// todo: remove throw exception from the runtime invoke
			if (null != exc) 
				throw exc.FixRemotingException();

			return res_msg.ReturnValue;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern virtual object InternalGetTransparentProxy ();

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
	}
}
