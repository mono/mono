using System;

class Foo {
	public static int Main ()
	{
		try {
			f ();
			return 1;
		} catch {
		}

		try {
			f2 ();
			return 2;
		} catch (ApplicationException) {
		}

		return 0;
	}

	static void f ()
	{
		try {
			goto skip;
		} catch {
			goto skip;
		} finally {
			throw new System.Exception ();
		}
	skip:
		;
	}

	static void f2 ()
	{
		try {
			goto FinallyExit;
		} finally {
			throw new ApplicationException ();
		}
	FinallyExit:
		Console.WriteLine ("Too late");
	}
}
