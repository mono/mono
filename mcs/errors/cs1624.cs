// CS1624: The body of `X.Test(int)' cannot be an iterator block because `System.Collections.ArrayList' is not an iterator interface type
// Line: 8
using System;
using System.Collections;

class X
{
	public static ArrayList Test (int a)
	{
		yield return 0;
        }

	static void Main ()
	{
		IEnumerable a = Test (3);
		Console.WriteLine (a);
	}
}
