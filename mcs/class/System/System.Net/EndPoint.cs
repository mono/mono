//
// System.Net.EndPoint.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Net.Sockets;

namespace System.Net {
	[Serializable]
	public abstract class EndPoint {

		// NB: These methods really do nothing but throw
		// NotSupportedException
		
		public virtual AddressFamily AddressFamily {
			get {
				throw new NotSupportedException();
			}
		}
		
		public virtual EndPoint Create (SocketAddress address)
		{
			throw new NotSupportedException();
		}

		public virtual SocketAddress Serialize ()
		{
			throw new NotSupportedException();
		}

		protected EndPoint ()
		{
		}
	}
}

