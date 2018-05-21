using System;
using System.Threading;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

static partial class Interop
{
	static RandomNumberGenerator randomNumberGenerator;

	internal static unsafe void GetRandomBytes (byte* buffer, int length)
	{
		if (randomNumberGenerator == null)
			Interlocked.CompareExchange (ref randomNumberGenerator, RandomNumberGenerator.Create (), null);
		lock (randomNumberGenerator) {
			var tempBytes = new byte[length];
			randomNumberGenerator.GetBytes (tempBytes);
			Marshal.Copy (tempBytes, 0, new IntPtr (buffer), length);
			Array.Clear (tempBytes, 0, length);
		}
	}
}
