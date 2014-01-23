using System;
using System.Collections;

delegate bool predicate (object a);

class X {

	public static int Main ()
	{
		ArrayList a = new ArrayList ();
		for (int i = 0; i < 10; i++)
			a.Add (i);

		ArrayList even = Find (delegate (object arg) {
			return ((((int) arg) % 2) == 0);
		}, a);

		Console.WriteLine ("Even numbers");
		foreach (object r in even){
			Console.WriteLine (r);
		}
		if (even.Count != 5)
			return 1;
		if (((int)even [0]) != 0 ||
		    ((int)even [1]) != 2 ||
		    ((int)even [2]) != 4 ||
		    ((int)even [3]) != 6 ||
		    ((int)even [4]) != 8)
			return 2;
				
		return 0;
	}

	static ArrayList Find (predicate p, ArrayList source)
	{
		ArrayList result = new ArrayList ();

		foreach (object a in source){
			if (p (a))
				result.Add (a);
		}

		return result;
	}
}
