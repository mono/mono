//
// System.Runtime.Remoting.Proxies.ProxyAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// Copyright (C) Ximian, Inc 2002.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Proxies {

	[AttributeUsage (AttributeTargets.Class)]
	public class ProxyAttribute : Attribute
	{
		public ProxyAttribute ()
		{
		}

		[MonoTODO]
		public virtual MarshalByRefObject CreateInstance (Type serverType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual RealProxy CreateProxy (ObjRef objref, Type serverType, object serverObject, Context serverContext)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void GetPropertiesForNewContext (IConstructionCallMessage mesg)
		{
		}

		[MonoTODO]
		public bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			throw new NotImplementedException ();
		}
	}
}

