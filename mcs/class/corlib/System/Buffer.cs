//
// System/Buffer.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System {
	public sealed class Buffer {

		private Buffer () {}

		public static int ByteLength (Array array) {
			// note: the other methods in this class also use ByteLength to test for
			// null and non-primitive arguments as a side-effect.

			if (array == null)
				throw new ArgumentNullException ();	// default message

			int length = ByteLengthInternal (array);
			if (length < 0)
				throw new ArgumentException ("Object must be array of primitives.");

			return length;
		}

		public static byte GetByte (Array array, int index) {
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("Index was out of range. Must be non-negative and less than the size of the collection.");

			return GetByteInternal (array, index);
		}

		public static void SetByte (Array array, int index, byte value) {
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("Index was out of range. Must be non-negative and less than the size of the collection.");

			SetByteInternal (array, index, value);
		}

		public static void BlockCopy (Array src, int src_offset, Array dest, int dest_offset, int count) {
			if (src_offset < 0 || dest_offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("Non-negative number required.");

			if (src_offset + count > ByteLength (src) || dest_offset + count > ByteLength (dest))
				throw new ArgumentException ("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

			BlockCopyInternal (src, src_offset, dest, dest_offset, count);
		}

		// private

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int ByteLengthInternal (Array array);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static byte GetByteInternal (Array array, int index);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void SetByteInternal (Array array, int index, int value);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void BlockCopyInternal (Array src, int src_offset, Array dest, int dest_offset, int count);
	}
}
