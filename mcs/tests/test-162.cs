using System;

struct A
{
	public int a;
	private long b;
	private float c;

	public A (int foo)
	{
		a = foo;
		b = 8;
		c = 9.0F;
	}
}

struct B
{
	public int a;
}

struct C
{
	public long b;

	public C (long foo)
	{
		b = foo;
	}

	// has `this' initializer, no need to initialize fields.
	public C (string foo)
		: this (500)
	{ }
}

class X
{
	static void test_output (A x)
	{
	}

	static void test_output (B y)
	{
	}

	static void test1 ()
	{
		A x;

		x.a = 5;
		Console.WriteLine (x.a);
	}

	static void test2 ()
	{
		B y;

		y.a = 5;
		Console.WriteLine (y.a);
		Console.WriteLine (y);
	}

	static void test3 ()
	{
		A x = new A (85);

		Console.WriteLine (x);
	}

	static void test4 (A x)
	{
		x.a = 5;
	}

	static void test5 (out A x)
	{
		x = new A (85);
	}

	static void test6 (out B y)
	{
		y.a = 1;
	}

	public static int Main ()
	{
		// Compilation-only test.
		return 0;
	}
}
