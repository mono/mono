using System;

// Tests private accessibility for dynamic binder

class Test
{
	private delegate int D ();
	private int field = 9;

	public static int Main ()
	{
		D del = () => 5;
		dynamic d = del;
		if (d () != 5)
			return 1;

		d = new Test ();
		if (d.field != 9)
			return 2;

		return 0;
	}
}
