//
// System.Net.IPv6Address.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//
// Note I: This class is not defined in the specs of .Net
//
// Note II : The name of this class is perhaps unfortunate as it turns
//           out that in ms.net there's an internal class called
//           IPv6Address in namespace System.
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


using System;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net {

	/// <remarks>
	///   Encapsulates an IPv6 Address.
	///   See RFC 2373 for more info on IPv6 addresses.
	/// </remarks>
	[Serializable]
	internal class IPv6Address {
		private ushort [] address;
		private int prefixLength;
		private long scopeId = 0;

		public static readonly IPv6Address Loopback = IPv6Address.Parse ("::1");
		public static readonly IPv6Address Unspecified = IPv6Address.Parse ("::");

		public IPv6Address (ushort [] addr)
		{
			if (addr == null)
				throw new ArgumentNullException ("addr");	
			if (addr.Length != 8)	
				throw new ArgumentException ("addr");
			address = addr;			
		}
		
		public IPv6Address (ushort [] addr, int prefixLength) : this (addr)
		{
			if (prefixLength < 0 || prefixLength > 128)
				throw new ArgumentException ("prefixLength");
			this.prefixLength = prefixLength;
		}
	
		public IPv6Address (ushort [] addr, int prefixLength, int scopeId) : this (addr, prefixLength)
		{
			this.scopeId = scopeId;
		}

		public static IPv6Address Parse (string ipString)
		{
			if (ipString == null)
				throw new ArgumentNullException ("ipString");

			IPv6Address result;
			if (TryParse (ipString, out result))
				return result;
			throw new FormatException ("Not a valid IPv6 address");
		}

		static int Fill (ushort [] addr, string ipString)
		{
			int p = 0;
			int slot = 0;

			if (ipString.Length == 0)
				return 0;
			
			// Catch double uses of ::
			if (ipString.IndexOf ("::", StringComparison.Ordinal) != -1)
				return -1;

			for (int i = 0; i < ipString.Length; i++){
				char c = ipString [i];
				int n;

				if (c == ':'){
					// Trailing : is not allowed.
					if (i == ipString.Length-1)
						return -1;
					
					if (slot == 8)
						return -1;
					
					addr [slot++] = (ushort) p;
					p = 0;
					continue;
				} if ('0' <= c && c <= '9')
					n = (int) (c - '0');
				else if ('a' <= c && c <= 'f')
					n = (int) (c - 'a' + 10);
				else if ('A' <= c && c <= 'F')
					n = (int) (c - 'A' + 10);
				else 
					return -1;
				p = (p << 4) + n;
				if (p > UInt16.MaxValue)
					return -1;
			}

			if (slot == 8)
				return -1;
			
			addr [slot++] = (ushort) p;

			return slot;
		}

		static bool TryParse (string prefix, out int res)
		{
			return Int32.TryParse (prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out res);
		}
		
		public static bool TryParse (string ipString, out IPv6Address result)
		{
			result = null;
			if (ipString == null)
				return false;

			if (ipString.Length > 2 && 
			    ipString [0] == '[' && 
			    ipString [ipString.Length - 1] == ']')
				ipString = ipString.Substring (1, ipString.Length - 2);

			if (ipString.Length  < 2)
				return false;

			int prefixLen = 0;
			int scopeId = 0;
			int pos = ipString.LastIndexOf ('/');
			if (pos != -1) {
				string prefix = ipString.Substring (pos + 1);
				if (!TryParse (prefix , out prefixLen))
					prefixLen = -1;
				if (prefixLen < 0 || prefixLen > 128)
					return false;
				ipString = ipString.Substring (0, pos);
			} else {
				pos = ipString.LastIndexOf ('%');
				if (pos != -1) {
					string prefix = ipString.Substring (pos + 1);
					if (!TryParse (prefix, out scopeId))
						scopeId = 0;
					ipString = ipString.Substring (0, pos);
				}			
			}

			//
			// At this point the prefix/suffixes have been removed
			// and we only have to deal with the ipv4 or ipv6 addressed
			//
			ushort [] addr = new ushort [8];

			//
			// Is there an ipv4 address at the end?
			//
			bool ipv4 = false;
			int pos2 = ipString.LastIndexOf (':');
			if (pos2 == -1)
				return false;

			int slots = 0;
			if (pos2 < (ipString.Length - 1)) {
				string ipv4Str = ipString.Substring (pos2 + 1);
				if (ipv4Str.IndexOf ('.') != -1) {
					IPAddress ip;
					
					if (!IPAddress.TryParse (ipv4Str, out ip))
						return false;
					
					long a = ip.InternalIPv4Address;
					addr [6] = (ushort) (((int) (a & 0xff) << 8) + ((int) ((a >> 8) & 0xff)));
					addr [7] = (ushort) (((int) ((a >> 16) & 0xff) << 8) + ((int) ((a >> 24) & 0xff)));
					if (pos2 > 0 && ipString [pos2 - 1] == ':') 
						ipString = ipString.Substring (0, pos2 + 1);
					else
						ipString = ipString.Substring (0, pos2);
					ipv4 = true;
					slots = 2;
				}
			}	

			//
			// Only an ipv6 block remains, either:
			// "hexnumbers::hexnumbers", "hexnumbers::" or "hexnumbers"
			//
			int c = ipString.IndexOf ("::", StringComparison.Ordinal);
			if (c != -1){
				int right_slots = Fill (addr, ipString.Substring (c+2));
				if (right_slots == -1){
					return false;
				}

				if (right_slots + slots > 8){
					return false;
				}

				int d = 8-slots-right_slots;
				for (int i = right_slots; i > 0; i--){
					addr [i+d-1] = addr [i-1];
					addr [i-1] = 0;
				}
				
				int left_slots = Fill (addr, ipString.Substring (0, c));
				if (left_slots == -1)
					return false;

				if (left_slots + right_slots + slots > 7)
					return false;
			} else {
				if (Fill (addr, ipString) != 8-slots)
					return false;
			}

			// Now check the results in the ipv6-address range only
			bool ipv6 = false;
			for (int i = 0; i < slots; i++){
				if (addr [i] != 0 || i == 5 && addr [i] != 0xffff)
					ipv6 = true;
			}
			
			// check IPv4 validity
			if (ipv4 && !ipv6) {
				for (int i = 0; i < 5; i++) {
					if (addr [i] != 0)
						return false;
				}

				if (addr [5] != 0 && addr [5] != 0xffff)
					return false;
			}

			result = new IPv6Address (addr, prefixLen, scopeId);
			return true;
		}

		public ushort [] Address {
			get { return address; }
		}

		public int PrefixLength {
			get { return this.prefixLength; }
		}
		
		public long ScopeId {
			get {
				return scopeId;
			}
			set {
				scopeId = value;
			}
		}

		public ushort this [int index] {
			get { return address [index]; }
		}		

		public AddressFamily AddressFamily {
			get { return AddressFamily.InterNetworkV6; }
		}

		public static bool IsLoopback (IPv6Address addr)
		{
			if (addr.address [7] != 1)
				return false;

			int x = addr.address [6] >> 8;
			if (x != 0x7f && x != 0)
				return false;

			for (int i = 0; i < 4; i++) {
				if (addr.address [i] != 0)
					return false;
			}

			if (addr.address [5] != 0 && addr.address [5] != 0xffff)
				return false;

			return true;
		}

		private static ushort SwapUShort (ushort number)
		{
			return (ushort) ( ((number >> 8) & 0xFF) + ((number << 8) & 0xFF00) );
		}

		// Convert the address into a format expected by the IPAddress (long) ctor
		private int AsIPv4Int ()
		{
			return (SwapUShort (address [7]) << 16) + SwapUShort (address [6]);
		}			

		public bool IsIPv4Compatible ()
		{
			for (int i = 0; i < 6; i++) 
				if (address [i] != 0)
					return false;
			return (AsIPv4Int () > 1);
		}
		
		public bool IsIPv4Mapped ()
		{
			for (int i = 0; i < 5; i++) 
				if (address [i] != 0)
					return false;
			return address [5] == 0xffff;
		}
		
		/// <summary>
		///   Overrides System.Object.ToString to return
		///   this object rendered in a canonicalized notation
		/// </summary>
		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();


			if(IsIPv4Compatible() || IsIPv4Mapped())
			{
				s.Append("::");

				if(IsIPv4Mapped())
					s.Append("ffff:");

				s.Append(new IPAddress( AsIPv4Int ()).ToString ());

				return s.ToString ();
			}
			else
			{
				int bestChStart = -1; // Best chain start
				int bestChLen = 0; // Best chain length
				int currChLen = 0; // Current chain length

				// Looks for the longest zero chain
				for (int i=0; i<8; i++)
				{
					if (address[i] != 0)
					{
						if ((currChLen > bestChLen) 
							&& (currChLen > 1))
						{
							bestChLen = currChLen;
							bestChStart = i - currChLen;
						}
						currChLen = 0;
					}
					else
						currChLen++;
				}
				if ((currChLen > bestChLen) 
					&& (currChLen > 1))
				{
					bestChLen = currChLen;
					bestChStart = 8 - currChLen;
				}

				// makes the string
				if (bestChStart == 0)
					s.Append(":");
				for (int i=0; i<8; i++)
				{
					if (i == bestChStart)
					{
						s.Append (":");
						i += (bestChLen - 1);
						continue;
					}
					s.AppendFormat("{0:x}", address [i]);
					if (i < 7) s.Append (':');
				}
			}
			if (scopeId != 0)
				s.Append ('%').Append (scopeId);
			return s.ToString ();
		}

		public string ToString (bool fullLength)
		{
			if (!fullLength)
				return ToString ();

			StringBuilder sb = new StringBuilder ();
			for (int i=0; i < address.Length - 1; i++) {
				sb.AppendFormat ("{0:X4}:", address [i]);
			}
			sb.AppendFormat ("{0:X4}", address [address.Length - 1]);
			return sb.ToString ();
		}

		/// <returns>
		///   Whether both objects are equal.
		/// </returns>
		public override bool Equals (object other)
		{
			System.Net.IPv6Address ipv6 = other as System.Net.IPv6Address;
			if (ipv6 != null) {
				for (int i = 0; i < 8; i++) 
					if (this.address [i] != ipv6.address [i])
						return false;
				return true;
			}
			
			System.Net.IPAddress ipv4 = other as System.Net.IPAddress;
			if (ipv4 != null) {
				for (int i = 0; i < 5; i++) 
					if (address [i] != 0)
						return false;

				if (address [5] != 0 && address [5] != 0xffff)
					return false;

				long a = ipv4.InternalIPv4Address;
				if (address [6] != (ushort) (((int) (a & 0xff) << 8) + ((int) ((a >> 8) & 0xff))) ||
				    address [7] != (ushort) (((int) ((a >> 16) & 0xff) << 8) + ((int) ((a >> 24) & 0xff))))
					return false;

				return true;
			}
			
			return false;
		}

		public override int GetHashCode ()
		{
			return Hash (((((int) address [0]) << 16) + address [1]), 
						((((int) address [2]) << 16) + address [3]),
						((((int) address [4]) << 16) + address [5]),
						((((int) address [6]) << 16) + address [7]));
		}
		
		private static int Hash (int i, int j, int k, int l) 
		{
			return i ^ (j << 13 | j >> 19) ^ (k << 26 | k >> 6) ^ (l << 7 | l >> 25);
		}
	}
}
