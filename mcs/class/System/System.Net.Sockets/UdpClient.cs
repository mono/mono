//
// System.Net.Sockets.UdpClient.cs
//
// Author:
//    Gonzalo Paniagua Javier <gonzalo@ximian.com>
//
// Copyright (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Net;

namespace System.Net.Sockets
{
	public class UdpClient : IDisposable
	{
		private bool disposed = false;
		private bool active = false;
		private Socket socket;
		
#region Constructors
		public UdpClient ()
		{
			InitSocket (null);
		}

		public UdpClient (int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			IPEndPoint localEP = new IPEndPoint (IPAddress.Any, port);
			InitSocket (localEP);
		}

		public UdpClient (IPEndPoint localEP)
		{
			if (localEP == null)
				throw new ArgumentNullException ("localEP");

			InitSocket (localEP);
		}

		public UdpClient (string hostname, int port)
		{
			if (hostname == null)
				throw new ArgumentNullException ("hostname");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			InitSocket (null);
			Connect (hostname, port);
		}

		private void InitSocket (EndPoint localEP)
		{
			socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			if (localEP != null)
				socket.Bind (localEP);
		}

#endregion // Constructors
#region Public methods
#region Close
		public void Close ()
		{
			((IDisposable) this).Dispose ();	
		}
#endregion
#region Connect
		public void Connect (IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			socket.Connect (endPoint);
			active = true;
		}

		public void Connect (IPAddress addr, int port)
		{
			if (addr == null)
				throw new ArgumentNullException ("addr");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			Connect (new IPEndPoint (addr, port));
		}

		public void Connect (string hostname, int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			Connect (new IPEndPoint (Dns.Resolve (hostname).AddressList [0], port));
		}
#endregion
#region Multicast methods
		public void DropMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();
			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");

			socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
						new MulticastOption (multicastAddr));
		}

		public void JoinMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();
			socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
						new MulticastOption (multicastAddr));
		}

		public void JoinMulticastGroup (IPAddress multicastAddr, int timeToLive)
		{
			CheckDisposed ();
			JoinMulticastGroup (multicastAddr);
			if (timeToLive < 0 || timeToLive > 255)
				throw new ArgumentOutOfRangeException ("timeToLive");

			socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive,
						timeToLive);
		}
#endregion
#region Data I/O
		public byte [] Receive (ref IPEndPoint remoteEP)
		{
			CheckDisposed ();
			// Length of the array for receiving data??
			byte [] recBuffer;
			int available = socket.Available;
			if (available < 512)
				available = 512;

			recBuffer = new byte [available];
			EndPoint endPoint = new IPEndPoint (IPAddress.Any, 0);
			int dataRead = socket.ReceiveFrom (recBuffer, ref endPoint);
			if (dataRead < recBuffer.Length)
				recBuffer = CutArray (recBuffer, dataRead);

			remoteEP = (IPEndPoint) endPoint;
			return recBuffer;
		}

		public int Send (byte [] dgram, int bytes)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram");

			if (!active)
				throw new InvalidOperationException ("Operation not allowed on " + 
								     "non-connected sockets.");

			return socket.Send (dgram, 0, bytes, SocketFlags.None);
		}

		public int Send (byte [] dgram, int bytes, IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram is null");
			
			if (active) {
				if (endPoint != null)
					throw new InvalidOperationException ("Cannot send packets to an " +
									     "arbitrary host while connected.");

				return socket.Send (dgram, 0, bytes, SocketFlags.None);
			}
			
			return socket.SendTo (dgram, 0, bytes, SocketFlags.None, endPoint);
		}

		public int Send (byte [] dgram, int bytes, string hostname, int port)
		{
			return Send (dgram, bytes, 
				     new IPEndPoint (Dns.Resolve (hostname).AddressList [0], port));
		}

		private byte [] CutArray (byte [] orig, int length)
		{
			byte [] newArray = new byte [length];
			Buffer.BlockCopy (orig, 0, newArray, 0, length);

			return newArray;
		}
#endregion
#region Properties
		protected bool Active {
			get { return active; }
			set { active = value; }
		}

		protected Socket Client {
			get { return socket; }
			set { socket = value; }
		}
#endregion
#region Disposing
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			if (disposed) 
				return;
			disposed = true;
			if (disposing) {
				// release managed resources
			}			
			// release unmanaged resources
			Socket s = socket;
			socket = null;
			if (s != null)
				s.Close ();
		}
		
		~UdpClient ()
		{
			Dispose (false);
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
#endregion
#endregion
	}
}

