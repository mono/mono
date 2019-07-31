using System.Buffers;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	partial class RandomNumberGenerator
	{
		public static void Fill (Span<byte> data)
		{
			FillSpan (data);
		}

		internal static unsafe void FillSpan (Span<byte> data)
		{
			if (data.Length > 0) {
				fixed (byte* ptr = data) Interop.GetRandomBytes (ptr, data.Length);
			}
		}

		public virtual void GetBytes(Span<byte> data)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(data.Length);
			try
			{
				GetBytes(array, 0, data.Length);
				new ReadOnlySpan<byte>(array, 0, data.Length).CopyTo(data);
			}
			finally
			{
				Array.Clear(array, 0, data.Length);
				ArrayPool<byte>.Shared.Return(array);
			}
		}

		public virtual void GetNonZeroBytes(Span<byte> data)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(data.Length);
			try
			{
				// NOTE: There is no GetNonZeroBytes(byte[], int, int) overload, so this call
				// may end up retrieving more data than was intended, if the array pool
				// gives back a larger array than was actually needed.
				GetNonZeroBytes(array);
				new ReadOnlySpan<byte>(array, 0, data.Length).CopyTo(data);
			}
			finally
			{
				Array.Clear(array, 0, data.Length);
				ArrayPool<byte>.Shared.Return(array);
			}
		}

		public static int GetInt32(int fromInclusive, int toExclusive)
		{
			if (fromInclusive >= toExclusive)
				throw new ArgumentException(SR.Argument_InvalidRandomRange);

			// The total possible range is [0, 4,294,967,295).
			// Subtract one to account for zero being an actual possibility.
			uint range = (uint)toExclusive - (uint)fromInclusive - 1;

			// If there is only one possible choice, nothing random will actually happen, so return
			// the only possibility.
			if (range == 0)
			{
				return fromInclusive;
			}

			// Create a mask for the bits that we care about for the range. The other bits will be
			// masked away.
			uint mask = range;
			mask |= mask >> 1;
			mask |= mask >> 2;
			mask |= mask >> 4;
			mask |= mask >> 8;
			mask |= mask >> 16;

			Span<uint> resultSpan = stackalloc uint[1];
			uint result;

			do
			{
				FillSpan(MemoryMarshal.AsBytes(resultSpan));
				result = mask & resultSpan[0];
			}
			while (result > range);

			return (int)result + fromInclusive;
		}

		public static int GetInt32(int toExclusive)
		{
			if (toExclusive <= 0)
				throw new ArgumentOutOfRangeException(nameof(toExclusive), SR.ArgumentOutOfRange_NeedPosNum);

			return GetInt32(0, toExclusive);
		}
	}
}
