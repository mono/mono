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
	
	static int MethodBest (short d)
	{
		return 1;
	}
	
	static int MethodBest (dynamic d)
	{
		return -1;
	}

	void test ()
	{
		dynamic d = 1;
		foo (d);
		foo_static (d);
	}

	public static int Main ()
	{
		new C ().test ();
		
		if (MethodBest (1) != 1)
			return 1;
		
		return 0;
	}
}
