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

			/* UDP sockets use Any to disconnect
			try {
				using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
					s.Connect (ep);
					s.Close ();
				}
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				Assert.AreEqual (10049, ex.ErrorCode, "#2");
			}
			*/

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
			Assert.AreEqual (block, true, "BlockingStatus01");

			block = BlockingConnect(false);
			Assert.AreEqual (block, false, "BlockingStatus02");
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

		[Test] // Connect (IPEndPoint)
		public void Connect1_RemoteEP_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			try {
				s.Connect ((IPEndPoint) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("remoteEP", ex.ParamName, "#5");
			}
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

			Assert.AreEqual (CFAConnected, false, "ConnectFail");
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
				Assert.AreEqual (0, empty.Count, "#01");
				Assert.AreEqual (1, list.Count, "#02");
				Socket.Select (empty, list, empty, 100);
				Assert.AreEqual (0, empty.Count, "#03");
				Assert.AreEqual (1, list.Count, "#04");
				Socket.Select (list, empty, empty, -1);
				Assert.AreEqual (0, empty.Count, "#05");
				Assert.AreEqual (1, list.Count, "#06");
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
		public void Disposed2 ()
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.Close();

			s.Blocking = true;
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
		public void SocketErrorTest ()
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

			Assert.AreEqual (false, sock.Connected, "SocketError #1");

			int error;

			error = (int)sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			Assert.AreEqual (10061, error, "SocketError #2");

			error = (int)sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			Assert.AreEqual (10061, error, "SocketError #3");

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
			
			Assert.AreEqual (false, sock.DontFragment, "DontFragmentDefaultTcp");

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
			
			Assert.AreEqual (true, sock.DontFragment, "DontFragmentChangeTcp");

			sock.Close ();
		}
		
		[Test]
		public void DontFragmentDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assert.AreEqual (false, sock.DontFragment, "DontFragmentDefaultUdp");

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
			
			Assert.AreEqual (true, sock.DontFragment, "DontFragmentChangeUdp");

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
			
			Assert.AreEqual (false, sock.EnableBroadcast, "EnableBroadcastDefaultUdp");

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
			
			Assert.AreEqual (true, sock.EnableBroadcast, "EnableBroadcastChangeUdp");

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
			
			Assert.AreEqual (true, sock.ExclusiveAddressUse, "ExclusiveAddressUseUnbound");
			
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
			
			Assert.AreEqual (false, sock.IsBound, "IsBoundTcp #1");
			
			sock.Bind (ep);
			Assert.AreEqual (true, sock.IsBound, "IsBoundTcp #2");

			sock.Listen (1);
			
			Socket sock2 = new Socket (AddressFamily.InterNetwork,
						   SocketType.Stream,
						   ProtocolType.Tcp);
			
			Assert.AreEqual (false, sock2.IsBound, "IsBoundTcp #3");
			
			sock2.Connect (ep);
			Assert.AreEqual (true, sock2.IsBound, "IsBoundTcp #4");
			
			sock2.Close ();
			Assert.AreEqual (true, sock2.IsBound, "IsBoundTcp #5");

			sock.Close ();
			Assert.AreEqual (true, sock.IsBound, "IsBoundTcp #6");
		}

		[Test]
		public void IsBoundUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback,
							BogusPort);
			
			Assert.AreEqual (false, sock.IsBound, "IsBoundUdp #1");
			
			sock.Bind (ep);
			Assert.AreEqual (true, sock.IsBound, "IsBoundUdp #2");
			
			sock.Close ();
			Assert.AreEqual (true, sock.IsBound, "IsBoundUdp #3");
			

			sock = new Socket (AddressFamily.InterNetwork,
					   SocketType.Dgram,
					   ProtocolType.Udp);
			
			Assert.AreEqual (false, sock.IsBound, "IsBoundUdp #4");
			
			sock.Connect (ep);
			Assert.AreEqual (true, sock.IsBound, "IsBoundUdp #5");
			
			sock.Close ();
			Assert.AreEqual (true, sock.IsBound, "IsBoundUdp #6");
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
			
			Assert.AreEqual (true, sock.MulticastLoopback, "MulticastLoopbackDefaultUdp");
			
			sock.Close ();
		}
		
		[Test]
		public void MulticastLoopbackChangeUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			sock.MulticastLoopback = false;
			
			Assert.AreEqual (false, sock.MulticastLoopback, "MulticastLoopbackChangeUdp");
			
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
			
			Assert.AreEqual (8192, sock.ReceiveBufferSize, "ReceiveBufferSizeDefault");
			
			sock.Close ();
		}
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void ReceiveBufferSizeDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assert.AreEqual (8192, sock.ReceiveBufferSize, "ReceiveBufferSizeDefaultUdp");
			
			sock.Close ();
		}

		[Test]
		public void ReceiveBufferSizeChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.ReceiveBufferSize = 16384;
			
			Assert.AreEqual (16384, sock.ReceiveBufferSize, "ReceiveBufferSizeChange");
			
			sock.Close ();
		}

		[Test]
		[Category("NotWorking")] // We cannot totally remove buffers (minimum is set to 256
		public void BuffersCheck_None ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				int original = s.ReceiveBufferSize;
				s.ReceiveBufferSize = 0;
				Assert.AreEqual (0, s.ReceiveBufferSize, "ReceiveBufferSize " + original.ToString ());

				original = s.SendBufferSize;
				s.SendBufferSize = 0;
				Assert.AreEqual (0, s.SendBufferSize, "SendBufferSize " + original.ToString ());
			}
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
			
			Assert.AreEqual (8192, sock.SendBufferSize, "SendBufferSizeDefault");
			
			sock.Close ();
		}
		
		[Test]
		[Category("NotWorking")] // We have different defaults for perf reasons
		public void SendBufferSizeDefaultUdp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Dgram,
						  ProtocolType.Udp);
			
			Assert.AreEqual (8192, sock.SendBufferSize, "SendBufferSizeDefaultUdp");
			
			sock.Close ();
		}

		[Test]
		public void SendBufferSizeChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.SendBufferSize = 16384;
			
			Assert.AreEqual (16384, sock.SendBufferSize, "SendBufferSizeChange");
			
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
			
			Assert.AreEqual (255, sock.Ttl, "TtlChange");
			
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
			
			Assert.AreEqual (false, sock.UseOnlyOverlappedIO, "UseOnlyOverlappedIODefault");
			
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
			
			Assert.AreEqual (true, sock.UseOnlyOverlappedIO, "UseOnlyOverlappedIOChange");
			
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
			
			Assert.AreEqual (0, sock.SendTimeout, "SendTimeoutDefault");
			
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
			Assert.AreEqual (50, Approximate (50, sock.SendTimeout), "SendTimeoutChange #1");
			
			sock.SendTimeout = 2000;
			Assert.AreEqual (2000, Approximate (2000, sock.SendTimeout), "SendTimeoutChange #2");
			
			sock.SendTimeout = 0;
			Assert.AreEqual (0, Approximate (0, sock.SendTimeout), "SendTimeoutChange #3");
			
			/* Should be the same as setting 0 */
			sock.SendTimeout = -1;
			Assert.AreEqual (0, sock.SendTimeout, "SendTimeoutChange #4");

			sock.SendTimeout = 65536;
			Assert.AreEqual (65536, Approximate (65536, sock.SendTimeout), "SendTimeoutChange #5");
			
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
			
			Assert.AreEqual (0, sock.ReceiveTimeout, "ReceiveTimeoutDefault");
			
			sock.Close ();
		}

		[Test]
		public void ReceiveTimeoutChange ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.ReceiveTimeout = 50;
			Assert.AreEqual (50, Approximate (50, sock.ReceiveTimeout), "ReceiveTimeoutChange #1");
			
			sock.ReceiveTimeout = 2000;
			Assert.AreEqual (2000, Approximate (2000, sock.ReceiveTimeout), "ReceiveTimeoutChange #2");
			
			sock.ReceiveTimeout = 0;
			Assert.AreEqual (0, sock.ReceiveTimeout, "ReceiveTimeoutChange #3");
			
			/* Should be the same as setting 0 */
			sock.ReceiveTimeout = -1;
			Assert.AreEqual (0, sock.ReceiveTimeout, "ReceiveTimeoutChange #4");

			sock.ReceiveTimeout = 65536;
			Assert.AreEqual (65536, Approximate (65536, sock.ReceiveTimeout), "ReceiveTimeoutChange #5");
			
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
			
			Assert.AreEqual (false, sock.NoDelay, "NoDelayDefaultTcp");
			
			sock.Close ();
		}

		[Test]
		public void NoDelayChangeTcp ()
		{
			Socket sock = new Socket (AddressFamily.InterNetwork,
						  SocketType.Stream,
						  ProtocolType.Tcp);
			
			sock.NoDelay = true;
			
			Assert.AreEqual (true, sock.NoDelay, "NoDelayChangeTcp");
			
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
			
			Assert.AreEqual (true, BAAccepted, "BeginAccept #1");
			Assert.AreEqual (true, BASocket.Connected, "BeginAccept #2");
			Assert.AreEqual (false, sock.Connected, "BeginAccept #3");
			Assert.AreEqual (true, conn.Connected, "BeginAccept #4");

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
			
			Assert.AreEqual (true, BADAccepted, "BeginAcceptData #1");
			Assert.AreEqual (true, BADSocket.Connected, "BeginAcceptData #2");
			Assert.AreEqual (false, sock.Connected, "BeginAcceptData #3");
			Assert.AreEqual (true, conn.Connected, "BeginAcceptData #4");
			Assert.AreEqual (send_bytes.Length, BADByteCount, "BeginAcceptData #5");
			
			/* The MS runtime gives the returned data in a
			 * much bigger array.  TODO: investigate
			 * whether it the size correlates to the first
			 * parameter in BeginAccept()
			 */
			Assert.IsFalse (BADBytes.Length == send_bytes.Length,
					"BeginAcceptData #6");

			for(int i = 0; i < send_bytes.Length; i++) {
				Assert.AreEqual (send_bytes[i], BADBytes[i], "BeginAcceptData #" + (i+7).ToString ());
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
				Assert.AreEqual (10022, ex.ErrorCode, "BeginAcceptSocketUdp #2");
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
			
			Assert.AreEqual (true, BADAccepted, "BeginAcceptSocket #1");
			Assert.AreEqual (true, BADSocket.Connected, "BeginAcceptSocket #2");
			Assert.AreEqual (false, sock.Connected, "BeginAcceptSocket #3");
			Assert.AreEqual (true, conn.Connected, "BeginAcceptSocket #4");
			Assert.AreEqual (send_bytes.Length, BADByteCount, "BeginAcceptSocket #5");
			Assert.AreEqual (AddressFamily.InterNetwork, acc.AddressFamily, "BeginAcceptSocket #6");
			Assert.AreEqual (SocketType.Stream, acc.SocketType, "BeginAcceptSocket #7");
			Assert.AreEqual (ProtocolType.Tcp, acc.ProtocolType, "BeginAcceptSocket #8");
			Assert.AreEqual (conn.LocalEndPoint, acc.RemoteEndPoint, "BeginAcceptSocket #9");
			
			/* The MS runtime gives the returned data in a
			 * much bigger array.  TODO: investigate
			 * whether it the size correlates to the first
			 * parameter in BeginAccept()
			 */
			Assert.IsFalse (BADBytes.Length == send_bytes.Length,
					"BeginAcceptSocket #10");

			for(int i = 0; i < send_bytes.Length; i++) {
				Assert.AreEqual (send_bytes[i], BADBytes[i], "BeginAcceptSocket #" + (i+11).ToString ());
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
			
			Assert.AreEqual (true, BCConnected, "BeginConnectAddressPort #1");
			
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
			
			Assert.AreEqual (true, BCConnected, "BeginConnectMultiple #1");
			Assert.AreEqual (AddressFamily.InterNetwork, sock.RemoteEndPoint.AddressFamily, "BeginConnectMultiple #2");
			IPEndPoint remep = (IPEndPoint)sock.RemoteEndPoint;
			
			Assert.AreEqual (IPAddress.Loopback, remep.Address, "BeginConnectMultiple #2");
			
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
			
			Assert.AreEqual (true, sock.Connected, "BeginDisconnect #1");
			
			sock.Shutdown (SocketShutdown.Both);

			BDCalledBack.Reset ();
			BDDisconnected = false;
			
			sock.BeginDisconnect (false, BDCallback, sock);
		
			if (BDCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginDisconnect wait timed out");
			}
			
			Assert.AreEqual (true, BDDisconnected, "BeginDisconnect #2");
			Assert.AreEqual (false, sock.Connected, "BeginDisconnect #3");
			
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
				Assert.AreEqual (10057, ex.ErrorCode, "BeginSendNotConnected #2");
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
				Assert.AreEqual (10022, ex.ErrorCode, "BindTwice #2");
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

			Assert.AreEqual (true, sock.Connected, "Close #1");
			
			sock.Close (2);
			
			Thread.Sleep (3000);
			
			Assert.AreEqual (false, sock.Connected, "Close #2");
			
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
			
			Assert.AreEqual (true, sock.Connected, "ConnectAddressPort #1");
			
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
			
			Assert.AreEqual (true, sock.Connected, "ConnectMultiple #1");
			Assert.AreEqual (AddressFamily.InterNetwork, sock.RemoteEndPoint.AddressFamily, "ConnectMultiple #2");
			IPEndPoint remep = (IPEndPoint)sock.RemoteEndPoint;
			
			Assert.AreEqual (IPAddress.Loopback, remep.Address, "ConnectMultiple #2");
			
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
			
			Assert.AreEqual (true, sock.Connected, "Disconnect #1");
			
			sock.Shutdown (SocketShutdown.Both);

			sock.Disconnect (false);

			Assert.AreEqual (false, sock.Connected, "BeginDisconnect #3");
			
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
				Assert.AreEqual (10022, ex.ErrorCode, "ListenNotBound #2");
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
			Assert.AreEqual (0, readbyte, "ReceiveRemoteClosed #1");
			
			stream.Read (bytes, 0, 0);

			readbyte = stream.ReadByte ();
			Assert.AreEqual (0, readbyte, "ReceiveRemoteClosed #2");
			
			stream.Read (bytes, 0, 0);

			readbyte = stream.ReadByte ();
			Assert.AreEqual (-1, readbyte, "ReceiveRemoteClosed #3");

			sock.Close ();

			RRCLastRead = true;
			RRCReady.Set ();
		}

		[Test] // Receive (Byte [])
		public void Receive1_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (Byte [])
		public void Receive1_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((byte []) null);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (Byte [], SocketFlags)
		public void Receive2_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((byte []) null, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (Byte [], SocketFlags)
		public void Receive2_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((byte []) null, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (Byte [], Int32, SocketFlags)
		public void Receive3_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((byte []) null, 0, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (Byte [], Int32, SocketFlags)
		public void Receive3_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((byte []) null, 0, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (Byte [], Int32, Int32, SocketFlags)
		public void Receive4_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((byte []) null, 0, 0, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (Byte [], Int32, Int32, SocketFlags)
		public void Receive4_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((byte []) null, 0, 0, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

#if NET_2_0
		[Test] // Receive (Byte [], Int32, Int32, SocketFlags, out SocketError)
		public void Receive5_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			SocketError error;
			try {
				s.Receive ((byte []) null, 0, 0, SocketFlags.None, out error);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (Byte [], Int32, Int32, SocketFlags, out SocketError)
		public void Receive5_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			SocketError error;
			try {
				s.Receive ((byte []) null, 0, 0, SocketFlags.None, out error);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>)
		public void Receive6_Buffers_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((IList<ArraySegment<byte>>) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffers", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>)
		public void Receive6_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((IList<ArraySegment<byte>>) null);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>, SocketFlags)
		public void Receive7_Buffers_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			try {
				s.Receive ((IList<ArraySegment<byte>>) null, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffers", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>, SocketFlags)
		public void Receive7_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			try {
				s.Receive ((IList<ArraySegment<byte>>) null, (SocketFlags) 666);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>, SocketFlags, out SocketError)
		public void Receive8_Buffers_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			SocketError error;
			try {
				s.Receive ((IList<ArraySegment<byte>>) null, (SocketFlags) 666,
					out error);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffers", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // Receive (IList<ArraySegment<Byte>>, SocketFlags, out SocketError)
		public void Receive8_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			SocketError error;
			try {
				s.Receive ((IList<ArraySegment<byte>>) null, (SocketFlags) 666,
					out error);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			} finally {
				s.Close ();
			}
		}
#endif

		[Test] // ReceiveFrom (Byte [], ref EndPoint)
		public void ReceiveFrom1_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], ref EndPoint)
		public void ReceiveFrom1_RemoteEP_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			byte [] buffer = new byte [0];
			EndPoint remoteEP = null;
			try {
				s.ReceiveFrom (buffer, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("remoteEP", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], ref EndPoint)
		public void ReceiveFrom1_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // ReceiveFrom (Byte [], SocketFlags, ref EndPoint)
		public void ReceiveFrom2_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], SocketFlags, ref EndPoint)
		public void ReceiveFrom2_RemoteEP_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			byte [] buffer = new byte [5];
			EndPoint remoteEP = null;
			try {
				s.ReceiveFrom (buffer, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("remoteEP", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], SocketFlags, ref EndPoint)
		public void ReceiveFrom2_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, SocketFlags, ref EndPoint)
		public void ReceiveFrom3_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, -1, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, SocketFlags, ref EndPoint)
		public void ReceiveFrom3_RemoteEP_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);

			byte [] buffer = new byte [5];
			EndPoint remoteEP = null;
			try {
				s.ReceiveFrom (buffer, -1, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("remoteEP", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, SocketFlags, ref EndPoint)
		public void ReceiveFrom3_Size_OutOfRange ()
		{
			Socket s;
			byte [] buffer = new byte [5];
			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);

			// size negative
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, -1, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("size", ex.ParamName, "#A5");
			} finally {
				s.Close ();
			}

			// size > buffer length
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, (buffer.Length + 1), (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("size", ex.ParamName, "#B5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, SocketFlags, ref EndPoint)
		public void ReceiveFrom3_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom ((Byte []) null, -1, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, Int32, SocketFlags, EndPoint)
		public void ReceiveFrom4_Buffer_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);

			try {
				s.ReceiveFrom ((Byte []) null, -1, -1, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("buffer", ex.ParamName, "#5");
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, Int32, SocketFlags, EndPoint)
		public void ReceiveFrom4_Offset_OutOfRange ()
		{
			Socket s;
			byte [] buffer = new byte [5];
			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);

			// offset negative
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, -1, 0, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("offset", ex.ParamName, "#A5");
			} finally {
				s.Close ();
			}

			// offset > buffer length
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, (buffer.Length + 1), 0, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("offset", ex.ParamName, "#B5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, Int32, SocketFlags, ref IPEndPoint)
		public void ReceiveFrom4_RemoteEP_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			byte [] buffer = new byte [5];
			EndPoint remoteEP = null;

			try {
				s.ReceiveFrom (buffer, -1, -1, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("remoteEP", ex.ParamName, "#5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, Int32, SocketFlags, EndPoint)
		public void ReceiveFrom4_Size_OutOfRange ()
		{
			Socket s;
			byte [] buffer = new byte [5];
			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);

			// size negative
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, 0, -1, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#A1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("size", ex.ParamName, "#A5");
			} finally {
				s.Close ();
			}

			// size > buffer length
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, 0, (buffer.Length + 1), (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("size", ex.ParamName, "#B5");
			} finally {
				s.Close ();
			}

			// offset + size > buffer length
			s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
							ProtocolType.Tcp);
			try {
				s.ReceiveFrom (buffer, 2, 4, (SocketFlags) 666, ref remoteEP);
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				// Specified argument was out of the range of valid values
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("size", ex.ParamName, "#C5");
			} finally {
				s.Close ();
			}
		}

		[Test] // ReceiveFrom (Byte [], Int32, Int32, SocketFlags, ref EndPoint)
		public void ReceiveFrom4_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp);
			s.Close ();

			byte [] buffer = new byte [5];
			EndPoint remoteEP = new IPEndPoint (IPAddress.Loopback, 8001);
			try {
				s.ReceiveFrom (buffer, -1, -1, (SocketFlags) 666,
					ref remoteEP);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
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

		//
		// Test case for bug #471580
		[Test]
		public void UdpDoubleBind ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			
			s.Bind (new IPEndPoint (IPAddress.Any, 12345));
			
			Socket ss = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			ss.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			
			ss.Bind (new IPEndPoint (IPAddress.Any, 12345));

			// If we make it this far, we succeeded.
			
			ss.Close ();
			s.Close ();
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

		[Test] // GetSocketOption (SocketOptionLevel, SocketOptionName)
		public void GetSocketOption1_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.GetSocketOption (0, 0);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // GetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void GetSocketOption2_OptionValue_Null ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				s.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					(byte []) null);
				Assert.Fail ("#1");
				} catch (SocketException ex) {
					// The system detected an invalid pointer address in attempting
					// to use a pointer argument in a call
					Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
					Assert.AreEqual (10014, ex.ErrorCode, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual (10014, ex.NativeErrorCode, "#6");
#if NET_2_0
					Assert.AreEqual (SocketError.Fault, ex.SocketErrorCode, "#7");
#endif
				}
		}

		[Test] // GetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void GetSocketOption2_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.GetSocketOption (0, 0, (byte []) null);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // GetSocketOption (SocketOptionLevel, SocketOptionName, Int32)
		public void GetSocketOption3_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.GetSocketOption (0, 0, 0);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void SetSocketOption1_DontLinger ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger,
					new byte [] { 0x00 });
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger,
					new byte [] { 0x01 });
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void SetSocketOption1_DontLinger_Null ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.DontLinger, (byte []) null);
					Assert.Fail ("#1");
				} catch (SocketException ex) {
					// The system detected an invalid pointer address in attempting
					// to use a pointer argument in a call
					Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
					Assert.AreEqual (10014, ex.ErrorCode, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual (10014, ex.NativeErrorCode, "#6");
#if NET_2_0
					Assert.AreEqual (SocketError.Fault, ex.SocketErrorCode, "#7");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void SetSocketOption1_Linger_Null ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.DontLinger, (byte []) null);
					Assert.Fail ("#1");
				} catch (SocketException ex) {
					// The system detected an invalid pointer address in attempting
					// to use a pointer argument in a call
					Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
					Assert.AreEqual (10014, ex.ErrorCode, "#3");
					Assert.IsNull (ex.InnerException, "#4");
					Assert.IsNotNull (ex.Message, "#5");
					Assert.AreEqual (10014, ex.NativeErrorCode, "#6");
#if NET_2_0
					Assert.AreEqual (SocketError.Fault, ex.SocketErrorCode, "#7");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Byte [])
		public void SetSocketOption1_Socket_Close ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger,
					new byte [] { 0x00 });
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Int32)
		public void SetSocketOption2_DontLinger ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, 0);
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, 5);
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Int32)
		[Category ("NotWorking")]
		public void SetSocketOption2_Linger ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger, 0);
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger, 5);
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Int32)
		public void SetSocketOption2_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.DontLinger, 0);
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_AddMembershipIPv4_IPv6MulticastOption ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.Any, 1901));
				try {
					s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
						new IPv6MulticastOption (mcast_addr));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
					// The specified value is not a valid 'MulticastOption'
					Assert.IsTrue (ex.Message.IndexOf ("'MulticastOption'") != -1, "#5:" + ex.Message);
					Assert.AreEqual ("optionValue", ex.ParamName, "#6");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#5");
					Assert.IsNull (ex.ParamName, "#6");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_AddMembershipIPv4_MulticastOption ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.Any, 1901));
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					new MulticastOption (mcast_addr));
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		[Category ("NotWorking")]
		public void SetSocketOption3_AddMembershipIPv4_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			try {
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					new MulticastOption (mcast_addr));
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				s.Close ();
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_AddMembershipIPv6_IPv6MulticastOption ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.IPv6Any, 1902));
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					new IPv6MulticastOption (mcast_addr));
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_AddMembershipIPv6_MulticastOption ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.IPv6Any, 1902));
				try {
					s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
						new MulticastOption (mcast_addr));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
					// The specified value is not a valid 'IPv6MulticastOption'
					Assert.IsTrue (ex.Message.IndexOf ("'IPv6MulticastOption'") != -1, "#5:" + ex.Message);
					Assert.AreEqual ("optionValue", ex.ParamName, "#6");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#5");
					Assert.IsNull (ex.ParamName, "#6");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		[Category ("NotWorking")]
		public void SetSocketOption3_AddMembershipIPv6_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			try {
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					new IPv6MulticastOption (mcast_addr));
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				s.Close ();
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DontLinger_Boolean ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.DontLinger, (object) false);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The specified value is not valid
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DontLinger_Int32 ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.DontLinger, (object) 0);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// The specified value is not valid
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DontLinger_LingerOption ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.DontLinger, new LingerOption (true, 1000));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
					// The specified value is not valid
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_Linger_Boolean ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.Linger, (object) false);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
					// The specified value is not valid
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_Linger_Int32 ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.Linger, (object) 0);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
#if NET_2_0
					// The specified value is not valid
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#4");
					Assert.IsNull (ex.ParamName, "#5");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_Linger_LingerOption ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					new LingerOption (false, 0));
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					new LingerOption (true, 0));
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					new LingerOption (false, 1000));
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					new LingerOption (true, 1000));
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DropMembershipIPv4_IPv6MulticastOption ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.Any, 1901));
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					new MulticastOption (mcast_addr));
				try {
					s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
						new IPv6MulticastOption (mcast_addr));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
					// The specified value is not a valid 'MulticastOption'
					Assert.IsTrue (ex.Message.IndexOf ("'MulticastOption'") != -1, "#5:" + ex.Message);
					Assert.AreEqual ("optionValue", ex.ParamName, "#6");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#5");
					Assert.IsNull (ex.ParamName, "#6");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DropMembershipIPv4_MulticastOption ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
				MulticastOption option = new MulticastOption (mcast_addr);

				s.Bind (new IPEndPoint (IPAddress.Any, 1901));
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					option);
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
					option);
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		[Category ("NotWorking")]
		public void SetSocketOption3_DropMembershipIPv4_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("239.255.255.250");

			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			try {
				s.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
					new MulticastOption (mcast_addr));
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				s.Close ();
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DropMembershipIPv6_IPv6MulticastOption ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			using (Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
				IPv6MulticastOption option = new IPv6MulticastOption (
					IPAddress.Parse ("ff02::1"));

				s.Bind (new IPEndPoint (IPAddress.IPv6Any, 1902));
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					option);
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
					option);
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_DropMembershipIPv6_MulticastOption ()
		{
#if NET_2_0
			if (!Socket.OSSupportsIPv6)
#else
			if (!Socket.SupportsIPv6)
#endif
				Assert.Ignore ("IPv6 not enabled.");

			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			using (Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
				s.Bind (new IPEndPoint (IPAddress.IPv6Any, 1902));
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					new IPv6MulticastOption (mcast_addr));
				try {
					s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
						new MulticastOption (mcast_addr));
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
#if NET_2_0
					// The specified value is not a valid 'IPv6MulticastOption'
					Assert.IsTrue (ex.Message.IndexOf ("'IPv6MulticastOption'") != -1, "#5:" + ex.Message);
					Assert.AreEqual ("optionValue", ex.ParamName, "#6");
#else
					Assert.AreEqual ("optionValue", ex.Message, "#5");
					Assert.IsNull (ex.ParamName, "#6");
#endif
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		[Category ("NotWorking")]
		public void SetSocketOption3_DropMembershipIPv6_Socket_NotBound ()
		{
			IPAddress mcast_addr = IPAddress.Parse ("ff02::1");

			Socket s = new Socket (AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			try {
				s.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
					new IPv6MulticastOption (mcast_addr));
				Assert.Fail ("#1");
			} catch (SocketException ex) {
				// An invalid argument was supplied
				Assert.AreEqual (typeof (SocketException), ex.GetType (), "#2");
				Assert.AreEqual (10022, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (10022, ex.NativeErrorCode, "#6");
#if NET_2_0
				Assert.AreEqual (SocketError.InvalidArgument, ex.SocketErrorCode, "#7");
#endif
			} finally {
				s.Close ();
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_OptionValue_Null ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				try {
					s.SetSocketOption (SocketOptionLevel.Socket,
						SocketOptionName.Linger, (object) null);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("optionValue", ex.ParamName, "#5");
				}
			}
		}

		[Test] // SetSocketOption (SocketOptionLevel, SocketOptionName, Object)
		public void SetSocketOption3_Socket_Closed ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			try {
				s.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger,
					new LingerOption (false, 0));
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a disposed object
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual (typeof (Socket).FullName, ex.ObjectName, "#5");
			}
		}

		[Test]
		public void Shutdown_NoConnect ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			s.Listen (1);
			try {
				s.Shutdown (SocketShutdown.Both);
				Assert.Fail ("#1");
			} catch (SocketException exc) {
				Assert.AreEqual (10057, exc.ErrorCode, "#2");
			} finally {
				s.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ReceiveAsync_Null ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.ReceiveAsync (null);
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ReceiveAsync_Default ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
				s.ReceiveAsync (saea);
			}
		}


		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ReceiveAsync_NullBuffer ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
				saea.SetBuffer (null, 0, 0);
				s.ReceiveAsync (null);
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReceiveAsync_ClosedSocket ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			s.ReceiveAsync (null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void SendAsync_Null ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				s.SendAsync (null);
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void SendAsync_Default ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
				s.SendAsync (saea);
			}
		}


		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void SendAsync_NullBuffer ()
		{
			using (Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				SocketAsyncEventArgs saea = new SocketAsyncEventArgs ();
				saea.SetBuffer (null, 0, 0);
				s.SendAsync (null);
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void SendAsync_ClosedSocket ()
		{
			Socket s = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Close ();
			s.SendAsync (null);
		}
	}
}

