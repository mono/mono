// System.Net.Sockets.TcpClientTest.cs
//
// Authors:
//    Brad Fitzpatrick (brad@danga.com)
//
// (C) Copyright 2003 Brad Fitzpatrick
//

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketTest
	{
		[Test]
		public void EndConnect ()
		{
		    IPAddress ipOne = IPAddress.Parse ("192.168.244.244");   // something bogus
		    IPEndPoint ipEP = new IPEndPoint (ipOne, 23483);  // something bogus
		    Socket sock = new Socket (ipEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		    IAsyncResult ar = sock.BeginConnect (ipEP, null, null);
		    bool gotException = false;

		    try {
			sock.EndConnect (ar);  // should raise an exception because connect was bogus
		    } catch {
			gotException = true;
		    }

		    Assertion.AssertEquals ("A01", gotException, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SelectEmpty ()
		{
			ArrayList list = new ArrayList ();
			Socket.Select (list, list, list, 1000);
		}
		
		private bool BlockingConnect (bool block)
		{
			IPEndPoint ep = new IPEndPoint(IPAddress.Any, 1234);
			Socket server = new Socket(AddressFamily.InterNetwork,
						   SocketType.Stream,
						   ProtocolType.Tcp);
			server.Bind(ep);
			server.Blocking=block;

			server.Listen(0);

			Socket conn = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			conn.Connect (ep);

			Socket client = server.Accept();
			bool client_block = client.Blocking;

			client.Close();
			conn.Close();
			server.Close();
			
			return(client_block);
		}

		[Test]
		public void AcceptBlockingStatus()
		{
			bool block;

			block = BlockingConnect(true);
			Assertion.AssertEquals ("BlockingStatus01",
						block, true);

			block = BlockingConnect(false);
			Assertion.AssertEquals ("BlockingStatus02",
						block, false);
		}
	}

}

