//
// System.Runtime.Remoting.Proxies.RealProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
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
		
		protected RealProxy ()
		{
			throw new NotImplementedException ();
		}

		protected RealProxy (Type classToProxy)
		{
			this.class_to_proxy = classToProxy;
		}

		protected RealProxy (Type classToProxy, IntPtr stub, object stubData)
		{
			throw new NotImplementedException ();
		}

		public virtual ObjRef CreateObjRef (Type requestedType)
		{
			throw new NotImplementedException ();
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
