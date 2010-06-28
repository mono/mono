//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace System.Net {

	/// <remarks>
	///   Encapsulates an IP Address.
	/// </remarks>
	[Serializable]
	public class IPAddress {
		// Don't change the name of this field without also
		// changing socket-io.c in the runtime
		// The IP address is stored in little-endian order inside the int, 
		// meaning the lower order bytes contain the netid
		private long m_Address;
		private AddressFamily m_Family;
		private ushort[] m_Numbers;	/// ip6 Stored in network order (as ip4)
		private long m_ScopeId;

		public static readonly IPAddress Any = new IPAddress(0);
		public static readonly IPAddress Broadcast = IPAddress.Parse ("255.255.255.255");
		public static readonly IPAddress Loopback = IPAddress.Parse ("127.0.0.1");
		public static readonly IPAddress None = IPAddress.Parse ("255.255.255.255");
		public static readonly IPAddress IPv6Any = IPAddress.ParseIPV6 ("::");
		public static readonly IPAddress IPv6Loopback = IPAddress.ParseIPV6 ("::1");
		public static readonly IPAddress IPv6None = IPAddress.ParseIPV6 ("::");

		private static short SwapShort (short number)
		{
			return (short) ( ((number >> 8) & 0xFF) | ((number << 8) & 0xFF00) );
		}

		private static int SwapInt (int number)
		{
			return (((number >> 24) & 0xFF)
				  | ((number >> 08) & 0xFF00)
				  | ((number << 08) & 0xFF0000)
				  | ((number << 24)));
		}

		private static long SwapLong(long number)
		{
			return (((number >> 56) & 0xFF)
				  | ((number >> 40) & 0xFF00)
				  | ((number >> 24) & 0xFF0000)
				  | ((number >> 08) & 0xFF000000)
				  | ((number << 08) & 0xFF00000000)
				  | ((number << 24) & 0xFF0000000000)
				  | ((number << 40) & 0xFF000000000000)
				  | ((number << 56)));
		}

		public static short HostToNetworkOrder(short host) {
			if (!BitConverter.IsLittleEndian)
				return(host);

			return SwapShort (host);
		}

		public static int HostToNetworkOrder(int host) {
			if (!BitConverter.IsLittleEndian)
				return(host);

			return SwapInt (host);
		}
		
		public static long HostToNetworkOrder(long host) {
			if (!BitConverter.IsLittleEndian)
				return(host);

			return SwapLong (host);
		}

		public static short NetworkToHostOrder(short network) {
			if (!BitConverter.IsLittleEndian)
				return(network);

			return SwapShort (network);
		}

		public static int NetworkToHostOrder(int network) {
			if (!BitConverter.IsLittleEndian)
				return(network);

			return SwapInt (network);
		}

		public static long NetworkToHostOrder(long network) {
			if (!BitConverter.IsLittleEndian)
				return(network);

			return SwapLong (network);
		}
		
		/// <summary>
		///   Constructor from a 32-bit constant with the address bytes in
		///   little-endian order (the lower order bytes contain the netid)
		/// </summary>
		public IPAddress (long addr)
		{
			m_Address = addr;
			m_Family = AddressFamily.InterNetwork;
		}

		public IPAddress (byte[] address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			int len = address.Length;

#if NET_2_0
			if (len != 16 && len != 4)
				throw new ArgumentException ("An invalid IP address was specified.",
					"address");
#else
			if (len != 16)
				throw new ArgumentException ("address");
#endif

			if (len == 16) {
				m_Numbers = new ushort [8];
				Buffer.BlockCopy(address, 0, m_Numbers, 0, 16);
				m_Family = AddressFamily.InterNetworkV6;
				m_ScopeId = 0;
			} else {
				m_Address = ((uint) address [3] << 24) + (address [2] << 16) +
					(address [1] << 8) + address [0];
				m_Family = AddressFamily.InterNetwork;
			}
		}

		public IPAddress(byte[] address, long scopeId)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			if (address.Length != 16)
#if NET_2_0
				throw new ArgumentException ("An invalid IP address was specified.",
					"address");
#else
				throw new ArgumentException("address");
#endif

			m_Numbers = new ushort [8];
			Buffer.BlockCopy(address, 0, m_Numbers, 0, 16);
			m_Family = AddressFamily.InterNetworkV6;
			m_ScopeId = scopeId;
		}

		internal IPAddress(ushort[] address, long scopeId)
		{
			m_Numbers = address;

			for(int i=0; i<8; i++)
				m_Numbers[i] = (ushort)HostToNetworkOrder((short)m_Numbers[i]);

			m_Family = AddressFamily.InterNetworkV6;
			m_ScopeId = scopeId;
		}

		public static IPAddress Parse (string ipString)
		{
			IPAddress ret;
			if (TryParse (ipString, out ret))
				return ret;
			throw new FormatException ("An invalid IP address was specified.");
		}

#if NET_2_0
		public
#else
		internal
#endif
		static bool TryParse (string ipString, out IPAddress address)
		{
			if (ipString == null)
				throw new ArgumentNullException ("ipString");

			if ((address = ParseIPV4 (ipString)) == null)
				if ((address = ParseIPV6 (ipString)) == null)
					return false;
			return true;
		}

		private static IPAddress ParseIPV4 (string ip)
		{
#if ONLY_1_1
			if (ip.Length == 0 || ip == " ")
				return new IPAddress (0);
#endif

			int pos = ip.IndexOf (' ');
			if (pos != -1) {
#if NET_2_0
				string [] nets = ip.Substring (pos + 1).Split (new char [] {'.'});
				if (nets.Length > 0) {
					string lastNet = nets [nets.Length - 1];
					if (lastNet.Length == 0)
						return null;
#if NET_2_1 //workaround for smcs, as it generate code that can't access string.GetEnumerator ()
					foreach (char c in lastNet.ToCharArray ())
#else
					foreach (char c in lastNet)
#endif
						if (!Uri.IsHexDigit (c))
							return null;
				}
#endif
				ip = ip.Substring (0, pos);
			}

			if (ip.Length == 0 || ip [ip.Length - 1] == '.')
				return null;

			string [] ips = ip.Split (new char [] {'.'});
			if (ips.Length > 4)
				return null;
			
			// Make the number in network order
			try {
				long a = 0;
				long val = 0;
				for (int i = 0; i < ips.Length; i++) {
					string subnet = ips [i];
					if ((3 <= subnet.Length && subnet.Length <= 4) &&
					    (subnet [0] == '0') && (subnet [1] == 'x' || subnet [1] == 'X')) {
						if (subnet.Length == 3)
							val = (byte) Uri.FromHex (subnet [2]);
						else 
							val = (byte) ((Uri.FromHex (subnet [2]) << 4) | Uri.FromHex (subnet [3]));
					} else if (subnet.Length == 0)
						return null;
					else if (subnet [0] == '0') {
						// octal
						val = 0;
						for (int j = 1; j < subnet.Length; j++) {
							if ('0' <= subnet [j] && subnet [j] <= '7')
								val = (val << 3) + subnet [j] - '0';
							else
								return null;
						}
					}
					else {
#if NET_2_0
						if (!Int64.TryParse (subnet, NumberStyles.None, null, out val))
							return null;
#else
						val = long.Parse (subnet, NumberStyles.None);
#endif
					}

					if (val > 0xFF)
						return null; // e.g. 256.0.0.1
					if (i == (ips.Length - 1)) 
						i = 3;
					for (int j = 0; val > 0; j++, val /= 0x100)
						a |= (val & 0xFF) << ((i - j) << 3);
				}

				return (new IPAddress (a));
			} catch (Exception) {
				return null;
			}
		}
		
		private static IPAddress ParseIPV6 (string ip)
		{
			IPv6Address newIPv6Address;

			if (IPv6Address.TryParse(ip, out newIPv6Address))
				return  new IPAddress (newIPv6Address.Address, newIPv6Address.ScopeId);
			return null;
		}

		[Obsolete("This property is obsolete. Use GetAddressBytes.")]
		public long Address 
		{
			get {
				if(m_Family != AddressFamily.InterNetwork)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				return m_Address;
			}
			set {
				/* no need to do this test, ms.net accepts any value.
				if (value < 0 || value > 0x00000000FFFFFFFF)
					throw new ArgumentOutOfRangeException (
						"the address must be between 0 and 0xFFFFFFFF");
				*/

				if(m_Family != AddressFamily.InterNetwork)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				m_Address = value;
			}
		}

		internal long InternalIPv4Address {
			get { return m_Address; }
		}

#if NET_2_0
		public bool IsIPv6LinkLocal {
			get {
				if (m_Family == AddressFamily.InterNetwork)
					return false;
				int v = NetworkToHostOrder ((short) m_Numbers [0]) & 0xFFF0;
				return 0xFE80 <= v && v < 0xFEC0;
			}
		}

		public bool IsIPv6SiteLocal {
			get {
				if (m_Family == AddressFamily.InterNetwork)
					return false;
				int v = NetworkToHostOrder ((short) m_Numbers [0]) & 0xFFF0;
				return 0xFEC0 <= v && v < 0xFF00;
			}
		}

		public bool IsIPv6Multicast {
			get {
				return m_Family != AddressFamily.InterNetwork &&
					((ushort) NetworkToHostOrder ((short) m_Numbers [0]) & 0xFF00) == 0xFF00;
			}
		}
#endif

		public long ScopeId {
			get {
				if(m_Family != AddressFamily.InterNetworkV6)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				return m_ScopeId;
			}
			set {
				if(m_Family != AddressFamily.InterNetworkV6)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				m_ScopeId = value;
			}
		}

		public byte [] GetAddressBytes () 
		{
			if(m_Family == AddressFamily.InterNetworkV6) {
				byte [] addressBytes = new byte [16];
				Buffer.BlockCopy (m_Numbers, 0, addressBytes, 0, 16);
				return addressBytes;
			} else {
				return new byte [4] { (byte)(m_Address & 0xFF),
						     (byte)((m_Address >> 8) & 0xFF),
						     (byte)((m_Address >> 16) & 0xFF),
						     (byte)(m_Address >> 24) }; 
			}
		}

		public AddressFamily AddressFamily 
		{
			get {
				return m_Family;
			}
		}
		
		
		/// <summary>
		///   Used to tell whether an address is a loopback.
		///   All IP addresses of the form 127.X.Y.Z, where X, Y, and Z are in 
		///   the range 0-255, are loopback addresses.
		/// </summary>
		/// <param name="addr">Address to compare</param>
		/// <returns></returns>
		public static bool IsLoopback (IPAddress addr)
		{
			if(addr.m_Family == AddressFamily.InterNetwork)
				return (addr.m_Address & 0xFF) == 127;
			else {
				for(int i=0; i<6; i++) {
					if(addr.m_Numbers[i] != 0)
						return false;
				}

				return NetworkToHostOrder((short)addr.m_Numbers[7]) == 1;
			}
		}

		/// <summary>
		///   Overrides System.Object.ToString to return
		///   this object rendered in a quad-dotted notation
		/// </summary>
		public override string ToString ()
		{
			if(m_Family == AddressFamily.InterNetwork)
				return ToString (m_Address);
			else
			{
				ushort[] numbers = m_Numbers.Clone() as ushort[];

				for(int i=0; i<numbers.Length; i++)
					numbers[i] = (ushort)NetworkToHostOrder((short)numbers[i]);

				IPv6Address v6 = new IPv6Address(numbers);
				v6.ScopeId = ScopeId;
				return v6.ToString();
			}
		}

		/// <summary>
		///   Returns this object rendered in a quad-dotted notation
		/// </summary>
		static string ToString (long addr)
		{
			// addr is in network order
			return  (addr & 0xff).ToString () + "." +
				((addr >> 8) & 0xff).ToString () + "." +
				((addr >> 16) & 0xff).ToString () + "." +
				((addr >> 24) & 0xff).ToString ();
		}

		/// <returns>
		///   Whether both objects are equal.
		/// </returns>
		public override bool Equals (object other)
		{
			IPAddress otherAddr = other as IPAddress;
			if (otherAddr != null){
				if(AddressFamily != otherAddr.AddressFamily)
					return false;

				if(AddressFamily == AddressFamily.InterNetwork) {
					return m_Address == otherAddr.m_Address;
				} else {
					ushort[] vals = otherAddr.m_Numbers;

					for(int i=0; i<8; i++)
						if(m_Numbers[i] != vals[i])
							return false;

					return true;
				}
			}
			return false;
		}

		public override int GetHashCode ()
		{
			if(m_Family == AddressFamily.InterNetwork)
				return (int)m_Address;
			else
				return Hash (((((int) m_Numbers[0]) << 16) + m_Numbers [1]), 
					((((int) m_Numbers [2]) << 16) + m_Numbers [3]),
					((((int) m_Numbers [4]) << 16) + m_Numbers [5]),
					((((int) m_Numbers [6]) << 16) + m_Numbers [7]));
		}

		private static int Hash (int i, int j, int k, int l) 
		{
			return i ^ (j << 13 | j >> 19) ^ (k << 26 | k >> 6) ^ (l << 7 | l >> 25);
		}

#pragma warning disable 169
		// Added for serialization compatibility with MS.NET
		private int m_HashCode;
#pragma warning restore
		
	}
}
