//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// Note: the address is stored in host order

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
		private long address;
		private AddressFamily _family = AddressFamily.InterNetwork;
		private ushort[] _numbers = new ushort[8];	/// ip6 Stored in network order (as ip4)
		private long _scopeId = 0;

		public static readonly IPAddress Any = new IPAddress(0);
		public static readonly IPAddress Broadcast = IPAddress.Parse ("255.255.255.255");
		public static readonly IPAddress Loopback = IPAddress.Parse ("127.0.0.1");
		public static readonly IPAddress None = IPAddress.Parse ("255.255.255.255");

#if NET_1_1
		public static readonly IPAddress IPv6Any = IPAddress.Parse ("::");
		public static readonly IPAddress IPv6Loopback = IPAddress.Parse ("::1");
		public static readonly IPAddress IPv6None = IPAddress.Parse ("::");
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
		///   Constructor from a 32-bit constant with its bytes 
		///   in network order.
		/// </summary>
		public IPAddress (long addr)
		{
			address = addr;
		}

#if NET_1_1
		public IPAddress(byte[] address) : this(address, 0)
		{
		}

		public IPAddress(byte[] address, long scopeId)
		{
			if(address.Length != 16)
				throw new ArgumentException("address");

			Buffer.BlockCopy(address, 0, _numbers, 0, 16);
			_family = AddressFamily.InterNetworkV6;
			_scopeId = scopeId;
		}

		internal IPAddress(ushort[] address, long scopeId)
		{
			_numbers = address;

			for(int i=0; i<8; i++)
				_numbers[i] = (ushort)HostToNetworkOrder((short)_numbers[i]);

			_family = AddressFamily.InterNetworkV6;
			_scopeId = scopeId;
		}
#endif

		public static IPAddress Parse (string ip)
		{
			IPAddress ret;

			if (ip == null)
				throw new ArgumentNullException ("Value cannot be null.");
				
#if NET_1_1
			if( (ret = ParseIPV4(ip)) == null)
				if( (ret = ParseIPV6(ip)) == null)
					throw new FormatException("An invalid IP address was specified.");
#else
			if( (ret = ParseIPV4(ip)) == null)
					throw new FormatException("An invalid IP address was specified.");
#endif
			return ret;
		}

		private static IPAddress ParseIPV4 (string ip)
		{
			if (ip.Length == 0 || ip [0] == ' ')
				return new IPAddress (0);
				
			int pos = ip.IndexOf (' ');
			if (pos != -1)
				ip = ip.Substring (0, pos);				
			else if (ip [ip.Length - 1] == '.')
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
						val = 0;
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

		[Obsolete]
#endif
		public long Address 
		{
			get {
				if(_family != AddressFamily.InterNetwork)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				return address;
			}
			set {
				/* no need to do this test, ms.net accepts any value.
				if (value < 0 || value > 0x00000000FFFFFFFF)
					throw new ArgumentOutOfRangeException (
						"the address must be between 0 and 0xFFFFFFFF");
				*/

				if(_family != AddressFamily.InterNetwork)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				address = value;
			}
		}

#if NET_1_1
		public long ScopeId {
			get {
				if(_family != AddressFamily.InterNetworkV6)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				return _scopeId;
			}
			set {
				if(_family != AddressFamily.InterNetworkV6)
					throw new Exception("The attempted operation is not supported for the type of object referenced");

				_scopeId = value;
			}
		}
		public byte[] GetAddressBytes() 
		{

			if(_family == AddressFamily.InterNetworkV6)
			{
				byte[] addressBytes = new byte[16];
				Buffer.BlockCopy(_numbers, 0, addressBytes, 0, 16);
				return addressBytes;
			}
			else
			{
				return new byte[4] { (byte)(address & 0xFF),
									   (byte)((address >> 8) & 0xFF),
									   (byte)((address >> 16) & 0xFF),
									   (byte)(address >> 24) }; 
			}
		}
#endif

		public AddressFamily AddressFamily 
		{
			get {
				return _family;
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
			if(addr._family == AddressFamily.InterNetwork)
				return (addr.address & 0xFF) == 127;
			else {
				for(int i=0; i<6; i++) {
					if(addr._numbers[i] != 0)
						return false;
				}

				return NetworkToHostOrder((short)addr._numbers[7]) == 1;
			}
		}

		/// <summary>
		///   Overrides System.Object.ToString to return
		///   this object rendered in a quad-dotted notation
		/// </summary>
		public override string ToString ()
		{
			if(_family == AddressFamily.InterNetwork)
				return ToString (address);
			else
			{
				ushort[] numbers = _numbers.Clone() as ushort[];

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

				if(AddressFamily == AddressFamily.InterNetwork)
					return Address == otherAddr.Address;
				else
				{
					ushort[] vals = otherAddr._numbers;

					for(int i=0; i<8; i++)
						if(_numbers[i] != vals[i])
							return false;

					return true;
				}
			}
			return false;
		}

		public override int GetHashCode ()
		{
			if(_family == AddressFamily.InterNetwork)
				return (int)Address;
			else
				return Hash (((((int) _numbers[0]) << 16) + _numbers [1]), 
					((((int) _numbers [2]) << 16) + _numbers [3]),
					((((int) _numbers [4]) << 16) + _numbers [5]),
					((((int) _numbers [6]) << 16) + _numbers [7]));
		}

		private static int Hash (int i, int j, int k, int l) 
		{
			return i ^ (j << 13 | j >> 19) ^ (k << 26 | k >> 6) ^ (l << 7 | l >> 25);
		}
	}
}
