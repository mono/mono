using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

internal partial class Interop
{
	static class MonoGetRandomBytesFallback
	{
		static object _rngAccess = new object ();
		static RNGCryptoServiceProvider _rng;

		internal static void GetRandomBytes (byte[] buffer)
		{
			lock (_rngAccess) {
				if (_rng == null)
					_rng = new RNGCryptoServiceProvider ();
				_rng.GetBytes (buffer);
			}
		}

		internal static unsafe void GetRandomBytes (byte* buffer, int length)
		{
			lock (_rngAccess) {
				if (_rng == null)
					_rng = new RNGCryptoServiceProvider ();
				_rng.GetBytes (buffer, (IntPtr)length);
			}
		}
	}

	internal static unsafe void GetRandomBytes (byte* buffer, int length)
	{
		MonoGetRandomBytesFallback.GetRandomBytes (buffer, length);
	}
}
