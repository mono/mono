//
// System.Net.IPEndPoint.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
				throw new ArgumentNullException ("address");

			Address = address;
			Port = port;
		}
		
		public IPEndPoint (long address, int port)
		{
			Address = new IPAddress (address);
			Port = port;
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
		public override EndPoint Create (SocketAddress socketAddress)
		{
			if (socketAddress == null)
				throw new ArgumentNullException ("socketAddress");

			if (socketAddress.Family != AddressFamily)
				throw new ArgumentException ("The IPEndPoint was created using " + AddressFamily + 
						" AddressFamily but SocketAddress contains " + socketAddress.Family + 
						" instead, please use the same type.");

			SocketAddress sockaddr = socketAddress;
			int size =sockaddr.Size;
			AddressFamily family = sockaddr.Family;
			int port;

			IPEndPoint ipe = null;
			switch(family)
			{
				case AddressFamily.InterNetwork:
					if (size < 8) {
						return(null);
					}
					
					port = (((int)sockaddr[2])<<8) + (int)sockaddr[3];
					long address=(((long)sockaddr[7])<<24) +
						(((long)sockaddr[6])<<16) +
						(((long)sockaddr[5])<<8) +
						(long)sockaddr[4];

					ipe = new IPEndPoint(address, port);
					break;
#if NET_1_1
				case AddressFamily.InterNetworkV6:
					if (size < 28) {
						return(null);
					}
					
					port	= (((int)sockaddr[2])<<8) + (int)sockaddr[3];

					/// maybe flowid ?
					/*
					int unknown	= (int)sockaddr[4] +
						(((int)sockaddr[5])<<8) +
						(((int)sockaddr[6])<<16) +
						(((int)sockaddr[7])<<24);
					*/

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
					long addr = address.InternalIPv4Address;
					sockaddr [4] = (byte) (addr & 0xff);
					sockaddr [5] = (byte) ((addr >> 8) & 0xff);
					sockaddr [6] = (byte) ((addr >> 16) & 0xff);
					sockaddr [7] = (byte) ((addr >> 24) & 0xff);
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

		public override bool Equals (object comparand)
		{
			IPEndPoint p = comparand as IPEndPoint;
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
