using System.Threading;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class SocketAcceptAsyncTest
	{
		[Test]
		public void AcceptAsyncShouldUseAcceptSocketFromEventArgs()
		{
			var readyEvent = new ManualResetEvent(false);
			var mainEvent = new ManualResetEvent(false);
			var listenSocket = new Socket(
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			var serverSocket = new Socket(
					AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Socket acceptedSocket = null;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
				listenSocket.Listen(1);

				var asyncEventArgs = new SocketAsyncEventArgs {AcceptSocket = serverSocket};
				asyncEventArgs.Completed += (s, e) =>
				{
					acceptedSocket = e.AcceptSocket;
					mainEvent.Set();
				};

				readyEvent.Set();

				if (listenSocket.AcceptAsync(asyncEventArgs))
					return;
				acceptedSocket = asyncEventArgs.AcceptSocket;
				mainEvent.Set();
			});
			Assert.IsTrue(readyEvent.WaitOne(1500));

			var clientSocket = new Socket(
				AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			clientSocket.Connect(listenSocket.LocalEndPoint);
			clientSocket.NoDelay = true;

			Assert.IsTrue(mainEvent.WaitOne(1500));
			Assert.AreEqual(serverSocket, acceptedSocket);
			mainEvent.Reset();

			if (acceptedSocket != null)
				acceptedSocket.Close();

			listenSocket.Close();
			readyEvent.Close();
			mainEvent.Close();
		}
	}
}
