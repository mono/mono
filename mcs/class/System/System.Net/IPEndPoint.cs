//
// System.Net.IPEndPoint.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class IPEndPoint : EndPoint {
		public IPAddress Address;

		public const int MaxPort = 65535;
		public const int MinPort = 0;

		public short Port;
		
		public IPEndPoint (IPAddress address, int port)
		{
			Address = address;
			Port = port;
		}
		
		public IPEndPoint (int iaddr, int port)
		{
			IPAddress address = new IPAddress (iaddr);
			
			IPEndPoint (address, port);
		}

		public int AddressFamily {
			override get {
				return 2;
			}
		}
	}
}
