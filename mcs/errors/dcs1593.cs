// CS1593: Delegate `System.Action<int>' does not take `2' arguments
// Line: 13

using System;

public class Test
{
	public static void Main ()
	{
		Action<int> a = (i) => {};

		dynamic d = 1;
		a (d, true);
	}
}
