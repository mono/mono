// System.Net.Sockets.TcpListener.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	  Patrik Torstensson
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2004 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;

namespace System.Net.Sockets
{
	/// <remarks>
	/// A slightly more abstracted way to listen for incoming
	/// network connections than a Socket.
	/// </remarks>
	public class TcpListener
	{
		// private data
		
		bool active;
		Socket server;
		EndPoint savedEP;
		
		// constructor

		/// <summary>
		/// Some code that is shared between the constructors.
		/// </summary>
		private void Init (AddressFamily family, EndPoint ep)
		{
			active = false;
			server = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
			savedEP = ep;
		}
		
		/// <summary>
		/// Constructs a new TcpListener to listen on a specified port
		/// </summary>
		/// <param name="port">The port to listen on, e.g. 80 if you 
		/// are a web server</param>
#if NET_1_1
		[Obsolete ("Use TcpListener (IPAddress address, int port) instead")]
#endif
		public TcpListener (int port)
		{
			if (port < 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port");

			Init (AddressFamily.InterNetwork, new IPEndPoint (IPAddress.Any, port));
		}

		/// <summary>
		/// Constructs a new TcpListener with a specified local endpoint
		/// </summary>
		/// <param name="local_end_point">The endpoint</param>
		public TcpListener (IPEndPoint local_end_point)
		{
			if (local_end_point == null)
				throw new ArgumentNullException ("local_end_point");

			Init (local_end_point.AddressFamily, local_end_point);
		}
		
		/// <summary>
		/// Constructs a new TcpListener, listening on a specified port
		/// and IP (for use on a multi-homed machine)
		/// </summary>
		/// <param name="listen_ip">The IP to listen on</param>
		/// <param name="port">The port to listen on</param>
		public TcpListener (IPAddress listen_ip, int port)
		{
			if (listen_ip == null)
				throw new ArgumentNullException ("listen_ip");

			if (port < 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port");

			Init (listen_ip.AddressFamily, new IPEndPoint(listen_ip, port));
		}


		// properties

		/// <summary>
		/// A flag that is 'true' if the TcpListener is listening,
		/// or 'false' if it is not listening
		/// </summary>
		protected bool Active
		{
			get { return active; }
		}

		/// <summary>
		/// The local end point
		/// </summary>
		public EndPoint LocalEndpoint
		{
			get { 
				if (active)
					return server.LocalEndPoint;

				return savedEP; 
			}
		}
		
		/// <summary>
		/// The listening socket
		/// </summary>
		protected Socket Server
		{
			get { return server; }
		}
		
		
		// methods

		/// <summary>
		/// Accepts a pending connection
		/// </summary>
		/// <returns>A Socket object for the new connection</returns>
		public Socket AcceptSocket ()
		{
			if (!active)
				throw new InvalidOperationException ("Socket is not listening");

			return server.Accept();
		}
		
		/// <summary>
		/// Accepts a pending connection
		/// </summary>
		/// <returns>A TcpClient
		/// object made from the new socket.</returns>
		public TcpClient AcceptTcpClient ()
		{
			if (!active)
				throw new InvalidOperationException ("Socket is not listening");

			Socket clientSocket = server.Accept ();

			TcpClient client = new TcpClient();
			// use internal method SetTcpClient to make a
			// client with the specified socket
			client.SetTcpClient (clientSocket);
			
			return client;
		}
		
		/// <summary>
		/// Destructor - stops the listener listening
		/// </summary>
		~TcpListener ()
		{
			if (active)
				Stop();
		}
	
		/// <returns>
		/// Returns 'true' if there is a connection waiting to be accepted
		/// with AcceptSocket() or AcceptTcpClient().
		/// </returns>
		public bool Pending ()
		{
			if (!active)
				throw new InvalidOperationException ("Socket is not listening");

			return server.Poll(0, SelectMode.SelectRead);
		}
		
		/// <summary>
		/// Tells the TcpListener to start listening.
		/// </summary>
		public void Start ()
		{
			if (active)
				return;

			if (server == null)
				throw new InvalidOperationException("Invalid server socket");

			server.Bind (savedEP);
			
			// MS: sets Listen to Int32.MaxValue
			server.Listen(5);	
			// According to the man page some BSD and BSD-derived
			// systems limit the backlog to 5.  This should really be
			// configurable though

			active = true;
		}
		
		/// <summary>
		/// Tells the TcpListener to stop listening and dispose
		/// of all managed resources.
		/// </summary>
		public void Stop ()
		{
			if (active) 
			{
				server.Close ();
				server = null;
			}

			Init (AddressFamily.InterNetwork, savedEP);
		}

	}
}
