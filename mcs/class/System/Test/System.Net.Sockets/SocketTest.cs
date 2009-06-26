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

#if NET_2_0
using System.Collections.Generic;
#endif

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
		[Ignore ("Bug #75158")] // Looks like MS fails after the .ctor, when you try to use the socket
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

			try {
				// should raise an exception because connect was bogus
				sock.EndConnect (ar);
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				Assert.AreEqual (10060, ex.ErrorCode, "#2");
			}
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

		static bool CFAConnected = false;
		static ManualResetEvent CFACalledBack;
		
		private static void CFACallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			CFAConnected = sock.Connected;
			
			if (sock.Connected) {
				sock.EndConnect (asyncResult);
			}

			CFACalledBack.Set ();
		}

		[Test]
		public void ConnectFailAsync ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			sock.Blocking = false;
			CFACalledBack = new ManualResetEvent (false);
			CFACalledBack.Reset ();

			/* Need a port that is not being used for
			 * anything...
			 */
			sock.BeginConnect (new IPEndPoint (IPAddress.Loopback,
							   114),
					   new AsyncCallback (CFACallback),
					   sock);
			CFACalledBack.WaitOne ();

			Assertion.AssertEquals ("ConnectFail", CFAConnected,
						false);
		}
		
#if !TARGET_JVM
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
#endif
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
				// Need to read the 10 bytes from the client to avoid a RST
				byte [] bytes = new byte [10];
				acc.Receive (bytes);
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

		byte[] buf = new byte[100];

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed1 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.ReceiveFrom (buf, ref ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed2 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Blocking = true;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed3 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.GetSocketOption (0, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed4 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.GetSocketOption (0, 0, null);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed5 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.GetSocketOption (0, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed6 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Listen (5);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed7 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Poll (100, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed8 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Receive (buf);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed9 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Receive (buf, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed10 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Receive (buf, 10, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed11 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.Receive (buf, 0, 10, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed12 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.ReceiveFrom (buf, 0, ref ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed13 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.ReceiveFrom (buf, 10, 0, ref ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed14 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.ReceiveFrom (buf, 0, 10, 0, ref ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed15 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Send (buf);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed16 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Send (buf, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed17 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Send (buf, 10, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed18 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.Send (buf, 0, 10, 0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed19 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.SendTo (buf, 0, ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed20 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.SendTo (buf, 10, 0, ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed21 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.SendTo (buf, 0, 10, 0, ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed22 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.SendTo (buf, ep);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed23 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			EndPoint ep = new IPEndPoint (IPAddress.Any, 31337);
			s.Close();

			s.Shutdown (0);
		}

		[Test]
		public void GetHashCodeTest ()
		{
			Socket server = new Socket (AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							9010);
			server.Bind (ep);
			server.Listen (1);

			Socket client = new Socket (AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp);
			int hashcodeA = client.GetHashCode ();
			client.Connect (ep);
			int hashcodeB = client.GetHashCode ();
			Assert.AreEqual (hashcodeA, hashcodeB, "#1");
			client.Close ();
			int hashcodeC = client.GetHashCode ();
#if NET_2_0
			Assert.AreEqual (hashcodeB, hashcodeC, "#2");
#else
			Assert.IsFalse (hashcodeB == hashcodeC, "#2");
#endif
			server.Close ();
		}

		static ManualResetEvent SocketError_event = new ManualResetEvent (false);

		private static void SocketError_callback (IAsyncResult ar)
		{
			Socket sock = (Socket)ar.AsyncState;
			
			if(sock.Connected) {
				sock.EndConnect (ar);
			}

			SocketError_event.Set ();
		}

		[Test]
		public void SocketError ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							BogusPort);
			
			SocketError_event.Reset ();

			sock.Blocking = false;
			sock.BeginConnect (ep, new AsyncCallback(SocketError_callback),
				sock);

			if (SocketError_event.WaitOne (2000, false) == false) {
				Assert.Fail ("SocketError wait timed out");
			}

			Assertion.AssertEquals ("SocketError #1", false,
						sock.Connected);

			int error;

			error = (int)sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			Assertion.AssertEquals ("SocketError #2", 10061,
						error);

			error = (int)sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			Assertion.AssertEquals ("SocketError #3", 10061,
						error);

			sock.Close ();
		}
		

#if NET_2_0
		[Test]
		public void SocketInformationCtor ()
		{
		}
		
		[Test]
		public void DontFragmentDefaultTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("DontFragmentDefaultTcp",
						false, sock.DontFragment);

			sock.Close ();
		}

		[Test]
		[Category ("NotOnMac")] // DontFragment doesn't work on Mac
		public void DontFragmentChangeTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.DontFragment = true;
			
			Assertion.AssertEquals ("DontFragmentChangeTcp",
						true, sock.DontFragment);

			sock.Close ();
		}
		
		[Test]
		public void DontFragmentDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assertion.AssertEquals ("DontFragmentDefaultUdp",
						false, sock.DontFragment);

			sock.Close ();
		}

		[Test]
		[Category ("NotOnMac")] // DontFragment doesn't work on Mac
		public void DontFragmentChangeUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			sock.DontFragment = true;
			
			Assertion.AssertEquals ("DontFragmentChangeUdp",
						true, sock.DontFragment);

			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void DontFragmentClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.DontFragment;
		}
		
		[Test]
		[Category ("NotWorking")] // Need to pick a non-IP AddressFamily that "works" on both mono and ms, this one only works on ms
		public void DontFragment ()
		{
			Socket sock = new Socket (AddressFamily.NetBios,
						  SocketType.Seqpacket,
						  ProtocolType.Unspecified);
			
			try {
				sock.DontFragment = true;
				Assert.Fail ("DontFragment #1");
			} catch (NotSupportedException) {
			} catch {
				Assert.Fail ("DontFragment #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		public void EnableBroadcastDefaultTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				bool value = sock.EnableBroadcast;
				Assert.Fail ("EnableBroadcastDefaultTcp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode, "EnableBroadcastDefaultTcp #2");
			} catch {
				Assert.Fail ("EnableBroadcastDefaultTcp #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void EnableBroadcastDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assertion.AssertEquals ("EnableBroadcastDefaultUdp",
						false, sock.EnableBroadcast);

			sock.Close ();
		}
		
		[Test]
		public void EnableBroadcastChangeTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.EnableBroadcast = true;
				Assert.Fail ("EnableBroadcastChangeTcp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode, "EnableBroadcastChangeTcp #2");
			} catch {
				Assert.Fail ("EnableBroadcastChangeTcp #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		public void EnableBroadcastChangeUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			sock.EnableBroadcast = true;
			
			Assertion.AssertEquals ("EnableBroadcastChangeUdp",
						true, sock.EnableBroadcast);

			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void EnableBroadcastClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			sock.Close ();
			
			bool val = sock.EnableBroadcast;
		}

		/* Can't test the default for ExclusiveAddressUse as
		 * it's different on different versions and service
		 * packs of windows
		 */
		[Test]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseUnbound ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.ExclusiveAddressUse = true;
			
			Assertion.AssertEquals ("ExclusiveAddressUseUnbound",
						true,
						sock.ExclusiveAddressUse);
			
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseBound ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Bind (new IPEndPoint (IPAddress.Any, 1235));
			sock.ExclusiveAddressUse = true;
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ExclusiveAddressUseClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.ExclusiveAddressUse;
		}
		
		[Test]
		public void IsBoundTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							BogusPort);
			
			Assertion.AssertEquals ("IsBoundTcp #1", false,
						sock.IsBound);
			
			sock.Bind (ep);
			Assertion.AssertEquals ("IsBoundTcp #2", true,
						sock.IsBound);

			sock.Listen (1);
			
			Socket sock2 = new Socket (AddressFamily.InterNetwork,
						   SocketType.Stream,
						   ProtocolType.Tcp);
			
			Assertion.AssertEquals ("IsBoundTcp #3", false,
						sock2.IsBound);
			
			sock2.Connect (ep);
			Assertion.AssertEquals ("IsBoundTcp #4", true,
						sock2.IsBound);
			
			sock2.Close ();
			Assertion.AssertEquals ("IsBoundTcp #5", true,
						sock2.IsBound);

			sock.Close ();
			Assertion.AssertEquals ("IsBoundTcp #6", true,
						sock.IsBound);
		}

		[Test]
		public void IsBoundUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							BogusPort);
			
			Assertion.AssertEquals ("IsBoundUdp #1", false,
						sock.IsBound);
			
			sock.Bind (ep);
			Assertion.AssertEquals ("IsBoundUdp #2", true,
						sock.IsBound);
			
			sock.Close ();
			Assertion.AssertEquals ("IsBoundUdp #3", true,
						sock.IsBound);
			

			sock = new Socket (AddressFamily.InterNetwork,
					   SocketType.Dgram,
					   ProtocolType.Udp);
			
			Assertion.AssertEquals ("IsBoundUdp #4", false,
						sock.IsBound);
			
			sock.Connect (ep);
			Assertion.AssertEquals ("IsBoundUdp #5", true,
						sock.IsBound);
			
			sock.Close ();
			Assertion.AssertEquals ("IsBoundUdp #6", true,
						sock.IsBound);
		}

		[Test]
		/* Should not throw an exception */
		public void IsBoundClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.IsBound;
		}
		
		/* Nothing much to test for LingerState */
		
		[Test]
		public void MulticastLoopbackDefaultTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				bool value = sock.MulticastLoopback;
				Assert.Fail ("MulticastLoopbackDefaultTcp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode, "MulticastLoopbackDefaultTcp #2");
			} catch {
				Assert.Fail ("MulticastLoopbackDefaultTcp #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void MulticastLoopbackChangeTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.MulticastLoopback = false;
				Assert.Fail ("MulticastLoopbackChangeTcp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode, "MulticastLoopbackChangeTcp #2");
			} catch {
				Assert.Fail ("MulticastLoopbackChangeTcp #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		public void MulticastLoopbackDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assertion.AssertEquals ("MulticastLoopbackDefaultUdp",
						true, sock.MulticastLoopback);
			
			sock.Close ();
		}
		
		[Test]
		public void MulticastLoopbackChangeUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			sock.MulticastLoopback = false;
			
			Assertion.AssertEquals ("MulticastLoopbackChangeUdp",
						false, sock.MulticastLoopback);
			
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void MulticastLoopbackClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.MulticastLoopback;
		}
		
		/* OSSupportsIPv6 depends on the environment */
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void ReceiveBufferSizeDefault ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("ReceiveBufferSizeDefault",
						8192, sock.ReceiveBufferSize);
			
			sock.Close ();
		}
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void ReceiveBufferSizeDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assertion.AssertEquals ("ReceiveBufferSizeDefaultUdp",
						8192, sock.ReceiveBufferSize);
			
			sock.Close ();
		}

		[Test]
		public void ReceiveBufferSizeChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.ReceiveBufferSize = 16384;
			
			Assertion.AssertEquals ("ReceiveBufferSizeChange",
						16384, sock.ReceiveBufferSize);
			
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ReceiveBufferSizeClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			int val = sock.ReceiveBufferSize;
		}
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void SendBufferSizeDefault ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("SendBufferSizeDefault",
						8192, sock.SendBufferSize);
			
			sock.Close ();
		}
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void SendBufferSizeDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assertion.AssertEquals ("SendBufferSizeDefaultUdp",
						8192, sock.SendBufferSize);
			
			sock.Close ();
		}

		[Test]
		public void SendBufferSizeChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.SendBufferSize = 16384;
			
			Assertion.AssertEquals ("SendBufferSizeChange",
						16384, sock.SendBufferSize);
			
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void SendBufferSizeClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			int val = sock.SendBufferSize;
		}
		
		/* No test for TTL default as it's platform dependent */
		[Test]
		public void TtlChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Ttl = 255;
			
			Assertion.AssertEquals ("TtlChange", 255, sock.Ttl);
			
			sock.Close ();
		}

		[Test]
		[Category ("NotOnMac")] // Mac doesn't throw when overflowing the ttl
		public void TtlChangeOverflow ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.Ttl = 256;
				Assert.Fail ("TtlChangeOverflow #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10022, ex.ErrorCode,
						 "TtlChangeOverflow #2");
			} catch {
				Assert.Fail ("TtlChangeoverflow #3");
			} finally {
				sock.Close ();
			}
		}
		
/* Apparently you can set TTL=0 on the ms runtime!!
			try {
				sock.Ttl = 0;
				Assert.Fail ("TtlChangeOverflow #4");
			} catch (SocketException ex) {
				Assert.AreEqual (10022, ex.ErrorCode,
						 "TtlChangeOverflow #5");
			} catch {
				Assert.Fail ("TtlChangeOverflow #6");
			} finally {
				sock.Close ();
			}
*/

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void TtlClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			int val = sock.Ttl;
		}
		
		[Test]
		public void UseOnlyOverlappedIODefault ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("UseOnlyOverlappedIODefault",
						false,
						sock.UseOnlyOverlappedIO);
			
			sock.Close ();
		}

		//
		// We need this because the Linux kernel in certain configurations
		// will end up rounding up the values passed on to the kernel
		// for socket send/receive timeouts.
		//
		int Approximate (int target, int value)
		{
			int epsilon = 10;
			
			if (value > target-10 && value < target+10)
				return target;
			return value;
		}
		
		[Test]
		public void UseOnlyOverlappedIOChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.UseOnlyOverlappedIO = true;
			
			Assertion.AssertEquals ("UseOnlyOverlappedIOChange",
						true,
						sock.UseOnlyOverlappedIO);
			
			sock.Close ();
		}

		[Test]
		/* Should not throw an exception */
		public void UseOnlyOverlappedIOClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.UseOnlyOverlappedIO;
		}
		
		[Test]
		public void SendTimeoutDefault ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("SendTimeoutDefault",
						0, sock.SendTimeout);
			
			sock.Close ();
		}

		[Test]
		public void SendTimeoutChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			/* Should be rounded up to 500, according to
			 * the MSDN docs, but the MS runtime doesn't
			 */
			sock.SendTimeout = 50;
			Assertion.AssertEquals ("SendTimeoutChange #1",
						50, Approximate (50, sock.SendTimeout));
			
			sock.SendTimeout = 2000;
			Assertion.AssertEquals ("SendTimeoutChange #2",
						2000, Approximate (2000, sock.SendTimeout));
			
			sock.SendTimeout = 0;
			Assertion.AssertEquals ("SendTimeoutChange #3",
						0, Approximate (0, sock.SendTimeout));
			
			/* Should be the same as setting 0 */
			sock.SendTimeout = -1;
			Assertion.AssertEquals ("SendTimeoutChange #4",
						0, sock.SendTimeout);

			sock.SendTimeout = 65536;
			Assertion.AssertEquals ("SendTimeoutChange #5",
						65536, Approximate (65536, sock.SendTimeout));
			
			try {
				sock.SendTimeout = -2;
				Assert.Fail ("SendTimeoutChange #8");
			} catch (ArgumentOutOfRangeException) {
			} catch {
				Assert.Fail ("SendTimeoutChange #9");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void SendTimeoutClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			int val = sock.SendTimeout;
		}
		
		[Test]
		public void ReceiveTimeoutDefault ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("ReceiveTimeoutDefault",
						0, sock.ReceiveTimeout);
			
			sock.Close ();
		}

		[Test]
		public void ReceiveTimeoutChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.ReceiveTimeout = 50;
			Assertion.AssertEquals ("ReceiveTimeoutChange #1",
						50, Approximate (50, sock.ReceiveTimeout));
			
			sock.ReceiveTimeout = 2000;
			Assertion.AssertEquals ("ReceiveTimeoutChange #2",
						2000, Approximate (2000, sock.ReceiveTimeout));
			
			sock.ReceiveTimeout = 0;
			Assertion.AssertEquals ("ReceiveTimeoutChange #3",
						0, sock.ReceiveTimeout);
			
			/* Should be the same as setting 0 */
			sock.ReceiveTimeout = -1;
			Assertion.AssertEquals ("ReceiveTimeoutChange #4",
						0, sock.ReceiveTimeout);

			sock.ReceiveTimeout = 65536;
			Assertion.AssertEquals ("ReceiveTimeoutChange #5",
						65536, Approximate (65536, sock.ReceiveTimeout));
			
			try {
				sock.ReceiveTimeout = -2;
				Assert.Fail ("ReceiveTimeoutChange #8");
			} catch (ArgumentOutOfRangeException) {
			} catch {
				Assert.Fail ("ReceiveTimeoutChange #9");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ReceiveTimeoutClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			int val = sock.ReceiveTimeout;
		}
		
		[Test]
		public void NoDelayDefaultTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			Assertion.AssertEquals ("NoDelayDefaultTcp", false,
						sock.NoDelay);
			
			sock.Close ();
		}

		[Test]
		public void NoDelayChangeTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.NoDelay = true;
			
			Assertion.AssertEquals ("NoDelayChangeTcp", true,
						sock.NoDelay);
			
			sock.Close ();
		}
		
		[Test]
		public void NoDelayDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			try {
				bool val = sock.NoDelay;
				Assert.Fail ("NoDelayDefaultUdp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode,
						 "NoDelayDefaultUdp #2");
			} catch {
				Assert.Fail ("NoDelayDefaultUdp #3");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void NoDelayChangeUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			try {
				sock.NoDelay = true;
				Assert.Fail ("NoDelayChangeUdp #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10042, ex.ErrorCode,
						 "NoDelayChangeUdp #2");
			} catch {
				Assert.Fail ("NoDelayChangeUdp #3");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void NoDelayClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			bool val = sock.NoDelay;
		}

		static bool BAAccepted = false;
		static Socket BASocket = null;
		static ManualResetEvent BACalledBack = new ManualResetEvent (false);
		
		private static void BACallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			
			BASocket = sock.EndAccept (asyncResult);
			
			BAAccepted = true;
			BACalledBack.Set ();
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void BeginAcceptNotBound ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);

			sock.BeginAccept (BACallback, sock);
			
			sock.Close ();
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void BeginAcceptNotListening ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);

			sock.Bind (new IPEndPoint (IPAddress.Any, 1236));
			
			sock.BeginAccept (BACallback, sock);
			
			sock.Close ();
		}

		[Test]
		public void BeginAccept ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1237);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			BACalledBack.Reset ();
			
			sock.BeginAccept (BACallback, sock);

			Socket conn = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			conn.Connect (ep);

			if (BACalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginAccept wait timed out");
			}
			
			Assertion.AssertEquals ("BeginAccept #1", true,
						BAAccepted);
			Assertion.AssertEquals ("BeginAccept #2", true,
						BASocket.Connected);
			Assertion.AssertEquals ("BeginAccept #3", false,
						sock.Connected);
			Assertion.AssertEquals ("BeginAccept #4", true,
						conn.Connected);

			BASocket.Close ();
			conn.Close ();
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void BeginAcceptClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			sock.BeginAccept (BACallback, sock);
		}

		static bool BADAccepted = false;
		static Socket BADSocket = null;
		static byte[] BADBytes;
		static int BADByteCount;
		static ManualResetEvent BADCalledBack = new ManualResetEvent (false);
		
		private static void BADCallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			
			BADSocket = sock.EndAccept (out BADBytes,
						    out BADByteCount,
						    asyncResult);
			
			BADAccepted = true;
			BADCalledBack.Set ();
		}

		[Test]
		public void BeginAcceptData ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1238);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			BADCalledBack.Reset ();
			
			sock.BeginAccept (256, BADCallback, sock);

			Socket conn = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			byte[] send_bytes = new byte[] {10, 11, 12, 13};
			
			conn.Connect (ep);
			conn.Send (send_bytes);

			if (BADCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginAcceptData wait timed out");
			}
			
			Assertion.AssertEquals ("BeginAcceptData #1", true,
						BADAccepted);
			Assertion.AssertEquals ("BeginAcceptData #2", true,
						BADSocket.Connected);
			Assertion.AssertEquals ("BeginAcceptData #3", false,
						sock.Connected);
			Assertion.AssertEquals ("BeginAcceptData #4", true,
						conn.Connected);
			Assertion.AssertEquals ("BeginAcceptData #5",
						send_bytes.Length,
						BADByteCount);
			
			/* The MS runtime gives the returned data in a
			 * much bigger array.  TODO: investigate
			 * whether it the size correlates to the first
			 * parameter in BeginAccept()
			 */
			Assert.IsFalse (BADBytes.Length == send_bytes.Length,
					"BeginAcceptData #6");

			for(int i = 0; i < send_bytes.Length; i++) {
				Assertion.AssertEquals ("BeginAcceptData #" + (i+7).ToString (), send_bytes[i], BADBytes[i]);
			}

			BADSocket.Close ();
			conn.Close ();
			sock.Close ();
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void BeginAcceptDataClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			sock.BeginAccept (256, BADCallback, sock);
		}

		[Test]
		public void BeginAcceptSocketUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket acc = new Socket (AddressFamily.InterNetwork,
						 SocketType.Dgram,
						 ProtocolType.Udp);
			
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1239);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.BeginAccept (acc, 256, BADCallback, sock);
				Assert.Fail ("BeginAcceptSocketUdp #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("BeginAcceptSocketUdp #2", 10022, ex.ErrorCode);
			} catch {
				Assert.Fail ("BeginAcceptSocketUdp #3");
			} finally {
				acc.Close ();
				sock.Close ();
			}
		}
		
		[Test]
		public void BeginAcceptSocketBound ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket acc = new Socket (AddressFamily.InterNetwork,
						 SocketType.Stream,
						 ProtocolType.Tcp);
			
			IPEndPoint ep1 = new IPEndPoint (IPAddress.Loopback,
							 1240);
			
			IPEndPoint ep2 = new IPEndPoint (IPAddress.Loopback,
							 1241);
			
			sock.Bind (ep1);
			sock.Listen (1);

			acc.Bind (ep2);
			
			try {
				sock.BeginAccept (acc, 256, BADCallback, sock);
				Assert.Fail ("BeginAcceptSocketBound #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("BeginAcceptSocketBound #2");
			} finally {
				acc.Close ();
				sock.Close ();
			}
		}
		
		[Test]
		public void BeginAcceptSocket ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket acc = new Socket (AddressFamily.InterNetwork,
						 SocketType.Stream,
						 ProtocolType.Tcp);
			
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1242);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			BADCalledBack.Reset ();
			
			sock.BeginAccept (acc, 256, BADCallback, sock);

			Socket conn = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			byte[] send_bytes = new byte[] {10, 11, 12, 13};
			
			conn.Connect (ep);
			conn.Send (send_bytes);

			if (BADCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginAcceptSocket wait timed out");
			}
			
			Assertion.AssertEquals ("BeginAcceptSocket #1", true,
						BADAccepted);
			Assertion.AssertEquals ("BeginAcceptSocket #2", true,
						BADSocket.Connected);
			Assertion.AssertEquals ("BeginAcceptSocket #3", false,
						sock.Connected);
			Assertion.AssertEquals ("BeginAcceptSocket #4", true,
						conn.Connected);
			Assertion.AssertEquals ("BeginAcceptSocket #5",
						send_bytes.Length,
						BADByteCount);
			Assertion.AssertEquals ("BeginAcceptSocket #6",
						AddressFamily.InterNetwork,
						acc.AddressFamily);
			Assertion.AssertEquals ("BeginAcceptSocket #7",
						SocketType.Stream,
						acc.SocketType);
			Assertion.AssertEquals ("BeginAcceptSocket #8",
						ProtocolType.Tcp,
						acc.ProtocolType);
			Assertion.AssertEquals ("BeginAcceptSocket #9",
						conn.LocalEndPoint,
						acc.RemoteEndPoint);
			
			/* The MS runtime gives the returned data in a
			 * much bigger array.  TODO: investigate
			 * whether it the size correlates to the first
			 * parameter in BeginAccept()
			 */
			Assert.IsFalse (BADBytes.Length == send_bytes.Length,
					"BeginAcceptSocket #10");

			for(int i = 0; i < send_bytes.Length; i++) {
				Assertion.AssertEquals ("BeginAcceptSocket #" + (i+11).ToString (), send_bytes[i], BADBytes[i]);
			}

			BADSocket.Close ();
			conn.Close ();
			acc.Close ();
			sock.Close ();
		}

		[Test]
		public void BeginAcceptSocketClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket acc = new Socket (AddressFamily.InterNetwork,
						 SocketType.Stream,
						 ProtocolType.Tcp);
			
			sock.Close ();
			
			try {
				sock.BeginAccept (acc, 256, BADCallback, null);
				Assert.Fail ("BeginAcceptSocketClosed #1");
			} catch (ObjectDisposedException) {
			} catch {
				Assert.Fail ("BeginAcceptSocketClosed #2");
			} finally {
				acc.Close ();
			}
		}

		[Test]
		public void BeginAcceptSocketAccClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket acc = new Socket (AddressFamily.InterNetwork,
						 SocketType.Stream,
						 ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1243);

			sock.Bind (ep);
			sock.Listen (1);
			
			acc.Close ();
			
			BADCalledBack.Reset ();
			
			try {
				sock.BeginAccept (acc, 256, BADCallback, null);
				Assert.Fail ("BeginAcceptSocketAccClosed #1");
			} catch (ObjectDisposedException) {
			} catch {
				Assert.Fail ("BeginAcceptSocketAccClosed #2");
			} finally {
				sock.Close ();
			}
		}
		
		static bool BCConnected = false;
		static ManualResetEvent BCCalledBack = new ManualResetEvent (false);
		
		private static void BCCallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			
			sock.EndConnect (asyncResult);
			BCConnected = true;
			
			BCCalledBack.Set ();
		}
		
		[Test]
		public void BeginConnectAddressPort ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1244);

			listen.Bind (ep);
			listen.Listen (1);
			
			BCCalledBack.Reset ();
			
			BCConnected = false;
			
			sock.BeginConnect (ip, 1244, BCCallback, sock);

			if (BCCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginConnectAddressPort wait timed out");
			}
			
			Assertion.AssertEquals ("BeginConnectAddressPort #1",
						true, BCConnected);
			
			sock.Close ();
			listen.Close ();
		}

		[Test]
		public void BeginConnectAddressPortNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = null;

			try {
				sock.BeginConnect (ip, 1244, BCCallback,
						   sock);
				Assert.Fail ("BeginConnectAddressPortNull #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("BeginConnectAddressPortNull #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void BeginConnectAddressPortListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1245);

			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.BeginConnect (ip, 1245, BCCallback, sock);
				Assert.Fail ("BeginConnectAddressPortListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("BeginConnectAddressPortListen #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void BeginConnectAddressPortClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			
			sock.Close ();
			
			sock.BeginConnect (ip, 1244, BCCallback, sock);
		}
		
		[Test]
		[Category ("NotOnMac")] // MacOSX will block attempting to connect to 127.0.0.4 causing the test to fail
		public void BeginConnectMultiple ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1246);
			IPAddress[] ips = new IPAddress[4];
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");

			listen.Bind (ep);
			listen.Listen (1);
			
			BCCalledBack.Reset ();
			
			BCConnected = false;
			
			sock.BeginConnect (ips, 1246, BCCallback, sock);
			
			/* Longer wait here, because the ms runtime
			 * takes a lot longer to not connect
			 */
			if (BCCalledBack.WaitOne (10000, false) == false) {
				Assert.Fail ("BeginConnectMultiple wait failed");
			}
			
			Assertion.AssertEquals ("BeginConnectMultiple #1",
						true, BCConnected);
			Assertion.AssertEquals ("BeginConnectMultiple #2",
						AddressFamily.InterNetwork,
						sock.RemoteEndPoint.AddressFamily);
			IPEndPoint remep = (IPEndPoint)sock.RemoteEndPoint;
			
			Assertion.AssertEquals ("BeginConnectMultiple #2",
						IPAddress.Loopback,
						remep.Address);
			
			sock.Close ();
			listen.Close ();
		}

		[Test]
		public void BeginConnectMultipleNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = null;
			
			try {
				sock.BeginConnect (ips, 1246, BCCallback,
						   sock);
				Assert.Fail ("BeginConnectMultipleNull #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("BeginConnectMultipleNull #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void BeginConnectMultipleListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = new IPAddress[4];
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1247);
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");
			
			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.BeginConnect (ips, 1247, BCCallback,
						   sock);
				Assert.Fail ("BeginConnectMultipleListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("BeginConnectMultipleListen #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void BeginConnectMultipleClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = new IPAddress[4];
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");
			
			sock.Close ();
			
			sock.BeginConnect (ips, 1247, BCCallback, sock);
		}
		
		[Test]
		public void BeginConnectHostPortNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.BeginConnect ((string)null, 0,
						   BCCallback, sock);
				Assert.Fail ("BeginConnectHostPort #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("BeginConnectHostPort #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void BeginConnectHostPortListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1248);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.BeginConnect ("localhost", 1248,
						   BCCallback, sock);
				Assert.Fail ("BeginConnectHostPortListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("BeginConnectHostPortListen #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")] // Need to pick a non-IP AddressFamily that "works" on both mono and ms, this one only works on ms
		public void BeginConnectHostPortNotIP ()
		{
			Socket sock = new Socket (AddressFamily.NetBios,
						  SocketType.Seqpacket,
						  ProtocolType.Unspecified);
			
			try {
				sock.BeginConnect ("localhost", 0, BCCallback,
						   sock);
				Assert.Fail ("BeginConnectHostPortNotIP #1");
			} catch (NotSupportedException) {
			} catch {
				Assert.Fail ("BeginConnectHostPortNotIP #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void BeginConnectHostPortClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			sock.BeginConnect ("localhost", 0, BCCallback, sock);
		}
		
		static bool BDDisconnected = false;
		static ManualResetEvent BDCalledBack = new ManualResetEvent (false);
		
		private static void BDCallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			
			sock.EndDisconnect (asyncResult);
			BDDisconnected = true;
			
			BDCalledBack.Set ();
		}
		
		[Test]
		[Category ("NotDotNet")] // "Needs XP or later"
		public void BeginDisconnect ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1254);
			
			listen.Bind (ep);
			listen.Listen (1);
			
			sock.Connect (ip, 1254);
			
			Assertion.AssertEquals ("BeginDisconnect #1", true,
						sock.Connected);
			
			sock.Shutdown (SocketShutdown.Both);

			BDCalledBack.Reset ();
			BDDisconnected = false;
			
			sock.BeginDisconnect (false, BDCallback, sock);
		
			if (BDCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginDisconnect wait timed out");
			}
			
			Assertion.AssertEquals ("BeginDisconnect #2", true,
						BDDisconnected);
			Assertion.AssertEquals ("BeginDisconnect #3", false,
						sock.Connected);
			
			sock.Close ();
			listen.Close ();
		}
		
		[Test]
		public void BeginReceiveSocketError ()
		{
		}
		
		[Test]
		public void BeginReceiveGeneric ()
		{
		}
		
		[Test]
		public void BeginReceiveGenericSocketError ()
		{
		}
		
		private static void BSCallback (IAsyncResult asyncResult)
		{
			Socket sock = (Socket)asyncResult.AsyncState;
			
			sock.EndSend (asyncResult);
		}
		
		[Test]
		public void BeginSendNotConnected ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			byte[] send_bytes = new byte[] {10, 11, 12, 13};
			
			try {
				sock.BeginSend (send_bytes, 0,
						send_bytes.Length,
						SocketFlags.None, BSCallback,
						sock);
				Assert.Fail ("BeginSendNotConnected #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("BeginSendNotConnected #2", 10057, ex.ErrorCode);
			} catch {
				Assert.Fail ("BeginSendNotConnected #3");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		public void BeginSendSocketError ()
		{
		}
		
		[Test]
		public void BeginSendGeneric ()
		{
		}
		
		[Test]
		public void BeginSendGenericSocketError ()
		{
		}
		
		[Test]
		public void BindTwice ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPEndPoint ep1 = new IPEndPoint (IPAddress.Loopback,
							1256);
			IPEndPoint ep2 = new IPEndPoint (IPAddress.Loopback,
							 1257);
			
			sock.Bind (ep1);
			
			try {
				sock.Bind (ep2);
				Assert.Fail ("BindTwice #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("BindTwice #2",
							10022, ex.ErrorCode);
			} catch {
				Assert.Fail ("BindTwice #3");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		public void Close ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1258);
			
			listen.Bind (ep);
			listen.Listen (1);
			
			sock.Connect (ep);

			Assertion.AssertEquals ("Close #1", true,
						sock.Connected);
			
			sock.Close (2);
			
			Thread.Sleep (3000);
			
			Assertion.AssertEquals ("Close #2", false,
						sock.Connected);
			
			listen.Close ();
		}
		
		[Test]
		public void ConnectAddressPort ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1249);

			listen.Bind (ep);
			listen.Listen (1);
			
			sock.Connect (ip, 1249);
			
			Assertion.AssertEquals ("ConnectAddressPort #1",
						true, sock.Connected);
			
			sock.Close ();
			listen.Close ();
		}

		[Test]
		public void ConnectAddressPortNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = null;

			try {
				sock.Connect (ip, 1249);
				Assert.Fail ("ConnectAddressPortNull #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("ConnectAddressPortNull #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void ConnectAddressPortListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1250);

			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.Connect (ip, 1250);
				Assert.Fail ("ConnectAddressPortListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("ConnectAddressPortListen #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ConnectAddressPortClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			
			sock.Close ();
			
			sock.Connect (ip, 1250);
		}
		
		[Test]
		[Category ("NotOnMac")] // MacOSX trashes the fd after the failed connect attempt to 127.0.0.4
		public void ConnectMultiple ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1251);
			IPAddress[] ips = new IPAddress[4];
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");

			listen.Bind (ep);
			listen.Listen (1);
			
			sock.Connect (ips, 1251);
			
			Assertion.AssertEquals ("ConnectMultiple #1",
						true, sock.Connected);
			Assertion.AssertEquals ("ConnectMultiple #2",
						AddressFamily.InterNetwork,
						sock.RemoteEndPoint.AddressFamily);
			IPEndPoint remep = (IPEndPoint)sock.RemoteEndPoint;
			
			Assertion.AssertEquals ("ConnectMultiple #2",
						IPAddress.Loopback,
						remep.Address);
			
			sock.Close ();
			listen.Close ();
		}

		[Test]
		public void ConnectMultipleNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = null;
			
			try {
				sock.Connect (ips, 1251);
				Assert.Fail ("ConnectMultipleNull #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("ConnectMultipleNull #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void ConnectMultipleListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = new IPAddress[4];
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							1252);
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");
			
			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.Connect (ips, 1252);
				Assert.Fail ("ConnectMultipleListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("ConnectMultipleListen #2");
			} finally {
				sock.Close ();
			}
		}
		
		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ConnectMultipleClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress[] ips = new IPAddress[4];
			
			ips[0] = IPAddress.Parse ("127.0.0.4");
			ips[1] = IPAddress.Parse ("127.0.0.3");
			ips[2] = IPAddress.Parse ("127.0.0.2");
			ips[3] = IPAddress.Parse ("127.0.0.1");
			
			sock.Close ();
			
			sock.Connect (ips, 1252);
		}
		
		[Test]
		public void ConnectHostPortNull ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.Connect ((string)null, 0);
				Assert.Fail ("ConnectHostPort #1");
			} catch (ArgumentNullException) {
			} catch {
				Assert.Fail ("ConnectHostPort #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		public void ConnectHostPortListen ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1253);
			
			sock.Bind (ep);
			sock.Listen (1);
			
			try {
				sock.Connect ("localhost", 1253);
				Assert.Fail ("ConnectHostPortListen #1");
			} catch (InvalidOperationException) {
			} catch {
				Assert.Fail ("ConnectHostPortListen #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")] // Need to pick a non-IP AddressFamily that "works" on both mono and ms, this one only works on ms
		public void ConnectHostPortNotIP ()
		{
			Socket sock = new Socket (AddressFamily.NetBios,
						  SocketType.Seqpacket,
						  ProtocolType.Unspecified);
			
			try {
				sock.Connect ("localhost", 0);
				Assert.Fail ("ConnectHostPortNotIP #1");
			} catch (NotSupportedException) {
			} catch {
				Assert.Fail ("ConnectHostPortNotIP #2");
			} finally {
				sock.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof(ObjectDisposedException))]
		public void ConnectHostPortClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.Close ();
			
			sock.Connect ("localhost", 0);
		}
		
		[Test]
		[Category ("NotDotNet")] // "Needs XP or later"
		public void Disconnect ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			Socket listen = new Socket (AddressFamily.InterNetwork,
						    SocketType.Stream,
						    ProtocolType.Tcp);
			IPAddress ip = IPAddress.Loopback;
			IPEndPoint ep = new IPEndPoint (ip, 1255);
			
			listen.Bind (ep);
			listen.Listen (1);
			
			sock.Connect (ip, 1255);
			
			Assertion.AssertEquals ("Disconnect #1", true,
						sock.Connected);
			
			sock.Shutdown (SocketShutdown.Both);

			sock.Disconnect (false);

			Assertion.AssertEquals ("BeginDisconnect #3", false,
						sock.Connected);
			
			sock.Close ();
			listen.Close ();
		}
		
		[Test]
		public void DuplicateAndClose ()
		{
		}
		
		[Test]
		public void IOControl ()
		{
		}
		
		[Test]
		public void ReceiveGeneric ()
		{
			int i;

			IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 1258);

			Socket listensock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listensock.Bind (endpoint);
			listensock.Listen(1);

			Socket sendsock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sendsock.Connect(endpoint);

			Socket clientsock = listensock.Accept();
			
			byte[] sendbuf = new byte[256];

			for(i = 0; i < 256; i++) {
				sendbuf[i] = (byte)i;
			}
			for (i = 4; i < 6; i++) {
				Assert.AreEqual (sendbuf[i], (byte)i,
						 "#1/" + i.ToString());
			}

			SocketError err;
			sendsock.Send (sendbuf, 0, 256, SocketFlags.None,
				       out err);


			byte[] recvbuf = new byte[256];
			List<ArraySegment<byte>> recvbuflist = new List<ArraySegment<byte>>(2);
			recvbuflist.Add(new ArraySegment<byte>(recvbuf, 4, 2));
			recvbuflist.Add(new ArraySegment<byte>(recvbuf, 20, 230));
			
			clientsock.Receive (recvbuflist);

			/* recvbuf should now hold the first 2 bytes
			 * of sendbuf from pos 4, and the next 230
			 * bytes of sendbuf from pos 20
			 */

			for (i = 0; i < 2; i++) {
				Assert.AreEqual (sendbuf[i], recvbuf[i + 4],
						 "#2/" + i.ToString());
			}
			for (i = 2; i < 232; i++) {
				Assert.AreEqual (sendbuf[i], recvbuf[i + 18],
						 "#2/" + i.ToString());
			}

			sendsock.Close ();
			clientsock.Close ();
			listensock.Close ();
		}
		
		[Test]
		public void SendGeneric ()
		{
			int i;

			IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 1259);

			Socket listensock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listensock.Bind (endpoint);
			listensock.Listen(1);

			Socket sendsock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sendsock.Connect(endpoint);

			Socket clientsock = listensock.Accept();

			byte[] sendbuf = new byte[256];
			List<ArraySegment<byte>> sendbuflist = new List<ArraySegment<byte>>(2);

			sendbuflist.Add(new ArraySegment<byte>(sendbuf, 4, 2));
			sendbuflist.Add(new ArraySegment<byte>(sendbuf, 20, 230));

			for(i = 0; i < 256; i++) {
				sendbuf[i] = (byte)i;
			}
			for (i = 4; i < 6; i++) {
				Assert.AreEqual (sendbuf[i], (byte)i,
						 "#1/" + i.ToString());
			}

			SocketError err;
			sendsock.Send (sendbuflist, SocketFlags.None, out err);

			
			byte[] recvbuf = new byte[256];

			clientsock.Receive (recvbuf);

			/* The first 2 bytes of recvbuf should now
			 * hold 2 bytes of sendbuf from pos 4, and the
			 * next 230 bytes of recvbuf should be sendbuf
			 * from pos 20
			 */

			for (i = 0; i < 2; i++) {
				Assert.AreEqual (recvbuf[i], sendbuf[i + 4],
						 "#2/" + i.ToString());
			}
			for (i = 2; i < 232; i++) {
				Assert.AreEqual (recvbuf[i], sendbuf[i + 18],
						 "#2/" + i.ToString());
			}

			sendsock.Close ();
			clientsock.Close ();
			listensock.Close ();
		}

		[Test]
		public void ListenNotBound ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			try {
				sock.Listen (1);
				Assert.Fail ("ListenNotBound #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("ListenNotBound #2",
							10022, ex.ErrorCode);
			} catch {
				Assert.Fail ("ListenNotBound #3");
			} finally {
				sock.Close ();
			}
		}
#endif

		static Socket CWRSocket;
		static bool CWRReceiving = true;
		static ManualResetEvent CWRReady = new ManualResetEvent (false);
		
		private static void CWRReceiveThread ()
		{
			byte[] buf = new byte[256];
			
			try {
				CWRSocket.Receive (buf);
			} catch (SocketException) {
				CWRReceiving = false;
			}

			CWRReady.Set ();
		}
		
		[Test]
		public void CloseWhileReceiving ()
		{
			CWRSocket = new Socket (AddressFamily.InterNetwork,
						SocketType.Dgram,
						ProtocolType.Udp);
			CWRSocket.Bind (new IPEndPoint (IPAddress.Loopback,
							1256));
			
			Thread recv_thread = new Thread (new ThreadStart (CWRReceiveThread));
			CWRReady.Reset ();
			recv_thread.Start ();
			Thread.Sleep (250);	/* Wait for the thread to be already receiving */

			CWRSocket.Close ();
			if (CWRReady.WaitOne (1000, false) == false) {
				Assert.Fail ("CloseWhileReceiving wait timed out");
			}
			
			Assert.IsFalse (CWRReceiving);
		}

		static bool RRCLastRead = false;
		static ManualResetEvent RRCReady = new ManualResetEvent (false);
		
		private static void RRCClientThread ()
		{
			byte[] bytes = new byte[8];
			int readbyte;
			
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			sock.Connect (new IPEndPoint (IPAddress.Loopback,
						      1257));
			
			NetworkStream stream = new NetworkStream (sock);

			readbyte = stream.ReadByte ();
			Assertion.AssertEquals ("ReceiveRemoteClosed #1",
						0, readbyte);
			
			stream.Read (bytes, 0, 0);

			readbyte = stream.ReadByte ();
			Assertion.AssertEquals ("ReceiveRemoteClosed #2",
						0, readbyte);
			
			stream.Read (bytes, 0, 0);

			readbyte = stream.ReadByte ();
			Assertion.AssertEquals ("ReceiveRemoteClosed #3",
						-1, readbyte);

			sock.Close ();

			RRCLastRead = true;
			RRCReady.Set ();
		}
		
		[Test]
		public void ReceiveRemoteClosed ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			sock.Bind (new IPEndPoint (IPAddress.Loopback, 1257));
			sock.Listen (1);
			
			RRCReady.Reset ();
			Thread client_thread = new Thread (new ThreadStart (RRCClientThread));
			client_thread.Start ();
			
			Socket client = sock.Accept ();
			NetworkStream stream = new NetworkStream (client);
			stream.WriteByte (0x00);
			stream.WriteByte (0x00);
			client.Close ();
			sock.Close ();

			RRCReady.WaitOne (1000, false);
			Assert.IsTrue (RRCLastRead);
		}

#if NET_2_0
		[Test]
                public void ConnectedProperty ()
                {
			TcpListener listener = new TcpListener (IPAddress.Loopback, 23456);
			listener.Start();

			Socket client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.Connect (IPAddress.Loopback, 23456);
			Socket server = listener.AcceptSocket ();

			try {
				server.EndSend(server.BeginSend (new byte[10], 0, 10, SocketFlags.None, null, null));
				client.Close ();
				try {
					server.EndReceive (server.BeginReceive (new byte[10], 0, 10, SocketFlags.None, null, null));
				} catch {
				}
				Assert.IsTrue (!client.Connected);
				Assert.IsTrue (!server.Connected);
			} finally {
				listener.Stop ();
				client.Close ();
				server.Close ();
			}
		}
#endif
		[Test]
		public void LingerOptions ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, 0);
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, new LingerOption (true, 1000));
			}
		}

		[Test]
		[ExpectedException (typeof (SocketException))]
		public void NullLingerOption ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, null);
			}
		}
	}
}

