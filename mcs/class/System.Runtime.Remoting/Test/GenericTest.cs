//
// MonoTests.Remoting.GenericTest.cs
//
// Authors:
//     Robert Jordan  <robertj@gmx.net>
//

#if NET_2_0

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.Remoting
{
	class Server <T> : MarshalByRefObject
	{
		public T Field;

		public void Test ()
		{
		}

		public V Test2 <V> (V v)
		{
			return v;
		}

		public T Test3 (T t)
		{
			return t;
		}
	}

	[TestFixture]
	public class GenericTest
	{
		class Helper
		{
			public static void Test (string url)
			{
				// create server
				Server <int> server = new Server <int> ();
				RemotingServices.Marshal (server, "test");
				try {
					// create client
					Server <int> client = (Server <int>) RemotingServices.Connect (
						typeof (Server <int>), url);
			
					// invoke
					client.Test ();
					Assert.AreEqual ("hello", client.Test2 <string> ("hello"), "#01");
					Assert.AreEqual (42, client.Test3 (42), "#02");

				} finally {
					RemotingServices.Disconnect (server);
				}
			}
		}

		[Test]
		public void TestTcp ()
		{
			TcpChannel c = new TcpChannel (18181);
			Helper.Test ("tcp://127.0.0.1:18181/test");
			c.StopListening (null);
		}

		[Test]
		public void TestIpc ()
		{
			string portName = "ipc" + Guid.NewGuid ().ToString ("N");
			IpcChannel c = new IpcChannel (portName);
			// FIXME: the named pipe of the Win32 IpcServerChannel
			// seems to require a sleep because the pipe is not
			// ready immediately after creation.
			Thread.Sleep (1000);
			Helper.Test ("ipc://" + portName + "/test");
			c.StopListening (null);
		}

		[Test]
		[Ignore ("The SOAP formatter doesn't support generics.")]
		// FIXME: change the SOAP formatter to throw on generic types
		public void TestHttp ()
		{
			HttpChannel c = new HttpChannel (19191);
			Helper.Test ("http://127.0.0.1:19191/test");
			c.StopListening (null);
		}
	}
}

#endif
