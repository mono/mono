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

		private int port;
		public int Port {
			get {
				return port;
			}
			set {
				port = value;
			}
		}
		
		public IPEndPoint (IPAddress address, int port)
		{
			Address = address;
			Port = port;
		}
		
		public IPEndPoint (long iaddr, int port)
		{
			IPAddress address = new IPAddress ((uint)iaddr);
			
			Address = address;
			this.port = port;
		}

		public override int AddressFamily {
			get {
				return 2;
			}
		}
	}
}
