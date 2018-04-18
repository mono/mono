using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MonoTests.Helpers {

	public static class NetworkHelpers
	{
		static Random rndPort = new Random ();
		static HashSet<int> portsTable = new HashSet<int> ();

		public static int FindFreePort ()
		{
			return LocalEphemeralEndPoint ().Port;
		}

		public static IPEndPoint LocalEphemeralEndPoint ()
		{
			int counter = 0;

			while (counter < 1000) {
				var testingPort = rndPort.Next (10000, 60000);

				var ep = new IPEndPoint (IPAddress.Loopback, testingPort);

				lock (portsTable) {
					if (portsTable.Contains (testingPort))
						continue;

					++counter;

					try {
						using (var socket = new Socket (ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp)) {
							socket.Bind (ep);
							socket.Close ();
						}

						portsTable.Add (testingPort);
						return ep;
					} catch (SocketException) { }
				}
			}

			throw new ApplicationException ($"Could not find available local port after {counter} retries");
		}
	}
}
