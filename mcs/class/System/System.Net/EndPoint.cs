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
			return null;
		}

		public virtual SocketAddress Serialize ()
		{
			return null;
		}

		protected EndPoint ()
		{
		}

		public virtual int AddressFamily {
			get {
				return 0;
			}
		}

		
	}
}

