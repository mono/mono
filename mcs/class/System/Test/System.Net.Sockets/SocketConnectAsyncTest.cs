using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketConnectAsyncTest
	{
		Socket serverSocket;
		Socket clientSocket;
		SocketAsyncEventArgs clientSocketAsyncArgs;
		ManualResetEvent readyEvent;
		ManualResetEvent mainEvent;
		Exception error;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			readyEvent = new ManualResetEvent (false);
			mainEvent = new ManualResetEvent (false);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			readyEvent.Close ();
			mainEvent.Close ();
		}

		void StartServer()
		{
			readyEvent.Reset();
			mainEvent.Reset();
			ThreadPool.QueueUserWorkItem (_ => DoWork ());
			readyEvent.WaitOne ();
		}

		void StopServer()
		{
			if (serverSocket != null)
				serverSocket.Close ();
		}

		void DoWork ()
		{
			serverSocket = new Socket (
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			serverSocket.Listen (1);

			var async = new SocketAsyncEventArgs ();
			async.Completed += (s,e) => OnAccepted (e);

			readyEvent.Set ();

			if (!serverSocket.AcceptAsync (async))
				OnAccepted (async);
		}

		void OnAccepted (SocketAsyncEventArgs e)
		{
			var acceptSocket = e.AcceptSocket;
			mainEvent.Set ();
		}

		[Test]
		[Category("NotWorking")]
		public void Connect ()
		{
			StartServer();

			EndPoint serverEndpoint = serverSocket.LocalEndPoint;

			var m = new ManualResetEvent (false);
			var e = new SocketAsyncEventArgs ();

			clientSocket = new Socket (
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			clientSocketAsyncArgs = new SocketAsyncEventArgs();
			clientSocketAsyncArgs.RemoteEndPoint = serverEndpoint;
			clientSocketAsyncArgs.Completed += (s,o) => {
				if (o.SocketError != SocketError.Success)
					error = new SocketException ((int)o.SocketError);
				m.Set ();
			};
			bool res = clientSocket.ConnectAsync(clientSocketAsyncArgs);
			if (res) {
				if (!m.WaitOne (1500))
					throw new TimeoutException ();
			}

			if (!mainEvent.WaitOne (1500))
				throw new TimeoutException ();
			if (error != null)
				throw error;

			m.Reset ();
			mainEvent.Reset ();

			StopServer();

			// Try again to non-listening endpoint, expect error

			error = null;
			clientSocket = new Socket (
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			clientSocketAsyncArgs = new SocketAsyncEventArgs ();
			clientSocketAsyncArgs.RemoteEndPoint = serverEndpoint;
			clientSocketAsyncArgs.Completed += (s,o) => {
				if (o.SocketError != SocketError.Success)
					error = new SocketException ((int)o.SocketError);
				m.Set ();
			};
			res = clientSocket.ConnectAsync (clientSocketAsyncArgs);
			if (res) {
				if (!m.WaitOne (1500))
					throw new TimeoutException ();
			}

			Assert.IsTrue (error != null, "Connect - no error");
			SocketException socketException = (SocketException)error;
			Assert.IsTrue(socketException.ErrorCode == (int)SocketError.ConnectionRefused); 
	
			m.Reset ();
			mainEvent.Reset ();
		}

	}
}