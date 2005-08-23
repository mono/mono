// Compiler options: /warnaserror

using System;
class Test
{
	static ulong value = 0;

	public static void Main ()
	{
		if (value < 9223372036854775809)
			Console.WriteLine ();
	}
}

