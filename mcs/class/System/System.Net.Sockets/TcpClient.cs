// System.Net.Sockets.TcpClient.cs
//
// Author:
//    Phillip Pearson (pp@myelin.co.nz)
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
//

// NB: This is untested (probably buggy) code - take care if using it

using System;
using System.Net;

namespace System.Net.Sockets
{
	/// <remarks>
	/// A slightly more abstracted way to create an
	/// outgoing network connections than a Socket.
	/// </remarks>
	public class TcpClient : IDisposable
	{
		// private data
		
		private NetworkStream stream;
		private bool active;
		private Socket client;
		private bool disposed = false;
		
		// constructor

		/// <summary>
		/// Some code that is shared between the constructors.
		/// </summary>
		private void Init ()
		{
			active = false;
			client = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Constructs a new TcpClient with no connection set up
		/// </summary>
		public TcpClient ()
		{
			Init();
			client.Bind(new IPEndPoint(IPAddress.Any, 0));
		}
	
		/// <summary>
		/// Constructs a new TcpClient with a specified local endpoint.
		/// Use this if you want to have your connections originating
		/// from a certain port, or a certain IP (on a multi homed
		/// system).
		/// </summary>
		/// <param name="local_end_point">The aforementioned local endpoint</param>
		public TcpClient (IPEndPoint local_end_point)
		{
			Init();
			client.Bind(local_end_point);
		}
		
		/// <summary>
		/// Constructs a new TcpClient and connects to a specified
		/// host on a specified port.  A quick way to set up a network
		/// connection.
		/// </summary>
		/// <param name="hostname">The host to connect to, e.g.
		/// 192.168.0.201 or www.myelin.co.nz</param>
		/// <param name="port">The port to connect to, e.g. 80 for HTTP</param>
		public TcpClient (string hostname, int port)
		{
			Init();
			client.Bind(new IPEndPoint(IPAddress.Any, 0));
			Connect(hostname, port);
		}
				
		/// <summary>
		/// A flag that is 'true' if the TcpClient has an active connection
		/// </summary>
		protected bool Active
		{
			get { return active; }
			set { active = value; }
		}
		
		/// <summary>
		/// The socket that all network comms passes through
		/// </summary>
		protected Socket Client
		{
			get { return client; }
			set {
				client = value;
				stream = null;
			}
		}

		/// <summary>
		/// Internal function to allow TcpListener.AcceptTcpClient
		/// to work (it needs to be able to set protected property
		/// 'Client')
		/// </summary>
		/// <param name="s"></param>
		internal void SetTcpClient (Socket s) 
		{
			Client = s;
		}
		
		/// <summary>
		/// If set, the socket will remain open after it has been
		/// instructed to close, in order to send data that remains
		/// in the buffer.
		/// </summary>
		public LingerOption LingerState
		{
			get {
				return (LingerOption)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.Linger);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.Linger, value);
			}
		}
				
		/// <summary>
		/// <p>If set, outbound data will be sent at once rather than collected
		/// until enough is available to fill a packet.</p>
		/// 
		/// <p>This is the TCP_NODELAY sockopt from BSD sockets and WinSock.
		/// For more information, look up the Nagle algorithm.</p>
		/// </summary>
		public bool NoDelay
		{
			get {
				return (bool)client.GetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay, value);
			}
		}
				
		/// <summary>
		/// How big the receive buffer is (from the connection socket)
		/// </summary>
		public int ReceiveBufferSize
		{
			get {
				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer, value);
			}
		}
			
		/// <summary>
		/// How long before the socket will time out on a 
		/// Receive() call
		/// </summary>
		public int ReceiveTimeout
		{
			get {
				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout, value);
			}
		}
		
		/// <summary>
		/// How big the send buffer is (from the connection socket)
		/// </summary>
		public int SendBufferSize
		{
			get {
				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer, value);
			}
		}
		
		/// <summary>
		/// How long before the socket will time out on a
		/// Send() call
		/// </summary>
		public int SendTimeout
		{
			get {
				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout);
			}
			set {
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout, value);
			}
		}
		
		
		// methods
		
		/// <summary>
		/// Closes the socket and disposes of all managed resources.
		/// 
		/// Throws SocketException if something goes wrong while
		/// closing the socket.
		/// </summary>
		public void Close ()
		{
			((IDisposable) this).Dispose ();
		}
		
		/// <summary>
		/// Connects to a specified remote endpoint
		/// 
		/// Throws SocketException if something goes wrong while
		/// connecting.
		/// </summary>
		/// <param name="remote_end_point">The aforementioned endpoint</param>
		public void Connect (IPEndPoint remote_end_point)
		{
			try {
				client.Connect(remote_end_point);
				stream = new NetworkStream(client, true);
				active = true;
			} finally {
				CheckDisposed ();
			}
		}
		
		/// <summary>
		/// Connects to an IP address on a port
		/// 
		/// Throws SocketException if something goes wrong while
		/// connecting.
		/// </summary>
		/// <param name="address">The IP address (get it from Dns.GetHostByName)</param>
		/// <param name="port">The port to connect to, e.g. 80 for HTTP</param>
		public void Connect (IPAddress address, int port)
		{
			Connect(new IPEndPoint(address, port));
		}
		
		/// <summary>
		/// Resolves a fully qualified domain name to an IP address
		/// and connects to it on a specified port
		/// 
		/// Throws SocketException if something goes wrong while
		/// connecting.
		/// </summary>
		/// <param name="hostname">The hostname, e.g. www.myelin.co.nz</param>
		/// <param name="port">The port, e.g. 80 for HTTP</param>
		[MonoTODO]
		public void Connect (string hostname, int port)
		{
			CheckDisposed ();
			IPHostEntry host = Dns.GetHostByName(hostname);
			/* TODO: This will connect to the first IP address returned
			from GetHostByName.  Is that right? */
			Connect(new IPEndPoint(host.AddressList[0], port));
		}
		
		/// <summary>
		/// Gets rid of all managed resources
		/// </summary>
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Gets rid of all unmanaged resources
		/// </summary>
		/// <param name="disposing">If this is true, it gets rid of all
		/// managed resources as well</param>
		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
				
			// release unmanaged resources
			NetworkStream s = stream;
			stream = null;
			if (s != null) {
				// This closes the socket as well, as the NetworkStream
				// owns the socket.
				s.Close();
				active = false;
				s = null;
			}
			client = null;
		}
		
		/// <summary>
		/// Destructor - just calls Dispose()
		/// </summary>
		~TcpClient ()
		{
			Dispose (false);
		}
		
		/// <returns>A NetworkStream object connected to the
		/// connection socket</returns>
		public NetworkStream GetStream()
		{
			try {
				if (stream == null)
				{
					stream = new NetworkStream (client, true);
				}
				return stream;
			}
			finally { CheckDisposed (); }
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
	}
}
