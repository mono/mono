//
// System.Net.EndPoint.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class EndPoint {
		public virtual EndPoint Create (SocketAddress address)
		{
		}

		public virtual SocketAddress Serialize ()
		{
		}

		protected EndPoint ()
		{
		}

		public int AddressFamily {
			virtual get {
			}
		}

		
	}
}

