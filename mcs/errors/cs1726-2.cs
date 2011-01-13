// gcs1726-2.cs: Friend assembly reference `MyAssemblyName, PublicKeyToken=43b5d2e9a794bdcb' is invalid. Strong named assemblies must specify a public key in their InternalsVisibleTo declarations
// Line: 8
// Compiler options: -keyfile:key.snk

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("MyAssemblyName, PublicKeyToken=43b5d2e9a794bdcb")]

public class Test
{
	static void Main ()
	{
	}
}

