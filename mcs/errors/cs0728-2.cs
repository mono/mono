// CS0728: Possibly incorrect assignment to `d' which is the argument to a using or lock statement
// Line: 12
// Compiler options: -warnaserror

using System;

public class Foo
{
	public static void Test (IDisposable d)
	{
		using (d) {
			d = null;
		}
	}
}
