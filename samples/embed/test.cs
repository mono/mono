using System;
using System.Runtime.CompilerServices;

class MonoEmbed {
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	extern static string gimme();

	static void Main() {
		Console.WriteLine (gimme ());
	}
}
