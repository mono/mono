using System;

struct A
{
	public int a;
	private long b;
	private float c;

	public A (int foo)
	// CS0171
	{
		a = foo;
		// CS0170
		b = (long) c;
	}
}

class X
{
	static void test1 (out A a)
	{
		// CS0165
		a.a = 5;
	}

	static void test_output (A a)
	{
	}

	static void test2 ()
	{
		A a;

		// CS0165
		test_output (a);
	}

	static void test3 ()
	{
		A a;

		a.a = 5;
		// CS0165
		test_output (a);
	}

	static void test4 ()
	{
		A a;

		// CS0165
		Console.WriteLine (a.a);
	}

	public static int Main ()
	{
		// Compilation-only test.
		return 0;
	}
}
