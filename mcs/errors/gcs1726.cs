// gcs1726.cs: Friend assembly reference `MyAssemblyName' is invalid. Strong named assemblies must specify a public key token in their InternalsVisibleTo declarations
// Line: 8
// Compiler options: -keyfile:InternalsVisibleTest.snk

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("MyAssemblyName")]

public class Test
{
	static void Main ()
	{
	}
}

