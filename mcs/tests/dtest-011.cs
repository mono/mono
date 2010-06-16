using System;

class C
{
	void foo (int i)
	{
		Console.WriteLine ("Got int: {0}", i);
	}

	void foo (string message)
	{
		throw new ApplicationException ();
	}

	static void foo_static (long l)
	{
		Console.WriteLine ("Got static long: {0}", l);
	}

	void test ()
	{
		dynamic d = 1;
		foo (d);
		foo_static (d);
	}

	static void Main ()
	{
		new C ().test ();
	}
}
