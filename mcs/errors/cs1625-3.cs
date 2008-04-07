// CS1625: Cannot yield in the body of a finally clause
// Line: 13
using System;
using System.Collections;

class X
{
	public static IEnumerable Test (int a)
	{
		try {
			;
		} finally {
			yield break;
		}
        }

	static void Main ()
	{
		IEnumerable a = Test (3);
		Console.WriteLine (a);
	}
}
