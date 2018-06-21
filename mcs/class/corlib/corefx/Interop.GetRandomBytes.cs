using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal partial class Interop
{
	internal static unsafe void GetRandomBytes (byte* buffer, int length)
	{
		MonoGetRandomBytesFallback.GetRandomBytes (buffer, length);
	}
}
