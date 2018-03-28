using System;

class TypePattern
{
	public static int Main ()
	{
		object o = 3;
		bool r = o is System.String t1;
		if (r)
			return 2;

		if (o is string t2)
			return 3;

		long? l = 5;
		bool r3 = l is long t4;

		if (!r3)
			return 8;

		Console.WriteLine ("ok");
		return 0;
	}

	static void Test1 (object arg)
	{
		while (arg is int b) {
			b = 2;
		}
	}

	static string Test2 (object arg)
	{
		if (arg is string s) {
			return s;
		} else {
			s = "";
		}
		
		return s;
	}
}