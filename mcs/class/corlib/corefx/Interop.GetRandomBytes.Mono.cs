using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal partial class Interop
{
	internal static class MonoGetRandomBytesFallback
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
				_rng.GetBytes (buffer, length);
			}
		}
	}
}
