// System.Net.Sockets.SocketTest.cs
//
// Authors:
//    Brad Fitzpatrick (brad@danga.com)
//    Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// (C) Copyright 2003 Brad Fitzpatrick
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketTest
	{
		// note: also used in SocketCas tests
		public const string BogusAddress = "192.168.244.244";
		public const int BogusPort = 23483;

		[Test]
		public void ConnectIPAddressAny ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Any, 0);

			try {
				using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
					s.Connect (ep);
					s.Close ();
				}
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				Assert.AreEqual (10049, ex.ErrorCode, "#2");
			}

			try {
				using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
					s.Connect (ep);
					s.Close ();
				}
				Assert.Fail ("#3");
			} catch (SocketException ex) {
				Assert.AreEqual (10049, ex.ErrorCode, "#4");
			}
		}

		[Test]
		[Ignore ("Bug #75158")]
		public void IncompatibleAddress ()
		{
			IPEndPoint epIPv6 = new IPEndPoint (IPAddress.IPv6Any,
								0);

			try {
				using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)) {
					s.Connect (epIPv6);
					s.Close ();
				}
				Assert.Fail ("#1");
			} catch (SocketException ex) {
#if !NET_2_0
				// invalid argument
				int expectedError = 10022;
#else
				// address incompatible with protocol
				int expectedError = 10047;
#endif
				Assert.AreEqual (expectedError, ex.ErrorCode,
						"#2");
			}
		}

		[Test]
		[Category ("InetAccess")]
		public void EndConnect ()
		{
		    IPAddress ipOne = IPAddress.Parse (BogusAddress);
		    IPEndPoint ipEP = new IPEndPoint (ipOne, BogusPort);
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
			IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 1234);
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

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void SetSocketOptionBoolean ()
		{
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 1);
			Socket sock = new Socket (ep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			try {
				sock.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void TestSelect1 ()
		{
			Socket srv = CreateServer ();
			ClientSocket clnt = new ClientSocket (srv.LocalEndPoint);
			Thread th = new Thread (new ThreadStart (clnt.ConnectSleepClose));
			Socket acc = null;
			try {
				th.Start ();
				acc = srv.Accept ();
				clnt.Write ();
				ArrayList list = new ArrayList ();
				ArrayList empty = new ArrayList ();
				list.Add (acc);
				Socket.Select (list, empty, empty, 100);
				Assertion.AssertEquals ("#01", 0, empty.Count);
				Assertion.AssertEquals ("#02", 1, list.Count);
				Socket.Select (empty, list, empty, 100);
				Assertion.AssertEquals ("#03", 0, empty.Count);
				Assertion.AssertEquals ("#04", 1, list.Count);
				Socket.Select (list, empty, empty, -1);
				Assertion.AssertEquals ("#05", 0, empty.Count);
				Assertion.AssertEquals ("#06", 1, list.Count);
			} finally {
				if (acc != null)
					acc.Close ();
				srv.Close ();
			}
		}

		static Socket CreateServer ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sock.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			sock.Listen (1);
			return sock;
		}

		class ClientSocket {
			Socket sock;
			EndPoint ep;

			public ClientSocket (EndPoint ep)
			{
				this.ep = ep;
				sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}

			public void ConnectSleepClose ()
			{
				sock.Connect (ep);
				Thread.Sleep (2000);
				sock.Close ();
			}

			public void Write ()
			{
				byte [] b = new byte [10];
				sock.Send (b);
			}
		}
	}
}

