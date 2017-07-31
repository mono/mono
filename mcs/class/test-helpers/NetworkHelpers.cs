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
			bool success = false;

			do {
				var ep = new IPEndPoint (IPAddress.Loopback, rndPort.Next (10000, 60000));
				var socket = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				try {
					socket.Bind (ep);
					socket.Close ();
					success = true;
				} catch (SocketException) { }

				return ep;
			} while (!success);
		}
	}
}
