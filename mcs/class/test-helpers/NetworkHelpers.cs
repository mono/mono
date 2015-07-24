using System;
using System.Net;
using System.Net.Sockets;

namespace MonoTests.Helpers {

	public static class NetworkHelpers
	{
		public static int FindFreePort ()
		{
			TcpListener l = new TcpListener(IPAddress.Loopback, 0);
			l.Start();
			int port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop();
			return port;
		}
		public static IPEndPoint LocalEphemeralEndPoint ()
		{
			return new IPEndPoint (IPAddress.Loopback, FindFreePort());
		}
	}
}
