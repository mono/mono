// CS1623: Iterators cannot have ref or out parameters
// Line: 8
using System;
using System.Collections;

class X
{
	public static IEnumerable Test (ref int a)
	{
		yield return 0;
        }

	static void Main ()
	{
		int i = 3;
		IEnumerable a = Test (ref i);
		Console.WriteLine (a);
	}
}
