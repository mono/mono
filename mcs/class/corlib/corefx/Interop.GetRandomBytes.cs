using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal partial class Interop
{
	static class MonoGetRandomBytesFallback
	{
		static object _rngAccess = new object ();
		static RandomNumberGenerator _rng;

		internal static void Test (byte[] buffer)
		{
			lock (_rngAccess) {
				if (_rng == null)
					_rng = new RNGCryptoServiceProvider ();
				_rng.GetBytes (buffer);
			}
		}
	}

	internal static unsafe void GetRandomBytes (byte* buffer, int length)
	{
		var bytes = new byte[length];
		MonoGetRandomBytesFallback.Test (bytes);
		Marshal.Copy (bytes, 0, (IntPtr)buffer, length);
	}
}
