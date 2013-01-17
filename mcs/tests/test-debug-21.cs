using System;
using System.Collections.Generic;

class C
{
	IEnumerable<int> Test ()
	{
		Console.WriteLine ("init");
		try
		{
			yield return 1;
		}
		finally
		{
			Console.WriteLine ("aa");
		}
		
		yield return 2;
	}

	public static void Main ()
	{
	}
}