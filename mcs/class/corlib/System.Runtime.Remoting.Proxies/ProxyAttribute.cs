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
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Proxies {

	[AttributeUsage (AttributeTargets.Class)]
	public class ProxyAttribute : Attribute, IContextAttribute
	{
		public ProxyAttribute ()
		{
		}

		public virtual MarshalByRefObject CreateInstance (Type serverType)
		{
			RemotingProxy proxy = new RemotingProxy (serverType, ChannelServices.CrossContextUrl, null);
			return (MarshalByRefObject) proxy.GetTransparentProxy();
		}

		public virtual RealProxy CreateProxy (ObjRef objref, Type serverType, object serverObject, Context serverContext)
		{
			return RemotingServices.GetRealProxy (RemotingServices.GetProxyForRemoteObject (objref, serverType));
		}

		public void GetPropertiesForNewContext (IConstructionCallMessage msg)
		{
			// Nothing to add
		}

		public bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			return true;
		}
	}
}

