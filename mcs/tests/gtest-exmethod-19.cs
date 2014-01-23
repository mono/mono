using System;
using System.Collections.Generic;

public static class Rocks
{
	public static string Test_1 (this string t)
	{
		return t + ":";
	}
	
	public static int Test_2<T> (this IEnumerable<T> e)
	{
		return 33;
	}
}

public class Test
{
	delegate string D ();

	public static int Main ()
	{
		string s = "jaj";

		D d = s.Test_1;
		Func<int> d2 = "33".Test_2;
		
		if ((string)d.Target != "jaj")
			return 10;
		
		if ((string)d2.Target != "33")
			return 11;

		string res = d ();
		Console.WriteLine (res);
		if (res != "jaj:")
			return 1;
			
		if (d2 () != 33)
			return 2;
			
		return 0;
	}
}
