//
// System/Buffer.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {
	public sealed class Buffer {

		public static void BlockCopy( Array src, int srcOffset, Array dst, int dstOffset, int count)
		{
			throw new NotImplementedException ();
		}

		public static int ByteLength( Array array)
		{
			throw new NotImplementedException ();
		}

		public static byte GetByte( Array array, int index)
		{
			throw new NotImplementedException ();
		}

		public static void SetByte( Array array, int index, byte value)
		{
			throw new NotImplementedException ();
		}
	}
}
