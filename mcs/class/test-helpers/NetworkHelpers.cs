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

		// Bind to the specified address using a system-assigned port.
		// Returns the assigned port.
		public static void Bind (this Socket socket, IPAddress address, out int port)
		{
			socket.Bind (new IPEndPoint (address, 0));
			port = ((IPEndPoint) socket.LocalEndPoint).Port;
		}


		// Bind to the specified address using a system-assigned port.
		// Returns the resulting end local end point.
		public static void Bind (this Socket socket, IPAddress address, out IPEndPoint ep)
		{
			socket.Bind (new IPEndPoint (address, 0));
			ep = (IPEndPoint) socket.LocalEndPoint;
		}

		// Creates and starts a TcpListener using a system-assigned port.
		// Returns the assigned port.
		public static TcpListener StartTcpListener (out int port)
		{
			var rv = new TcpListener (0);
			rv.Start ();
			port = ((IPEndPoint) rv.LocalEndpoint).Port;
			return rv;
		}

		// Creates and starts a TcpListener using a system-assigned port.
		// Returns the resulting local end point.
		public static TcpListener StartTcpListener (out IPEndPoint ep)
		{
			var rv = new TcpListener (0);
			rv.Start ();
			ep = (IPEndPoint) rv.LocalEndpoint;
			return rv;
		}

		// Creates and starts a TcpListener using the specified address and a system-assigned port.
		// Returns the assigned port.
		public static TcpListener StartTcpListener (IPAddress address, out int port)
		{
			var rv = new TcpListener (address, 0);
			rv.Start ();
			port = ((IPEndPoint) rv.LocalEndpoint).Port;
			return rv;
		}

		// Creates and starts a TcpListener using the specified address and a system-assigned port.
		// Returns the resulting local end point.
		public static TcpListener StartTcpListener (IPAddress address, out IPEndPoint ep)
		{
			var rv = new TcpListener (address, 0);
			rv.Start ();
			ep = (IPEndPoint) rv.LocalEndpoint;
			return rv;
		}

		public static HttpListener CreateAndStartHttpListener (string host, int port, string path, AuthenticationSchemes? authSchemes = null, Action<HttpListener> initializer = null)
		{
			var prefix = host + port + path;
			HttpListener listener = new HttpListener ();
			if (initializer != null)
				initializer (listener);
			if (authSchemes.HasValue)
				listener.AuthenticationSchemes = authSchemes.Value;
			listener.Prefixes.Add (prefix);
			listener.Start ();
			return listener;
		}

		public static HttpListener CreateAndStartHttpListener (string host, out int port, string path, AuthenticationSchemes? authSchemes = null, Action<HttpListener> initializer = null)
		{
			// There's no way to create an HttpListener with a system-assigned port.
			// So we use NetworkHelpers.FindFreePort, and re-try if we fail because someone else has already used the port.
			for (int i = 0; i < 10; i++) {
				try {
					var tentativePort = NetworkHelpers.FindFreePort ();
					var listener = CreateAndStartHttpListener (host, tentativePort, path, authSchemes, initializer);
					port = tentativePort;
					return listener;
				} catch (SocketException se) {
					if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
						continue;
					throw;
				}
			}
			throw new Exception ("Unable to create HttpListener after 10 attempts");
		}

		public static HttpListener CreateAndStartHttpListener (string host, out int port, string path, out string uri, AuthenticationSchemes? authSchemes = null, Action<HttpListener> initializer = null)
		{
			var rv = CreateAndStartHttpListener (host, out port, path, authSchemes, initializer);
			uri = host + port + path;
			return rv;
		}

	}
}
