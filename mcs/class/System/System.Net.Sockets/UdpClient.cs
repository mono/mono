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
		private IPEndPoint localEP;
		
#region Constructors
		public UdpClient ()
		{
			localEP = new IPEndPoint (IPAddress.Any, 0);
			InitSocket ();
		}

		public UdpClient (int port)
		{
			// IPEndPoint throws ArgumentException when port is invalid
			localEP = new IPEndPoint (IPAddress.Any, port);
			InitSocket ();
		}

		public UdpClient (IPEndPoint localEP)
		{
			if (localEP == null)
				throw new ArgumentNullException ("IPEndPoint cannot be null");

			this.localEP = localEP;
			InitSocket ();
		}

		public UdpClient (string hostname, int port)
		{
			if (hostname == null)
				throw new ArgumentNullException ("hostname cannot be null");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentException ("Invalid port");

			localEP = new IPEndPoint (IPAddress.Any, 0);
			InitSocket ();
			Connect (hostname, port);
		}

		private void InitSocket ()
		{
			active = false;
			socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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
			try {
				socket.Connect (endPoint);
				active = true;
			} finally {
				CheckDisposed ();
			}
		}

		public void Connect (IPAddress addr, int port)
		{
			Connect (new IPEndPoint (addr, port));
		}

		public void Connect (string hostname, int port)
		{
			Connect (new IPEndPoint (Dns.Resolve (hostname).AddressList [0], port));
		}
#endregion
#region Multicast methods
		public void DropMulticastGroup (IPAddress multicastAddr)
		{
			try {
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
							new MulticastOption (multicastAddr));
			} finally {
				CheckDisposed ();
			}
		}

		public void JoinMulticastGroup (IPAddress multicastAddr)
		{
			try {
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
							new MulticastOption (multicastAddr));
			} finally {
				CheckDisposed ();
			}
		}

		public void JoinMulticastGroup (IPAddress multicastAddr, int timeToLive)
		{
			JoinMulticastGroup (multicastAddr);
			try {
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive,
							timeToLive);
			} finally {
				CheckDisposed ();
			}
		}
#endregion
#region Data I/O
		public byte [] Receive (ref IPEndPoint remoteEP)
		{
			try {
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
			} finally {
				CheckDisposed ();
			}
		}

		public int Send (byte [] dgram, int bytes)
		{
			try {
				if (dgram == null)
					throw new ArgumentNullException ("dgram is null");

				byte [] realDgram;
				if (dgram.Length <= bytes)
					realDgram = dgram;
				else
					realDgram = CutArray (dgram, (bytes >= dgram.Length) ? bytes : dgram.Length);

				// the socket should be connected already, so I use Send instead of SendTo
				return socket.Send (realDgram);
			} finally {
				CheckDisposed ();
			}
		}

		public int Send (byte [] dgram, int bytes, IPEndPoint endPoint)
		{
			try {
				if (dgram == null)
					throw new ArgumentNullException ("dgram is null");

				byte [] realDgram;
				if (dgram.Length <= bytes)
					realDgram = dgram;
				else
					realDgram = CutArray (dgram, (bytes >= dgram.Length) ? bytes : dgram.Length);

				// the socket should not be connected
				return socket.SendTo (realDgram, endPoint);
			} finally {
				CheckDisposed ();
			}
		}

		public int Send (byte [] dgram, int bytes, string hostname, int port)
		{
			return Send (dgram, bytes, 
				     new IPEndPoint (Dns.Resolve (hostname).AddressList [0], port));
		}

		private byte [] CutArray (byte [] orig, int length)
		{
			byte [] newArray = new byte [length];
			Array.Copy (orig, 0, newArray, 0, length);

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

/* 
// commented because in the ms.net implementation these are not overriden. -- LP
#region Overrides
		public override bool Equals (object obj)
		{
			if (obj is UdpClient)
				return (((UdpClient) obj).socket  == socket &&
					((UdpClient) obj).localEP == localEP);

			return false;
		}

		public override int GetHashCode ()
		{
			return (socket.GetHashCode () + localEP.GetHashCode () + (active ? 1 : 0));
		}
#endregion
*/

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
				localEP = null;
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

