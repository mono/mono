//
// MonoTests.Remoting.TcpCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class TcpSyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new TcpChannelManager ();
		}
	}

	[TestFixture]
	public class TcpAsyncCallTest : AsyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new TcpChannelManager ();
		}
	}

	[TestFixture]
	public class TcpReflectionCallTest : ReflectionCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new TcpChannelManager ();
		}
	}

	[TestFixture]
	public class TcpDelegateCallTest : DelegateCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new TcpChannelManager ();
		}
	}

	[Serializable]
	public class TcpChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			Hashtable props = new Hashtable ();
			props["port"] = 0;
			props["bindTo"] = "127.0.0.1";
			return new TcpChannel (props, null, null);
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			Hashtable props = new Hashtable ();
			props["port"] = 0;
			props["bindTo"] = "127.0.0.1";
			return new TcpChannel (props, null, null);
		}
	}
}

