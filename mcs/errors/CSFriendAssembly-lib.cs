using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#if NET_2_0

[assembly: AssemblyKeyFile ("InternalsVisibleTest.snk")]
[assembly: InternalsVisibleTo ("gcs0281, PublicKeyToken=43b5d2e9a794bdcb")]

public class FriendClass
{
	internal static void MyMethod ()
	{
	}
}

#endif

