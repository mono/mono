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

struct D
{
	public int foo;
}

struct E
{
	public D d;
	public bool e;

	public E (int foo)
	{
		this.e = true;
		this.d.foo = 9;
	}
}

struct F
{
	public E e;
	public float f;
}

class X
{
	static void test_output (A x)
	{ }

	static void test_output (B y)
	{ }

	static void test_output (E e)
	{ }

	static void test_output (F f)
	{ }

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

	static void test7 ()
	{
		E e;
		e.e = true;
		e.d.foo = 5;

		test_output (e);
	}

	static void test8 ()
	{
		F f;
		f.e.e = true;
		f.e.d.foo = 5;
		f.f = 3.14F;

		test_output (f);
	}

	static void test9 ()
	{
		E e = new E (5);
		Console.WriteLine (e.d.foo);
	}

	static void test10 ()
	{
		F f;
		f.e = new E (10);
		Console.WriteLine (f.e.d.foo);
		Console.WriteLine (f.e.d);
		f.f = 3.14F;
		Console.WriteLine (f);
	}

	public static int Main ()
	{
		// Compilation-only test.
		return 0;
	}
}
