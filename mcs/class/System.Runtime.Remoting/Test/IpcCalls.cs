//
// System.Runtime.Remoting.Test.IpcCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//         Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
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

#if NET_2_0

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class IpcSyncCallTest : SyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new IpcChannelManager ();
		}
	}

	[TestFixture]
	public class IpcAsyncCallTest : AsyncCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new IpcChannelManager ();
		}
	}

	[TestFixture]
	public class IpcReflectionCallTest : ReflectionCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new IpcChannelManager ();
		}
	}

	[TestFixture]
	public class IpcDelegateCallTest : DelegateCallTest
	{
		public override ChannelManager CreateChannelManager ()
		{
			return new IpcChannelManager ();
		}
	}

	[Serializable]
	public class IpcChannelManager : ChannelManager
	{
		public override IChannelSender CreateClientChannel ()
		{
			return new IpcChannel ();
		}

		public override IChannelReceiver CreateServerChannel ()
		{
			// simulate the Tcp/HttpChannel(0) semantics with a GUID.
			string portName = "ipc" + Guid.NewGuid ().ToString ("N");
			return new IpcChannel (portName);
		}
	}
}

#endif
