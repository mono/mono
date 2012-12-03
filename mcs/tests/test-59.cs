//
// Tests the varios type conversions.
//
using System;

class X {

	static int test_explicit ()
	{
		object x_int = 1;
		object x_uint_1 = 1u;
		object x_uint_2 = 1U;
		object x_long_1 = 1l;
		object x_long_2 = 1L;
		object x_ulong_1 = 1ul;
		object x_ulong_2 = 1UL;
		object x_ulong_3 = 1lu;
		object x_ulong_4 = 1Lu;
		object x_ulong_5 = 1LU;

		if (!(x_int is int))
			return 1;

		if (!(x_uint_1 is uint))
			return 2;

		if (!(x_uint_2 is uint))
			return 3;

		if (!(x_long_1 is long))
			return 5;

		if (!(x_long_2 is long))
			return 6;

		if (!(x_ulong_1 is ulong))
			return 7;

		if (!(x_ulong_2 is ulong))
			return 8;
		
		if (!(x_ulong_3 is ulong))
			return 9;

		if (!(x_ulong_4 is ulong))
			return 10;

		if (!(x_ulong_5 is ulong))
			return 11;

		return 0;

	}

	static int test_implicit ()
	{
		object i_int = 1;
		object i_uint = 0x80000000;
		object i_long = 0x100000000;
		object i_ulong = 0x8000000000000000;

		if (!(i_int is int))
			return 1;
		if (!(i_uint is uint))
			return 2;
		if (!(i_long is long))
			return 3;
		if (!(i_ulong is ulong))
			return 4;

		return 0;
	}
	
	public static int Main ()
	{
		int v;
		v = test_explicit ();

		if (v != 0)
			return v;

		v = test_implicit ();
		if (v != 0)
			return 20 + v;

		//
		// Just a compilation fix: 21418
		//
		ulong l = 1;
		if (l != 0L)
			;


		// This was a compilation bug, error: 57522
		ulong myulog = 0L;

		Console.WriteLine ("Tests pass");
		return 0;
	}
}
