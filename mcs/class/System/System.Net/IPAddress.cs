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

		public static readonly IPAddress Any = new IPAddress(0);
		public static readonly IPAddress Broadcast = IPAddress.Parse ("255.255.255.255");
		public static readonly IPAddress Loopback = IPAddress.Parse ("127.0.0.1");
		public static readonly IPAddress None = IPAddress.Parse ("255.255.255.255");

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
			Address = addr;
		}

		public static IPAddress Parse (string ip)
		{
			if (ip == null)
				throw new ArgumentNullException ("null ip string");
				
			if (ip.Length == 0 || ip [0] == ' ')
				return new IPAddress (0);
				
			int pos = ip.IndexOf (' ');
			if (pos != -1)
				ip = ip.Substring (0, pos);				
			else if (ip [ip.Length - 1] == '.')
				throw new FormatException ("An invalid IP address was specified");

			string [] ips = ip.Split (new char [] {'.'});
			if (ips.Length > 4)
				throw new FormatException ("An invalid IP address was specified");
			
			// Make the number in network order
			try {
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
				throw new FormatException ("An invalid IP address was specified");
			}
		}
		
		public long Address {
			get {
				return address;
			}
			set {
				/* no need to do this test, ms.net accepts any value.
				if (value < 0 || value > 0x00000000FFFFFFFF)
					throw new ArgumentOutOfRangeException (
						"the address must be between 0 and 0xFFFFFFFF");
				*/

				address = value;
			}
		}

		public AddressFamily AddressFamily {
			get {
				return(AddressFamily.InterNetwork);
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
			return (addr.address & 0xFF) == 127;
		}

		/// <summary>
		///   Overrides System.Object.ToString to return
		///   this object rendered in a quad-dotted notation
		/// </summary>
		public override string ToString ()
		{
			return ToString (address);
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
				return Address == ((System.Net.IPAddress) other).Address;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return (int)Address;
		}
	}
}
