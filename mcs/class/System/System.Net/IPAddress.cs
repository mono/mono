//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	/// <remarks>
	///   Encapsulates an IP Address.
	/// </remarks>
	public class IPAddress {
		public long Address;

		public static readonly IPAddress Any;
		public static readonly IPAddress Broadcast;
		public static readonly IPAddress Loopback;
		public static readonly IPAddress None;
		
		public const uint InaddrAny       = 0;
		public const uint InaddrBroadcast = 0xffffffff;
		public const uint InaddrLoopback  = 0x7f000001;
		public const uint InaddrNone      = 0xffffffff;

		static IPAddress ()
		{
			Any.Address = InaddrAny;
			Broadcast.Address = InaddrBroadcast;
			Loopback.Address = InaddrLoopback;
			None.Address = InaddrNone;
		}
		
		/// <summary>
		///   Constructor from a 32-bit constant.
		/// </summary>
		public IPAddress (long address)
		{
			this.Address = address;
		}

		/// <summary>
		///   Constructor from a dotted quad notation. 
		/// </summary>
		public IPAddress (string ip)
		{
			string[] ips = ip.Split (new char[] {'.'});
			int i;
			uint a = 0;

			for (i = 0; i < ips.Length; i++)
				a = (a << 8) |  (UInt16.Parse(ips [i]));

			Address = a;
		}

		/// <summary>
		///   Used to tell whether an address is a loopback.
		/// </summary>
		/// <param name="addr">Address to compare</param>
		/// <returns></returns>
		public static bool IsLoopback (IPAddress addr)
		{
			return addr.Address == InaddrLoopback;
		}

		/// <summary>
		///   Overrides System.Object.ToString to return
		///   this object rendered in a quad-dotted notation
		/// </summary>
		public override string ToString ()
		{
			return ToString (Address);
		}

		/// <summary>
		///   Returns this object rendered in a quad-dotted notation
		/// </summary>
		static string ToString (long addr)
		{
			return  (addr >> 24).ToString () + "." +
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

		[MonoTODO]
		public static short HostToNetworkOrder (short host)
		{
			return host;
		}

		[MonoTODO]
		public static int HostToNetworkOrder (int host)
		{
			return host;
		}

		[MonoTODO]
		public static long HostToNetworkOrder (long host)
		{
			return host;
		}

		[MonoTODO]
		public static short NetworkToHostOrder (short host)
		{
			return host;
		}

		[MonoTODO]
		public static int NetworkToHostOrder (int host)
		{
			return host;
		}

		[MonoTODO]
		public static long NetworkToHostOrder (long host)
		{
			return host;
		}
	}
	
}
