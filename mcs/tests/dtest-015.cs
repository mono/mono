using System;

class Program
{
	public static int Main ()
	{
		dynamic d = new Program ();
		var p = d as int?;
		if (p != null)
			return 1;

		var p2 = d as Program;
		if (p2 == null)
			return 2;

		return 0;
	}
}
