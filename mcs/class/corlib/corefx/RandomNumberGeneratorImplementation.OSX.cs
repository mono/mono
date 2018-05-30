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
		const string libSystem = "/usr/lib/libSystem.dylib";

		[DllImport (libSystem)]
		extern static int CCRandomGenerateBytes (/* void* */ ref byte bytes, /* size_t */ IntPtr count);

		private static void GetBytes (ref byte pbBuffer, int count)
		{
			Debug.Assert (count > 0);

			if (CCRandomGenerateBytes (ref pbBuffer, (IntPtr)count) != 0)
				throw new CryptographicException ("CCRandomGenerateBytes() failed.");
		}
	}
}
