using System;
using System.Collections.Generic;

public class TestCase
{
	public static void Main ()
	{
		Test (new IList<int> [] { new int[] { 1, 2, 3 } });
	}
	
	public static void Test<T> (IList<IList<T>> l)
	{
		Action action = delegate {
			var temp = l;
			Func<IList<IList<T>>, int> f = a => 1;
			f (temp);
		};
		
		action ();
	}
}