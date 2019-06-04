using System;
using System.Reflection;

class X
{
	public static int Main ()
	{
		if (GetTypeHelper ("Mono.Runtime").FullName != "Mono.Runtime")
			return 1;

		return 0;
	}

	static Type GetTypeHelper (string name)
	{
		return Type.GetType (name, true);
	}	
}