//
// System.Runtime.Remoting.Proxies.RealProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez (lsg@ctv.es)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;


namespace System.Runtime.Remoting.Proxies
{
	internal class TransparentProxy {
		public RealProxy _rp;
		IntPtr _class;
	}
	
	public abstract class RealProxy {

		Type class_to_proxy;
		Identity _objectIdentity;

		protected RealProxy ()
		{
			throw new NotImplementedException ();
		}

		protected RealProxy (Type classToProxy)
		{
			this.class_to_proxy = classToProxy;
		}

		internal RealProxy (Type classToProxy, Identity identity)
		{
			this.class_to_proxy = classToProxy;
			_objectIdentity = identity;
		}

		protected RealProxy (Type classToProxy, IntPtr stub, object stubData)
		{
			throw new NotImplementedException ();
		}

		public virtual ObjRef CreateObjRef (Type requestedType)
		{
			return _objectIdentity.CreateObjRef (requestedType);
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
			return res_msg.ReturnValue;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern virtual object GetTransparentProxy ();
	
	}

}
