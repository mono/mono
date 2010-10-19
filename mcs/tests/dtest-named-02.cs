using System;

public class Test
{
	static int counter;

	static int M1 ()
	{
		if (counter != 2)
			throw new ApplicationException ();

		return counter++;
	}

	static int M2 ()
	{
		if (counter != 3)
			throw new ApplicationException ();

		return counter++;
	}

	static dynamic M3 ()
	{
		if (counter != 1)
			throw new ApplicationException ();

		return counter++;
	}

	static int Foo (int a, int b, int c)
	{
		if (a != 2)
			return 1;

		if (b != 3)
			return 2;

		if (c != 1)
			return 3;

		return 0;
	}

	public static int Main ()
	{
		counter = 1;
		return Foo (c: M3 (), a: M1 (), b: M2 ());
	}
}