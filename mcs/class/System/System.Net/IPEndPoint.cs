//
// System.Net.IPEndPoint.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Net.Sockets;

namespace System.Net {
	[Serializable]
	public class IPEndPoint : EndPoint {

		private IPAddress address;
		private int port;

		public const int MaxPort = 65535;
		public const int MinPort = 0;
		
		public IPEndPoint (IPAddress address, int port)
		{
			if (address == null)
				throw new ArgumentNullException ("Value cannot be null");

			Address = address;
			Port = port;
		}
		
		public IPEndPoint (long iaddr, int port) : this (new IPAddress (iaddr), port)
		{
		}

		public IPAddress Address {
			get {
				return (address);
			}
			set {
				address=value;
			}
		}

		public override AddressFamily AddressFamily {
			get {
				return AddressFamily.InterNetwork;
			}
		}

		public int Port {
			get {
				return port;
			}
			set {
				// LAMESPEC: no mention of sanity checking
				// PS: MS controls the range when setting the value
				if (value < MinPort || value > MaxPort)
					throw new ArgumentOutOfRangeException ("Invalid port");
			
				port = value;
			}
		}

		// bytes 2 and 3 store the port, the rest
		// stores the address
		public override EndPoint Create(SocketAddress sockaddr) {
			int size=sockaddr.Size;
			
			// LAMESPEC: no mention of what to do if
			// sockaddr is bogus
			if(size<8) {
				// absolute minimum amount needed for
				// an address family, buffer size,
				// port and address
				return(null);
			}
			AddressFamily family=(AddressFamily)sockaddr[0];
			if(family!=AddressFamily.InterNetwork) {
				return(null);
			}
			
			int port=(((int)sockaddr[2])<<8) + (int)sockaddr[3];
			long address=(((long)sockaddr[7])<<24) +
				(((long)sockaddr[6])<<16) +
				(((long)sockaddr[5])<<8) +
				(long)sockaddr[4];

			IPEndPoint ipe = new IPEndPoint(address, port);
			
			return(ipe);
		}

		public override SocketAddress Serialize() {
			// .net produces a 16 byte buffer, even though
			// only 8 bytes are used. I guess its just a
			// holdover from struct sockaddr padding.
			SocketAddress sockaddr = new SocketAddress(AddressFamily.InterNetwork, 16);

			// bytes 2 and 3 store the port, the rest
			// stores the address
			sockaddr [2] = (byte) ((port>>8) & 0xff);
			sockaddr [3] = (byte) (port & 0xff);

			sockaddr [4] = (byte) (address.Address & 0xff);
			sockaddr [5] = (byte) ((address.Address >> 8) & 0xff);
			sockaddr [6] = (byte) ((address.Address >> 16) & 0xff);
			sockaddr [7] = (byte) ((address.Address >> 24) & 0xff);
			
			return(sockaddr);
		}

		public override string ToString() {
			return(address.ToString() + ":" + port);
		}

		public override bool Equals (Object obj)
		{
			IPEndPoint p = obj as IPEndPoint;
			return p != null && 
			       p.port == port && 
			       p.address.Equals (address);
		}

		public override int GetHashCode ()
		{
			return address.GetHashCode () + port;
		}
	}
}
