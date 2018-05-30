// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Private;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{
	partial class RandomNumberGeneratorImplementation
	{
#if MONO_FEATURE_BTLS
		internal const string BTLS_DYLIB = "libmono-btls-shared";

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_get_random_bytes (ref byte buffer, int num);
#endif

		private static void GetBytes (ref byte pbBuffer, int count)
		{
			Debug.Assert (count > 0);

#if MONO_FEATURE_BTLS
			var ret = mono_btls_get_random_bytes (ref pbBuffer, count);
			if (ret != 1)
				throw new CryptographicException ("mono_btls_get_random_bytes() failed.");
#else
			throw new CryptographicException ("mono_btls_get_random_bytes() not available.");
#endif
		}
	}
}
