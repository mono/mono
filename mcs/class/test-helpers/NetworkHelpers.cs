using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MonoTests.Helpers {

	public static class NetworkHelpers
	{
		const int POOLSIZE = 50;
		static Queue<TcpListener> listenerPool = new Queue<TcpListener> (POOLSIZE);

		private static void PopulatePool ()
		{
			for (int i = 0; i < POOLSIZE; i++) {
				TcpListener l = new TcpListener (IPAddress.Loopback, 0);
				l.Start ();
				listenerPool.Enqueue (l);
			}
		}

		public static int FindFreePort ()
		{
			TcpListener listener;

			lock (listenerPool) {
				if (listenerPool.Count == 0)
					PopulatePool ();
				
				listener = listenerPool.Dequeue ();
			}

			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop ();
			return port;
		}

		public static IPEndPoint LocalEphemeralEndPoint ()
		{
			return new IPEndPoint (IPAddress.Loopback, FindFreePort());
		}
	}
}
