using System.Buffers;

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
	}
}
