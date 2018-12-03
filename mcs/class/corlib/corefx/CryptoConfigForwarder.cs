// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace System.Security.Cryptography
{
	internal static class CryptoConfigForwarder
	{
		internal static object CreateFromName (string name) => CryptoConfig.CreateFromName (name);

		internal static HashAlgorithm CreateDefaultHashAlgorithm ()
		{
#if FULL_AOT_RUNTIME
			return new System.Security.Cryptography.SHA1CryptoServiceProvider ();
#else
			return (HashAlgorithm)CreateFromName ("System.Security.Cryptography.HashAlgorithm");
#endif
		}
	}
}
