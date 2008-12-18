// CS0030: Cannot convert type `System.Enum' to `int'
// Line: 11

using System;

class MainClass
{
	public static void Main ()
	{
		Enum e = null;
		int i = (int) e;
	}
}
