// CS0165: Use of unassigned local variable `a'
// Line: 9

using System;

class C {
	public static int test5 ()
	{
		int a;

		try {
			Console.WriteLine ("TRY");
			a = 8;
		} catch {
			a = 9;
		} finally {
			// CS0165
			Console.WriteLine (a);
		}

		return a;
	}
}
