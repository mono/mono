using System;

class X
{
	public static int Main ()
	{
		int x = 4;
		try {
			throw null;
		} catch (NullReferenceException) if (x > 0) {
			Console.WriteLine ("catch");
			return 0;
		}
	}
}