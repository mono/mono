using System;
using System.Collections;

class C
{
	public static IEnumerable Test (bool b, int value)
	{
		if (b) {
			Console.WriteLine (value);
		}
		
		yield return 1;
	}
	
	public static void Main ()
	{
		Test (true, 5);
	}
}