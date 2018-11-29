// System.Net.Sockets.TcpListenerTest.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Martin Willemoes Hansen (mwh@sysrq.dk)
//    Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2001 Phillip Pearson (http://www.myelin.co.nz)
// (C) Copyright 2003 Martin Willemoes Hansen (mwh@sysrq.dk)
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class TcpListenerTest
	{
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void TcpListener ()
		{
			// listen with a new listener (IPv4 is the default)
			TcpListener inListener = NetworkHelpers.CreateAndStartTcpListener (out int port);
			

			// connect to it from a new socket
			IPHostEntry hostent = Dns.GetHostByAddress (IPAddress.Loopback);
			Socket outSock = null;

			foreach (IPAddress address in hostent.AddressList) {
				if (address.AddressFamily == AddressFamily.InterNetwork) {
					/// Only keep IPv4 addresses, our Server is in IPv4 only mode.
					outSock = new Socket (address.AddressFamily, SocketType.Stream,
						ProtocolType.IP);
					IPEndPoint remote = new IPEndPoint (address, port);
					outSock.Connect (remote);
					break;
				}
			}
			
			// There is no guarantee that the connecting socket will be in the listener's
			//  accept queue yet (though it is highly likely on Linux). We wait up to one
			//  second for the connecting socket to enter the listener's accept queue.
			Assert.IsTrue (inListener.Server.Poll (1000, SelectMode.SelectRead));
			Assert.IsTrue (inListener.Pending ());
			Socket inSock = inListener.AcceptSocket ();

			// now send some data and see if it comes out the other end
			const int len = 1024;
			byte[] outBuf = new Byte [len];
			for (int i=0; i<len; i++) 
				outBuf [i] = (byte) (i % 256);

			outSock.Send (outBuf, 0, len, 0);

			byte[] inBuf = new Byte[len];
			int ret = inSock.Receive (inBuf, 0, len, 0);


			// let's see if it arrived OK
			Assert.IsTrue (ret != 0);
			for (int i=0; i<len; i++) 
				Assert.IsTrue (inBuf[i] == outBuf [i]);

			// tidy up after ourselves
			inSock.Close ();

			inListener.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CtorInt1 ()
		{
			int nex = 0;
			try { new TcpListener (-1); } catch { nex++; }
			new TcpListener (0);
			new TcpListener (65535);
			try { new TcpListener (65536); } catch { nex++; }
			try { new TcpListener (100000); } catch { nex++; }
			Assert.IsTrue (nex == 3);			
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void CtorIPEndPoint ()
		{
			new TcpListener (null);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void CtorIPAddressInt1 ()
		{
			new TcpListener (null, 100000);
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#else
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void CtorIPAddressInt2 ()
		{
			new TcpListener (IPAddress.Any, 100000);
		}

		class MyListener : TcpListener
		{
			public MyListener ()
				: base (IPAddress.Loopback, 0)
			{
			}

			public Socket GetSocket ()
			{
				return Server;
			}

			public bool IsActive {
				get { return Active; }
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PreStartStatus ()
		{
			MyListener listener = new MyListener ();
			Assert.AreEqual (false, listener.IsActive, "#01");
			Assert.IsTrue (null != listener.GetSocket (), "#02");
			try {
				listener.AcceptSocket ();
				Assert.Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			try {
				listener.AcceptTcpClient ();
				Assert.Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			try {
				listener.Pending ();
				Assert.Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			listener.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void PostStartStatus ()
		{
			MyListener listener = new MyListener ();
			listener.Start ();
			Assert.AreEqual (true, listener.IsActive, "#01");
			Assert.IsTrue (null != listener.GetSocket (), "#02");
			
			Socket sock = listener.GetSocket ();
			listener.Start (); // Start called twice
			Assert.AreEqual (true, listener.IsActive, "#03");
			Assert.IsTrue (null != listener.GetSocket (), "#04");

			Assert.AreEqual (false, listener.Pending (), "#05");

			listener.Stop ();
			Assert.AreEqual (false, listener.IsActive, "#06");
			Assert.IsTrue (null != listener.GetSocket (), "#07");
			Assert.IsTrue (sock != listener.GetSocket (), "#08");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void StartListenMoreThan5 ()
		{
			TcpListener listen = new TcpListener (IPAddress.Loopback, 0);

			listen.Start (6);
			listen.Stop ();
			
			listen.Start (256);
			listen.Stop ();
			
			listen.Start (1024);
			listen.Stop ();

			listen.Start (32768);
			listen.Stop ();
			
			listen.Start (65536);
			listen.Stop ();
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void EndAcceptTcpClient ()
		{
			var listenerSocket = NetworkHelpers.CreateAndStartTcpListener (IPAddress.Any, out int port);
			listenerSocket.BeginAcceptTcpClient (new AsyncCallback (l => {
				listenerSocket.EndAcceptTcpClient (l);
			}), null);


			using (var outClient = new TcpClient ("localhost", port)) {
				using (var stream = outClient.GetStream ()) {
					stream.WriteByte (3);
				}
			}
		}
	}
}
