//
// MonoTests.Remoting.HttpCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class HttpSyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	[TestFixture]
	public class HttpAsyncCallTest : AsyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	[TestFixture]
	public class HttpReflectionCallTest : ReflectionCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	[TestFixture]
	public class HttpDelegateCallTest : DelegateCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new HttpChannelManager ();
		}
	}

	[Serializable]
	public class HttpChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			return new HttpChannel ();
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			return new HttpChannel (9739);
		}
	}
}

