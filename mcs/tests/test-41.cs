//
// This tests the ref access to parameters
//
using System;

class X {

	static void A (ref int a, ref uint b, ref sbyte c, ref byte d, ref long e, ref ulong f,
		       ref short g, ref ushort h, ref char i, ref X x, ref float j, ref double k)
	{
		if (a == 1)
			a = 2;

		if (b == 1)
			b = 2;

		if (c == 1)
			c = 2;

		if (d == 1)
			d = 2;

		if (e == 1)
			e = 2;

		if (f == 1)
			f = 2;

		if (g == 1)
			g = 2;

		if (h == 1)
			h = 2;

		if (i == 'a')
			i = 'b';

		if (x == null)
			x = new X ();

		if (j == 1.0)
			j = 2.0F;
		if (k == 1.0)
			k = 2.0;
	}

	public static int Main ()
	{
		int a = 1;
		uint b = 1;
		sbyte c = 1;
		byte d = 1;
		long e = 1;
		ulong f = 1;
		short g = 1;
		ushort h = 1;
		char i = 'a';
		float j = 1.0F;
		double k = 1.0;
		X x = null;

		A (ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref i, ref x, ref j, ref k);

		if (a != 2)
			return 1;
		if (b != 2)
			return 2;
		if (c != 2)
			return 3;
		if (d != 2)
			return 4;
		if (e != 2)
			return 5;
		if (f != 2)
			return 6;
		if (g != 2)
			return 7;
		if (h != 2)
			return 8;
		if (i != 'b')
			return 9;
		if (j != 2.0)
			return 10;
		if (k != 2.0)
			return 11;
		if (x == null)
			return 12;

		Console.WriteLine ("Test passed");
		return 0;
	}
}
	
