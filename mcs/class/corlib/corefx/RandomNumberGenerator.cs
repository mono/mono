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
	}
}
