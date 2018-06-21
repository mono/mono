// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.Private;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography
{
	partial class RandomNumberGeneratorImplementation
	{
		private static unsafe void GetBytes (byte *pbBuffer, int count)
		{
			Interop.MonoGetRandomBytesFallback.GetRandomBytes (pbBuffer, count);
		}
	}
}
