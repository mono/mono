using System;

class X
{
	public readonly int x;

	public X (int x)
	{
		this.x = x;
	}

	public override string ToString ()
	{
		return String.Format ("X ({0})", x);
	}

	public static X operator & (X a, X b)
	{
		return new X (a.x * b.x);
	}

	public static X operator | (X a, X b)
	{
		return new X (a.x + b.x);
	}

	// Returns true if the value is odd.
	public static bool operator true (X x)
	{
		return (x.x % 2) != 0;
	}

	// Returns true if the value is even.
	public static bool operator false (X x)
	{
		return (x.x % 2) == 0;
	}

	public static int Test ()
	{
		X x = new X (3);
		X y = new X (4);

		X t1 = x && y;
		X t2 = y && x;
		X t3 = x || y;
		X t4 = y || x;

		// Console.WriteLine ("TEST: {0} {1} {2} {3} {4} {5}", x, y, t1, t2, t3, t4);

		if (t1.x != 12)
			return 1;
		if (t2.x != 4)
			return 2;
		if (t3.x != 3)
			return 3;
		if (t4.x != 7)
			return 4;

		return 0;
	}

	public static int Main ()
	{
		int result = Test ();
		Console.WriteLine ("RESULT: {0}", result);
		return result;
	}
}
