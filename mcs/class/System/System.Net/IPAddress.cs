//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Net.Sockets;

namespace System.Net {

	/// <remarks>
	///   Encapsulates an IP Address.
	/// </remarks>
	[Serializable]
	public class IPAddress {
		// Don't change the name of this field without also
		// changing socket-io.c in the runtime
		private long address;

		public static readonly IPAddress Any=new IPAddress(0);
		public static readonly IPAddress Broadcast=new IPAddress(0xffffffff);
		public static readonly IPAddress Loopback=new IPAddress(0x7f000001);
		public static readonly IPAddress None=new IPAddress(0xffffffff);
		
		[MonoTODO("Figure out host endian")]
		public static short HostToNetworkOrder(short host) {
			return(host);
		}

		[MonoTODO("Figure out host endian")]
		public static int HostToNetworkOrder(int host) {
			return(host);
		}
		
		[MonoTODO("Figure out host endian")]
		public static long HostToNetworkOrder(long host) {
			return(host);
		}

		[MonoTODO("Figure out host endian")]
		public static short NetworkToHostOrder(short network) {
			return(network);
		}

		[MonoTODO("Figure out host endian")]
		public static int NetworkToHostOrder(int network) {
			return(network);
		}

		[MonoTODO("Figure out host endian")]
		public static long NetworkToHostOrder(long network) {
			return(network);
		}
		
		/// <summary>
		///   Constructor from a 32-bit constant.
		/// </summary>
		public IPAddress (long addr)
		{
			address = addr;
		}

		public static IPAddress Parse(string ip)
		{
			if(ip==null) {
				throw new ArgumentNullException("null ip string");
			}

			string[] ips = ip.Split (new char[] {'.'});
			int i;
			long a = 0;

			for (i = 0; i < ips.Length; i++)
				a = (a << 8) |  (UInt16.Parse(ips [i]));

			return(new IPAddress(a));
		}
		
		public long Address {
			get {
				return(address);
			}
			set {
				address=value;
			}
		}

		public AddressFamily AddressFamily {
			get {
				return(AddressFamily.InterNetwork);
			}
		}
		
		
		/// <summary>
		///   Used to tell whether an address is a loopback.
		/// </summary>
		/// <param name="addr">Address to compare</param>
		/// <returns></returns>
		public static bool IsLoopback (IPAddress addr)
		{
			return addr.Equals(Loopback);
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
			return  ((addr >> 24) & 0xff).ToString () + "." +
				((addr >> 16) & 0xff).ToString () + "." +
				((addr >> 8) & 0xff).ToString () + "." +
				(addr & 0xff).ToString ();
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
