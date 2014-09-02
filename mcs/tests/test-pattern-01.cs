// Compiler options: -langversion:experimental

using System;

class TypePattern
{
	public static int Main ()
	{
		object o = 3;
		bool r = o is System.String t1;
		if (t1 != null)
			return 1;

		if (r)
			return 2;

		if (o is string t2)
			return 3;

		if (t2 != null)
			return 4;

		object o2 = (int?) 4;
		bool r2 = o2 is byte? t3;

		if (t3 != null)
			return 5;

		if (r2)
			return 6;

		long? l = 5;
		bool r3 = l is long t4;
		if (t4 != 5)
			return 7;

		if (!r3)
			return 8;

		Console.WriteLine ("ok");
		return 0;
	}
}