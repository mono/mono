using System;

class X
{
	static int Foo = 10;

	static void Test ()
	{
		while (true) {
			if (Foo == 1)
				throw new Exception ("Error Test");
			else
				break;
		}

		Foo = 20;
	}

	public static int Main ()
	{
		Test ();
		if (Foo != 20)
			return 1;
		return 0;
	}
}
