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
				return address.AddressFamily;
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
			int port;

			IPEndPoint ipe = null;
			switch(family)
			{
				case AddressFamily.InterNetwork:
					port = (((int)sockaddr[2])<<8) + (int)sockaddr[3];
					long address=(((long)sockaddr[7])<<24) +
						(((long)sockaddr[6])<<16) +
						(((long)sockaddr[5])<<8) +
						(long)sockaddr[4];

					ipe = new IPEndPoint(address, port);
					break;
#if NET_1_1
				case AddressFamily.InterNetworkV6:
					port	= (((int)sockaddr[2])<<8) + (int)sockaddr[3];

					/// maybe flowid ?
					int unknown	= (int)sockaddr[4] +
						(((int)sockaddr[5])<<8) +
						(((int)sockaddr[6])<<16) +
						(((int)sockaddr[7])<<24);

					int scopeId	= (int)sockaddr[24] +
						(((int)sockaddr[25])<<8) +
						(((int)sockaddr[26])<<16) +
						(((int)sockaddr[27])<<24);

					ushort[] addressData = new ushort[8];
					for(int i=0; i<8; i++)
						addressData[i] = (ushort)((sockaddr[8+i*2] << 8) + sockaddr[8+i*2+1]);

					ipe = new IPEndPoint (new IPAddress(addressData, scopeId), port);
					break;
#endif
				default:
					return null;
			}

			return(ipe);
		}

		public override SocketAddress Serialize() {
			SocketAddress sockaddr = null;

			switch (address.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					// .net produces a 16 byte buffer, even though
					// only 8 bytes are used. I guess its just a
					// holdover from struct sockaddr padding.
					sockaddr = new SocketAddress(AddressFamily.InterNetwork, 16);

					// bytes 2 and 3 store the port, the rest
					// stores the address
					sockaddr [2] = (byte) ((port>>8) & 0xff);
					sockaddr [3] = (byte) (port & 0xff);

					sockaddr [4] = (byte) (address.Address & 0xff);
					sockaddr [5] = (byte) ((address.Address >> 8) & 0xff);
					sockaddr [6] = (byte) ((address.Address >> 16) & 0xff);
					sockaddr [7] = (byte) ((address.Address >> 24) & 0xff);
					break;
#if NET_1_1
				case AddressFamily.InterNetworkV6:
					sockaddr = new SocketAddress(AddressFamily.InterNetworkV6, 28);

					sockaddr [2] = (byte) ((port>>8) & 0xff);
					sockaddr [3] = (byte) (port & 0xff);

					byte[] addressBytes = address.GetAddressBytes();
					for(int i=0; i<16; i++)
						sockaddr[8+i] = addressBytes[i];
					
					sockaddr [24] = (byte) (address.ScopeId & 0xff);
					sockaddr [25] = (byte) ((address.ScopeId >> 8) & 0xff);
					sockaddr [26] = (byte) ((address.ScopeId >> 16) & 0xff);
					sockaddr [27] = (byte) ((address.ScopeId >> 24) & 0xff);
					break;
#endif
			}

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
