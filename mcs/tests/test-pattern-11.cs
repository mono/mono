using System;

class X
{
	public static int Main ()
	{
		object o = null;
		for (o = "abcd"; o is String s; o = null) {
			Console.WriteLine (s);
		}

		
		return 0;
	}
}