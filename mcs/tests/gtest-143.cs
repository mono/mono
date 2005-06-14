using System;

class X
{
	static int Test ()
	{
		int? a = 5;
		int? b = a++;

		if (a != 6)
			return 1;
		if (b != 5)
			return 2;

		int? c = ++a;

		if (c != 7)
			return 3;

		b++;
		++b;

		if (b != 7)
			return 4;

		int? d = b++ + ++a;

		if (a != 8)
			return 5;
		if (b != 8)
			return 6;
		if (d != 15)
			return 7;

		return 0;
	}

	static int Main ()
	{
		int result = Test ();
		if (result != 0)
			Console.WriteLine ("ERROR: {0}", result);
		return result;
	}
}
