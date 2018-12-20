#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

using System.Runtime.CompilerServices;
using System.Runtime;

namespace System
{
	partial class Buffer
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern unsafe void InternalMemcpy (byte *dest, byte *src, int count);

		public static int ByteLength (Array array)
		{
			// note: the other methods in this class also use ByteLength to test for
			// null and non-primitive arguments as a side-effect.

			if (array == null)
				throw new ArgumentNullException ("array");

			int length = _ByteLength (array);
			if (length < 0)
				throw new ArgumentException (Locale.GetText ("Object must be an array of primitives."));

			return length;
		}

		public static byte GetByte (Array array, int index)
		{
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("index");

			return _GetByte (array, index);
		}

		public static void SetByte (Array array, int index, byte value)
		{
			if (index < 0 || index >= ByteLength (array))
				throw new ArgumentOutOfRangeException ("index");

			_SetByte (array, index, value);
		}

		public static void BlockCopy (Array src, int srcOffset, Array dst, int dstOffset, int count)
		{
			if (src == null)
				throw new ArgumentNullException ("src");

			if (dst == null)
				throw new ArgumentNullException ("dst");

			if (srcOffset < 0)
				throw new ArgumentOutOfRangeException ("srcOffset", Locale.GetText(
					"Non-negative number required."));

			if (dstOffset < 0)
				throw new ArgumentOutOfRangeException ("dstOffset", Locale.GetText (
					"Non-negative number required."));

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", Locale.GetText (
					"Non-negative number required."));

			// We do the checks in unmanaged code for performance reasons
			bool res = InternalBlockCopy (src, srcOffset, dst, dstOffset, count);
			if (!res) {
				// watch for integer overflow
				if ((srcOffset > ByteLength (src) - count) || (dstOffset > ByteLength (dst) - count))
					throw new ArgumentException (Locale.GetText (
						"Offset and length were out of bounds for the array or count is greater than " + 
						"the number of elements from index to the end of the source collection."));
			}
		}

		[CLSCompliantAttribute (false)]
		public static unsafe void MemoryCopy (void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
		{
			if (sourceBytesToCopy > destinationSizeInBytes) {
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
			}

			var src = (byte*)source;
			var dst = (byte*)destination;
			while (sourceBytesToCopy > int.MaxValue) {
				Memcpy (dst, src, int.MaxValue);
				sourceBytesToCopy -= int.MaxValue;
				src += int.MaxValue;
				dst += int.MaxValue;
			}

			memcpy1 (dst, src, (int) sourceBytesToCopy);
		}

		[CLSCompliantAttribute (false)]
		public static unsafe void MemoryCopy (void* source, void* destination, ulong destinationSizeInBytes, ulong sourceBytesToCopy)
		{
			if (sourceBytesToCopy > destinationSizeInBytes) {
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sourceBytesToCopy);
			}

			var src = (byte*)source;
			var dst = (byte*)destination;
			while (sourceBytesToCopy > int.MaxValue) {
				Memcpy (dst, src, int.MaxValue);
				sourceBytesToCopy -= int.MaxValue;
				src += int.MaxValue;
				dst += int.MaxValue;
			}

			Memcpy (dst, src, (int) sourceBytesToCopy);
		}

		internal static unsafe void memcpy4 (byte *dest, byte *src, int size) {
			/*while (size >= 32) {
				// using long is better than int and slower than double
				// FIXME: enable this only on correct alignment or on platforms
				// that can tolerate unaligned reads/writes of doubles
				((double*)dest) [0] = ((double*)src) [0];
				((double*)dest) [1] = ((double*)src) [1];
				((double*)dest) [2] = ((double*)src) [2];
				((double*)dest) [3] = ((double*)src) [3];
				dest += 32;
				src += 32;
				size -= 32;
			}*/
			while (size >= 16) {
				((int*)dest) [0] = ((int*)src) [0];
				((int*)dest) [1] = ((int*)src) [1];
				((int*)dest) [2] = ((int*)src) [2];
				((int*)dest) [3] = ((int*)src) [3];
				dest += 16;
				src += 16;
				size -= 16;
			}
			while (size >= 4) {
				((int*)dest) [0] = ((int*)src) [0];
				dest += 4;
				src += 4;
				size -= 4;
			}
			while (size > 0) {
				((byte*)dest) [0] = ((byte*)src) [0];
				dest += 1;
				src += 1;
				--size;
			}
		}
		internal static unsafe void memcpy2 (byte *dest, byte *src, int size) {
			while (size >= 8) {
				((short*)dest) [0] = ((short*)src) [0];
				((short*)dest) [1] = ((short*)src) [1];
				((short*)dest) [2] = ((short*)src) [2];
				((short*)dest) [3] = ((short*)src) [3];
				dest += 8;
				src += 8;
				size -= 8;
			}
			while (size >= 2) {
				((short*)dest) [0] = ((short*)src) [0];
				dest += 2;
				src += 2;
				size -= 2;
			}
			if (size > 0)
				((byte*)dest) [0] = ((byte*)src) [0];
		}
		static unsafe void memcpy1 (byte *dest, byte *src, int size) {
			while (size >= 8) {
				((byte*)dest) [0] = ((byte*)src) [0];
				((byte*)dest) [1] = ((byte*)src) [1];
				((byte*)dest) [2] = ((byte*)src) [2];
				((byte*)dest) [3] = ((byte*)src) [3];
				((byte*)dest) [4] = ((byte*)src) [4];
				((byte*)dest) [5] = ((byte*)src) [5];
				((byte*)dest) [6] = ((byte*)src) [6];
				((byte*)dest) [7] = ((byte*)src) [7];
				dest += 8;
				src += 8;
				size -= 8;
			}
			while (size >= 2) {
				((byte*)dest) [0] = ((byte*)src) [0];
				((byte*)dest) [1] = ((byte*)src) [1];
				dest += 2;
				src += 2;
				size -= 2;
			}
			if (size > 0)
				((byte*)dest) [0] = ((byte*)src) [0];
		}

		internal static unsafe void Memcpy (byte *dest, byte *src, int len) {
			// For bigger lengths, we use the heavily optimized native code
			if (len > 32) {
				InternalMemcpy (dest, src, len);
				return;
			}
			// FIXME: if pointers are not aligned, try to align them
			// so a faster routine can be used. Handle the case where
			// the pointers can't be reduced to have the same alignment
			// (just ignore the issue on x86?)
			if ((((int)dest | (int)src) & 3) != 0) {
				if (((int)dest & 1) != 0 && ((int)src & 1) != 0 && len >= 1) {
					dest [0] = src [0];
					++dest;
					++src;
					--len;
				}
				if (((int)dest & 2) != 0 && ((int)src & 2) != 0 && len >= 2) {
					((short*)dest) [0] = ((short*)src) [0];
					dest += 2;
					src += 2;
					len -= 2;
				}
				if ((((int)dest | (int)src) & 1) != 0) {
					memcpy1 (dest, src, len);
					return;
				}
				if ((((int)dest | (int)src) & 2) != 0) {
					memcpy2 (dest, src, len);
					return;
				}
			}
			memcpy4 (dest, src, len);
		}

		internal static unsafe void Memmove (byte *dest, byte *src, uint len)
		{
            if (((nuint)dest - (nuint)src < len) || ((nuint)src - (nuint)dest < len))
				goto PInvoke;
			Memcpy (dest, src, (int) len);
			return;

            PInvoke:
            RuntimeImports.Memmove(dest, src, len);
		}

		internal static void Memmove<T>(ref T destination, ref T source, nuint elementCount)
		{
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                unsafe {
                    fixed (byte* pDestination = &Unsafe.As<T, byte>(ref destination), pSource = &Unsafe.As<T, byte>(ref source))
                        Memmove(pDestination, pSource, (uint)elementCount * (uint)Unsafe.SizeOf<T>());
                }
			} else {
                unsafe {
                    fixed (byte* pDestination = &Unsafe.As<T, byte>(ref destination), pSource = &Unsafe.As<T, byte>(ref source))
                        RuntimeImports.Memmove_wbarrier(pDestination, pSource, (uint)elementCount, typeof(T).TypeHandle.Value);
				}
			}
		}
	}
}
