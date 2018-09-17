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
		internal static extern unsafe bool InternalMemcpy (byte *dest, byte *src, int count);

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

		internal static unsafe void Memcpy (byte *dest, byte *src, int len)
		{
			InternalMemcpy (dest, src, len);
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
				InternalMemcpy (dst, src, int.MaxValue);
				sourceBytesToCopy -= int.MaxValue;
				src += int.MaxValue;
				dst += int.MaxValue;
			}

			InternalMemcpy (dst, src, (int) sourceBytesToCopy);
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
				InternalMemcpy (dst, src, int.MaxValue);
				sourceBytesToCopy -= int.MaxValue;
				src += int.MaxValue;
				dst += int.MaxValue;
			}

			InternalMemcpy (dst, src, (int) sourceBytesToCopy);
		}

		internal static unsafe void Memmove (byte *dest, byte *src, uint len)
		{
            if (((nuint)dest - (nuint)src < len) || ((nuint)src - (nuint)dest < len))
				goto PInvoke;
			InternalMemcpy (dest, src, (int) len);
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
