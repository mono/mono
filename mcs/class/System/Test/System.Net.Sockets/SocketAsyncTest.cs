using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketAsyncTest
	{
		Socket serverSocket;
		Socket clientSocket;
		ManualResetEvent readyEvent;
		ManualResetEvent mainEvent;
		Exception error;

		void SetUp ()
		{
			readyEvent = new ManualResetEvent (false);
			mainEvent = new ManualResetEvent (false);

			ThreadPool.QueueUserWorkItem (_ => DoWork ());
			readyEvent.WaitOne ();

			if (error != null)
				throw error;

			clientSocket = new Socket (
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			clientSocket.Connect (serverSocket.LocalEndPoint);
			clientSocket.NoDelay = true;
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			if (serverSocket != null)
				serverSocket.Close ();
			readyEvent.Close ();
			mainEvent.Close ();
		}

		void DoWork ()
		{
			try {
				serverSocket = new Socket (
					AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				serverSocket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
				serverSocket.Listen (1);

				var async = new SocketAsyncEventArgs ();
				async.Completed += (s,e) => OnAccepted (e);

				if (!serverSocket.AcceptAsync (async))
					OnAccepted (async);
			} catch (Exception e) {
				error = e;
			} finally {
				readyEvent.Set ();
			}
		}

		void OnAccepted (SocketAsyncEventArgs e)
		{
			var acceptSocket = e.AcceptSocket;

			try {
				var header = new byte [4];
				acceptSocket.Receive (header);
				if ((header [0] != 0x12) || (header [1] != 0x34) ||
				    (header [2] != 0x56) || (header [3] != 0x78))
					throw new InvalidOperationException ();
			} catch (Exception ex) {
				error = ex;
				return;
			}

			var recvAsync = new SocketAsyncEventArgs ();
			recvAsync.Completed += (sender, args) => OnReceived (args);
			recvAsync.SetBuffer (new byte [4], 0, 4);
			if (!acceptSocket.ReceiveAsync (recvAsync))
				OnReceived (recvAsync);

			mainEvent.Set ();
		}

		void OnReceived (SocketAsyncEventArgs e)
		{
			if (e.SocketError != SocketError.Success)
				error = new SocketException ((int) e.SocketError);
			else if (e.Buffer [0] != 0x9a)
				error = new InvalidOperationException ();

			mainEvent.Set ();
		}

		[Test]
		[Category("Test")]
		[Category("MultiThreaded")]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void SendAsync ()
		{
			SetUp ();
			var buffer = new byte [] { 0x12, 0x34, 0x56, 0x78 };
			var m = new ManualResetEvent (false);
			var e = new SocketAsyncEventArgs ();
			e.SetBuffer (buffer, 0, buffer.Length);
			e.Completed += (s,o) => {
				if (o.SocketError != SocketError.Success)
					error = new SocketException ((int)o.SocketError);
				m.Set ();
			};
			bool res = clientSocket.SendAsync (e);
			if (res) {
				if (!m.WaitOne (1500))
					Assert.Fail ("Timeout #1");
			}

			if (!mainEvent.WaitOne (1500))
				Assert.Fail ("Timeout #2");
			if (error != null)
				throw error;

			m.Reset ();
			mainEvent.Reset ();

			buffer [0] = 0x9a;
			buffer [1] = 0xbc;
			buffer [2] = 0xde;
			buffer [3] = 0xff;
			res = clientSocket.SendAsync (e);
			if (res) {
				if (!m.WaitOne (1500))
					Assert.Fail ("Timeout #3");
			}

			if (!mainEvent.WaitOne (1500))
				Assert.Fail ("Timeout #4");
			if (error != null)
				throw error;
		}
	}
}
