//
// System.Net.IPAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	// <remarks>
	//   Encapsulates an IP Address.
	// </remarks>
	public class IPAddress {
		public int Address;
		
		public const int InaddrAny       = 0;
		public const int InaddrBroadcast = 0xffffffff;
		public const int InaddrLoopback  = 0x7f000001;
		public const int InaddrNone      = 0xffffffff;
		
		// <summary>
		//   Constructor from a 32-bit constant.
		// </summary>
		public IPAddress (int address)
		{
			this.address = address;
		}

		// <summary>
		//   Constructor from a dotted quad notation. 
		// </summary>
		public IPAddress (string ip)
		{
			string ips = ip.Split (".");
			int i, a = 0;

			for (i = 0; i < ips.Count; i++)
				a = (a << 8) |  (ips [i].ToInt16 ());

			Address = a;
		}

		// <summary>
		//   Used to tell whether an address is a loopback.
		// </summary>
		// <param name="addr">Address to compare</param>
		// <returns></returns>
		public static bool IsLoopback (IPAddress addr)
		{
			return addr.Address == InaddrLoopback;
		}

		// <summary>
		//   Overrides System.Object.ToString to return
		//   this object rendered in a quad-dotted notation
		// </summary>
		public override string ToString ()
		{
			System.Net.IPAddress.ToString (Address);
		}

		// <summary>
		//   Returns this object rendered in a quad-dotted notation
		// </summary>
		public static string ToString (int addr)
		{
			return  (addr >> 24).ToString () + "." +
				((addr >> 16) & 0xff).ToString () + "." +
				((addr >> 8) & 0xff).ToString () + "." +
				(addr & 0xff).ToString ();
		}

		// <returns>
		//   Whether both objects are equal.
		// </returns>
		public override bool Equal (object other)
		{
			if (typeof (other) is System.Net.IPAddress){
				return Address == ((System.Net.IPAddress) other).Address;
			}
			return false;
		}
	}
	
}
