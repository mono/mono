//
// System.Runtime.Remoting.Proxies.RemotingProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Runtime.CompilerServices;


namespace System.Runtime.Remoting.Proxies
{

	public class RemotingProxy : RealProxy {

		IMessageSink sink;
		
		public RemotingProxy (Type type, IMessageSink sink) : base (type)
		{
			this.sink = sink;
		}

		public override IMessage Invoke (IMessage request)
		{
			return sink.SyncProcessMessage (request);
		}

	}
}
