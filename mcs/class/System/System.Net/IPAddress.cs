//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// Note: the address is stored in host order

using System.Net.Sockets;
using System.Runtime.InteropServices;

using System;

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
			return b0 + (b1 << 8) + (b2 << 16) + (b3 << 24) + (b4 << 32) + (b5 << 40) + (b6 << 48) + (b7 << 56);
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

		public static IPAddress Parse(string ip)
		{
			if(ip == null)
				throw new ArgumentNullException("null ip string");

			int pos = 0;
			int ndots = 0;
			char current;
			bool prevDigit = false;

			while (pos < ip.Length) {
				current  = ip [pos++];
				if (Char.IsDigit (current))
					prevDigit = true;
				else
				if (current == '.') {
					// No more than 3 dots. Doesn't allow ending with a dot.
					if (++ndots > 3 || pos == ip.Length || prevDigit == false)
						throw new FormatException ("the string is not a valid ip");

					prevDigit = false;
				}
				else if (!Char.IsDigit (current)) {
					if (!Char.IsWhiteSpace (current))
						throw new FormatException ("the string is not a valid ip");

					// The same as MS does
					if (pos == 1) 
						return new IPAddress (0);

					break;
				}
			}

			if (ndots != 3)
				throw new FormatException ("the string is not a valid ip");


			int a = 0;
			string [] ips = ip.Split (new char [] {'.'});
			// Make the number in network order
			for (int i = ips.Length - 1; i >= 0; i--)
				a = (a << 8) |  (Byte.Parse(ips [i]));
			
			return (new IPAddress (a));
		}
		
		public long Address {
			get {
				return address;
			}
			set {
				// FIXME: Temporarily disabled as a workaround for bug #23547
				/*if (value < 0 || value > 0x00000000FFFFFFFF)
					throw new ArgumentOutOfRangeException (
						"the address must be between 0 and 0xFFFFFFFF");*/

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
			return (NetworkToHostOrder (addr.address) & 0xFF) == 127;
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
