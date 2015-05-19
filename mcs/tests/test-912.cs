// Compiler options: -r:test-912-lib.dll

using System;
using TypeLib;

public class Bar
{
	public const ushort FIELD = Foo.CONSTANT;

	public static int Main ()
	{
		Console.WriteLine (FIELD);
		if (FIELD != 65535)
			return 1;

		return 0;
	}
}