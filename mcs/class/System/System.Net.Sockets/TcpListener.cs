// System.Net.Sockets.TcpListener.cs
//
// Author:
//    Phillip Pearson (pp@myelin.co.nz)
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
//

// NB: This is untested (probably buggy) code - take care using it

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
		
		private bool active;
		private Socket server;
		
		// constructor

		/// <summary>
		/// Some code that is shared between the constructors.
		/// </summary>
		private void Init (AddressFamily family)
		{
			active = false;
			server = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
		}
		
		/// <summary>
		/// Constructs a new TcpListener to listen on a specified port
		/// </summary>
		/// <param name="port">The port to listen on, e.g. 80 if you 
		/// are a web server</param>
		public TcpListener (int port)
		{
			Init(AddressFamily.InterNetwork);
			server.Bind(new IPEndPoint(IPAddress.Any, port));
		}

		/// <summary>
		/// Constructs a new TcpListener with a specified local endpoint
		/// </summary>
		/// <param name="local_end_point">The endpoint</param>
		public TcpListener (IPEndPoint local_end_point)
		{
			Init(local_end_point.AddressFamily);
			server.Bind(local_end_point);
		}
		
		/// <summary>
		/// Constructs a new TcpListener, listening on a specified port
		/// and IP (for use on a multi-homed machine)
		/// </summary>
		/// <param name="listen_ip">The IP to listen on</param>
		/// <param name="port">The port to listen on</param>
		public TcpListener (IPAddress listen_ip, int port)
		{
			Init(listen_ip.AddressFamily);
			server.Bind(new IPEndPoint(listen_ip, port));
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
			get { return server.LocalEndPoint; }
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
		/// <returns>A Socket object for the new connection</returns>
		public Socket AcceptSocket ()
		{
			return server.Accept();
		}
		
		/// <summary>
		/// Accepts a pending connection
		/// </summary>
		/// <returns>A TcpClient
		/// object made from the new socket.</returns>
		public TcpClient AcceptTcpClient ()
		{
			TcpClient client = new TcpClient();
			// use internal method SetTcpClient to make a
			// client with the specified socket
			client.SetTcpClient(AcceptSocket());
			return client;
		}
		
		/// <summary>
		/// Destructor - stops the listener listening
		/// </summary>
		~TcpListener ()
		{
			if (active == true) {
				Stop();
			}
		}
	
		/// <returns>
		/// Returns 'true' if there is a connection waiting to be accepted
		/// with AcceptSocket() or AcceptTcpClient().
		/// </returns>
		public bool Pending ()
		{
			return server.Poll(1000, SelectMode.SelectRead);
		}
		
		/// <summary>
		/// Tells the TcpListener to start listening.
		/// </summary>
		[MonoTODO]
		public void Start ()
		{
			server.Listen(5);	// According to the
						// man page some BSD
						// and BSD-derived
						// systems limit the
						// backlog to 5.  This
						// should really be
						// configurable though
			active = true;
		}
		
		/// <summary>
		/// Tells the TcpListener to stop listening and dispose
		/// of all managed resources.
		/// </summary>
		public void Stop ()
		{
			server.Close();
		}

	}
}
