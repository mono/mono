//
// System.Net.SocketAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class SocketAddress {
		short family;
		int size;
		
		public SocketAddress (short family, int size)
		{
			this.family = family;
			this.size = size;
		}

		public SocketAddress (short family)
		{
			this.family = family;
		}
		
		public short Family {
			get {
				return family;
			}
		}

		public int Size {
			get {
				return size;
			}
		}

		public byte this [ int offset ] {
			get {
				// FIXME; Unimplemented.
				return 0;
			}

			set {
				// FIXME: Unimplemented.
			}
		}
	}
}
