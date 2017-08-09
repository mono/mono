using System;
using System.Net;
using System.Net.Sockets;

namespace MonoTests.Helpers {

	public static class NetworkHelpers
	{
		static Random rndPort = new Random ();

		public static int FindFreePort ()
		{
			return LocalEphemeralEndPoint ().Port;
		}

		public static IPEndPoint LocalEphemeralEndPoint ()
		{
			while (true) {
				var ep = new IPEndPoint (IPAddress.Loopback, rndPort.Next (10000, 60000));
				var socket = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				try {
					socket.Bind (ep);
					socket.Close ();
					return ep;
				} catch (SocketException) { }
			}
		}
	}
}
