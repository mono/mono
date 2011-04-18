// CS1631: Cannot yield a value in the body of a catch clause
// Line: 13
using System;
using System.Collections;

class X
{
	public static IEnumerable Test (int a)
	{
		try {
			;
		} catch {
			yield return 0;
		}
        }

	static void Main ()
	{
		IEnumerable a = Test (3);
		Console.WriteLine (a);
	}
}
