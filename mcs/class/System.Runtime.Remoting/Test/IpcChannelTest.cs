//
// MonoTests.Remoting.IpcChannelTests.cs
//
// Authors:
// 	Robert Jordan (robertj@gmx.net)
//

#if NET_2_0

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	[TestFixture]
	public class IpcChannelTest
	{
		[Test]
		public void Bug81653 ()
		{
			IpcClientChannel c = new IpcClientChannel ();
			ChannelDataStore cd = new ChannelDataStore (new string[] { "foo" });
			string objectUri;
			c.CreateMessageSink (null, cd, out objectUri);
		}
	}
}

#endif
