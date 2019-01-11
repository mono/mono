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
		public static TcpListener CreateAndStartTcpListener (out int port)
		{
			var rv = new TcpListener (0);
			rv.Start ();
			port = ((IPEndPoint) rv.LocalEndpoint).Port;
			return rv;
		}

		// Creates and starts a TcpListener using a system-assigned port.
		// Returns the resulting local end point.
		public static TcpListener CreateAndStartTcpListener (out IPEndPoint ep)
		{
			var rv = new TcpListener (0);
			rv.Start ();
			ep = (IPEndPoint) rv.LocalEndpoint;
			return rv;
		}

		// Creates and starts a TcpListener using the specified address and a system-assigned port.
		// Returns the assigned port.
		public static TcpListener CreateAndStartTcpListener (IPAddress address, out int port)
		{
			var rv = new TcpListener (address, 0);
			rv.Start ();
			port = ((IPEndPoint) rv.LocalEndpoint).Port;
			return rv;
		}

		// Creates and starts a TcpListener using the specified address and a system-assigned port.
		// Returns the resulting local end point.
		public static TcpListener CreateAndStartTcpListener (IPAddress address, out IPEndPoint ep)
		{
			var rv = new TcpListener (address, 0);
			rv.Start ();
			ep = (IPEndPoint) rv.LocalEndpoint;
			return rv;
		}

		// Creates and starts an HttpListener using the specified host, port,
		// path and authSchemes.
		//
		// If specified, the initializer will be called immediately after the
		// HttpListener is created (typical usage would be to set/change
		// properties before starting the listener)
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

		// Creates and starts an HttpListener using the specified host, path
		// and authSchemes. The method will try to find an unused port, and
		// use that (multiple attempts with random port numbers will be made).
		//
		// If specified, the initializer will be called immediately after the
		// HttpListener is created (typical usage would be to set/change
		// properties before starting the listener). Be aware that the
		// initializer can be called multiple times (in case multiple creation
		// attempts have to be made).
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

		// Creates and starts an HttpListener using the specified host, path
		// and authSchemes. The method will try to find an unused port, and
		// use that (multiple attempts with random port numbers will be made).
		//
		// If specified, the initializer will be called immediately after the
		// HttpListener is created (typical usage would be to set/change
		// properties before starting the listener). Be aware that the
		// initializer can be called multiple times (in case multiple creation
		// attempts have to be made).
		//
		// The resulting uri will also be returned (this is just host + port + path).
		public static HttpListener CreateAndStartHttpListener (string host, out int port, string path, out string uri, AuthenticationSchemes? authSchemes = null, Action<HttpListener> initializer = null)
		{
			var rv = CreateAndStartHttpListener (host, out port, path, authSchemes, initializer);
			uri = host + port + path;
			return rv;
		}

	}
}
