//
// System.Runtime.Remoting.LeaseSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
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
			#if !DISABLE_REMOTING
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);

			ILease lease = identity.Lease;
			if (lease != null && lease.CurrentLeaseTime < lease.RenewOnCallTime)
				lease.Renew (lease.RenewOnCallTime);
			#endif
		}

		public IMessageSink NextSink 
		{ 
			get { return _nextSink; }
		}
	}
}
