//
// System.Buffer.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System
{
	public sealed class Buffer
	{
		private Buffer ()
		{
		}

		public static int ByteLength (Array array)
		{
			// note: the other methods in this class also use ByteLength to test for
			// null and non-primitive arguments as a side-effect.

			if (array == null)
				throw new ArgumentNullException ("array");

			int length = ByteLengthInternal (array);
			if (length < 0)
				throw new ArgumentException (Locale.GetText ("Object must be an array of primitives."));

			return length;
		}

		public static byte GetByte (Array array, int index)
		{
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("index", Locale.GetText(
					"Value must be non-negative and less than the size of the collection."));

			return GetByteInternal (array, index);
		}

		public static void SetByte (Array array, int index, byte value)
		{
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("index", Locale.GetText(
					"Value must be non-negative and less than the size of the collection."));

			SetByteInternal (array, index, value);
		}

		public static void BlockCopy (Array src, int srcOffset, Array dest, int destOffset, int count)
		{
			if (srcOffset < 0)
				throw new ArgumentOutOfRangeException ("srcOffset", Locale.GetText(
					"Non-negative number required."));

			if (destOffset < 0)
				throw new ArgumentOutOfRangeException ("destOffset", Locale.GetText (
					"Non-negative number required."));

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", Locale.GetText (
					"Non-negative number required."));

			if (srcOffset + count > ByteLength (src) || destOffset + count > ByteLength (dest))
				throw new ArgumentException (Locale.GetText (
					"Offset and length were out of bounds for the array or count is greater than" + 
					"the number of elements from index to the end of the source collection."));

			BlockCopyInternal (src, srcOffset, dest, destOffset, count);
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
