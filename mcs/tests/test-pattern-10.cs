using System;

class X
{
	public static int Main ()
	{
		Test (null);
		if (Test ((long) 0) != 1)
			return 1;

		object o = "aa";
		if (o != null) {
			if (o is long s) {
				Console.WriteLine (s);
			}
		} else if (o is string s) {
			Console.WriteLine (s);
		}

		return 0;
	}

	static int Test (object o)
	{
		if (o is long s) {
			return 1;
		}

		return 0;
	}
}