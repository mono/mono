using System.Runtime.CompilerServices;

namespace System
{
	partial class MemoryExtensions
	{
		public static ReadOnlySpan<char> AsSpan (this string text)
		{
			if (text == null)
				return default;

			return new ReadOnlySpan<char> (Unsafe.As<Pinnable<char>> (text), StringAdjustment, text.Length);
		}
	}
}
