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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Runtime.CompilerServices;


namespace System.Runtime.Remoting.Proxies
{

	public class RemotingProxy : RealProxy 
	{
		IMessageSink _sink;

		internal RemotingProxy (Type type, Identity identity) : base (type, identity)
		{
			_sink = identity.ClientSink;
		}

		public override IMessage Invoke (IMessage request)
		{
			((MonoMethodMessage)request).Uri = ObjectIdentity.ObjectUri;
			return _sink.SyncProcessMessage (request);
		}

	}
}
