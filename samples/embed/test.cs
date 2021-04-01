using System;
using System.Runtime.CompilerServices;

class MonoEmbed {
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	extern static string gimme();

	static int Main ()
	{
		System.Console.WriteLine(gimme ());
		if (gimme ().Equals("All your monos are belong to us!"))
			return 0;
		return 100;
	}
}
