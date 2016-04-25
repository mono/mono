using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketAcceptAsyncTest
	{
		private Socket _listenSocket;
		private Socket _clientSocket;
		private Socket _serverSocket;
		private Socket _acceptedSocket;
		private ManualResetEvent _readyEvent;
		private ManualResetEvent _mainEvent;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_readyEvent = new ManualResetEvent(false);
			_mainEvent = new ManualResetEvent(false);

			ThreadPool.QueueUserWorkItem(_ => StartListen());
			if (!_readyEvent.WaitOne(1500))
				throw new TimeoutException();

			_clientSocket = new Socket(
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_clientSocket.Connect(_listenSocket.LocalEndPoint);
			_clientSocket.NoDelay = true;
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			if (_acceptedSocket != null)
				_acceptedSocket.Close();
			if (_listenSocket != null)
				_listenSocket.Close();
			_readyEvent.Close();
			_mainEvent.Close();
		}

		private void StartListen()
		{
			_listenSocket = new Socket(
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			_listenSocket.Listen(1);

			_serverSocket = new Socket(
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      var async = new SocketAsyncEventArgs();
			async.AcceptSocket = _serverSocket;
			async.Completed += (s, e) => OnAccepted(e);

			_readyEvent.Set();

			if (!_listenSocket.AcceptAsync(async))
				OnAccepted(async);
		}

		private void OnAccepted(SocketAsyncEventArgs e)
		{
			_acceptedSocket = e.AcceptSocket;
			_mainEvent.Set();
		}

		[Test]
		[Category("Test")]
		public void AcceptAsyncShouldUseAcceptSocketFromEventArgs()
		{
			if (!_mainEvent.WaitOne(1500))
				throw new TimeoutException();
			Assert.AreEqual(_serverSocket, _acceptedSocket);
			_mainEvent.Reset();
		}
	}
}
