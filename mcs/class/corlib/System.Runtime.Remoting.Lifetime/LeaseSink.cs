//
// System.Runtime.Remoting.LeaseSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Lifetime
{
	// This sink updates lifetime information
	// about an object

	internal class LeaseSink: IMessageSink
	{
		IMessageSink _nextSink;

		public LeaseSink (IMessageSink nextSink)
		{
			_nextSink = nextSink;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			RenewLease (msg);
			return _nextSink.SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			RenewLease (msg);
			return _nextSink.AsyncProcessMessage (msg, replySink);
		}

		void RenewLease (IMessage msg)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);

			ILease lease = identity.Lease;
			if (lease != null && lease.CurrentLeaseTime < lease.RenewOnCallTime)
				lease.Renew (lease.RenewOnCallTime);
		}

		public IMessageSink NextSink 
		{ 
			get { return _nextSink; }
		}
	}
}
