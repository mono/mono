// Compiler options: -warnaserror -warn:4

using System;

[assembly: CLSCompliant (true)]

public class Foo
{
#pragma warning disable 3019
	[CLSCompliant (false)]
#pragma warning restore 3019
	internal ushort ToUint16 ()
	{
		return ushort.MaxValue;
	}

	public static void Main ()
	{
	}
}
