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
		private AddressFamily m_Family = AddressFamily.InterNetwork;
		private ushort[] m_Numbers = new ushort[8];	/// ip6 Stored in network order (as ip4)
		private long m_ScopeId = 0;

		public static readonly IPAddress Any = new IPAddress(0);
		public static readonly IPAddress Broadcast = IPAddress.Parse ("255.255.255.255");
		public static readonly IPAddress Loopback = IPAddress.Parse ("127.0.0.1");
		public static readonly IPAddress None = IPAddress.Parse ("255.255.255.255");

#if NET_1_1
		public static readonly IPAddress IPv6Any = IPAddress.ParseIPV6 ("::");
		public static readonly IPAddress IPv6Loopback = IPAddress.ParseIPV6 ("::1");
		public static readonly IPAddress IPv6None = IPAddress.ParseIPV6 ("::");
#endif

		private static short SwapShort (short number)
		{
			return (short) ( ((number >> 8) & 0xFF) + ((number << 8) & 0xFF00) );
		}

		private static int SwapInt (int number)
		{
			byte b0 = (byte) ((number >> 24) & 0xFF);
			byte b1 = (byte) ((number >> 16) & 0xFF);
			byte b2 = (byte) ((number >> 8) & 0xFF);
			byte b3 = (byte) (number & 0xFF);
			return b0 + (b1 << 8) + (b2 << 16) + (b3 << 24);
		}

		private static long SwapLong (long number)
		{
			byte b0 = (byte) ((number >> 56) & 0xFF);
			byte b1 = (byte) ((number >> 48) & 0xFF);
			byte b2 = (byte) ((number >> 40) & 0xFF);
			byte b3 = (byte) ((number >> 32) & 0xFF);
			byte b4 = (byte) ((number >> 24) & 0xFF);
			byte b5 = (byte) ((number >> 16) & 0xFF);
			byte b6 = (byte) ((number >> 8) & 0xFF);
			byte b7 = (byte) (number & 0xFF);
			return (long) b0 + ((long) b1 << 8) + ((long) b2 << 16) + ((long) b3 << 24) + ((long) b4 << 32) + ((long) b5 << 40) + ((long) b6 << 48) + ((long) b7 << 56);
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
		}

#if NET_1_1
		public IPAddress (byte[] address)
		{
			int len = address.Length;
#if NET_2_0
			if (len != 16 && len != 4)
				throw new ArgumentException ("address");
#else
			if (len != 16)
				throw new ArgumentException ("address");
#endif
			if (len == 16) {
				Buffer.BlockCopy(address, 0, m_Numbers, 0, 16);
				m_Family = AddressFamily.InterNetworkV6;
				m_ScopeId = 0;
			} else {
				m_Address = (address [3] << 24) + (address [2] << 16) +
					(address [1] << 8) + address [0];
			}
		}

		public IPAddress(byte[] address, long scopeId)
		{
			if (address.Length != 16)
				throw new ArgumentException("address");

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
#endif

		public static IPAddress Parse (string ip)
		{
			IPAddress ret;
			if (TryParse (ip, out ret))
				return ret;
			throw new FormatException("An invalid IP address was specified.");
		}

#if NET_2_0
		public
#endif
		static bool TryParse (string ip, out IPAddress address)
		{
			if (ip == null)
				throw new ArgumentNullException ("Value cannot be null.");
				
#if NET_1_1
			if( (address = ParseIPV4(ip)) == null)
				if( (address = ParseIPV6(ip)) == null)
					return false;
#else
			if( (address = ParseIPV4(ip)) == null)
					return false;
#endif
			return true;
		}

		private static IPAddress ParseIPV4 (string ip)
		{
			if (ip.Length == 0 || ip == " ")
				return new IPAddress (0);
				
			int pos = ip.IndexOf (' ');
			if (pos != -1)
				ip = ip.Substring (0, pos);				

			if (ip.Length == 0 || ip [ip.Length - 1] == '.')
				return null;

			string [] ips = ip.Split (new char [] {'.'});
			if (ips.Length > 4)
				return null;
			
			// Make the number in network order
			try 
			{
				long a = 0;
				byte val = 0;
				for (int i = 0; i < ips.Length; i++) {
					string subnet = ips [i];
					if ((3 <= subnet.Length && subnet.Length <= 4) &&
					    (subnet [0] == '0') &&
					    (subnet [1] == 'x' || subnet [2] == 'X')) {
						if (subnet.Length == 3)
							val = (byte) Uri.FromHex (subnet [2]);
						else 
							val = (byte) ((Uri.FromHex (subnet [2]) << 4) | Uri.FromHex (subnet [3]));
					} else if (subnet.Length == 0)
						return null;
					else 
						val = Byte.Parse (subnet, NumberStyles.None);

					if (ips.Length < 4 && i == (ips.Length - 1)) 
						i = 3;

					a |= (long) val << (i << 3);
				}

				return (new IPAddress (a));
			} catch (Exception) {
				return null;
			}
		}
		
#if NET_1_1
		private static IPAddress ParseIPV6 (string ip)
		{
			try 
			{
				IPv6Address newIPv6Address = IPv6Address.Parse(ip);
				return new IPAddress(newIPv6Address.Address, newIPv6Address.ScopeId);
			}
			catch (Exception) {
				return null;
			}
		}

		[Obsolete("This property is obsolete. Use GetAddressBytes.")]
#endif
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
			get { return m_Family != AddressFamily.InterNetwork && (m_Numbers [0] &= 0xFE80) != 0; }
		}

		public bool IsIPv6SiteLocal {
			get { return m_Family != AddressFamily.InterNetwork && (m_Numbers [0] &= 0xFEC0) != 0; }
		}

		public bool IsIPv6Multicast {
			get { return m_Family != AddressFamily.InterNetwork && (m_Numbers [0] & 0xFF00) != 0; }
		}
#endif

#if NET_1_1
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
#endif
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

				return new IPv6Address(numbers).ToString();
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
			if (other is System.Net.IPAddress){
				IPAddress otherAddr = other as IPAddress;

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
