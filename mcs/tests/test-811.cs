using System;

class C
{
	static void TestRefValue (__arglist)
	{
		ArgIterator args = new ArgIterator (__arglist);

		var o = __refvalue ( args.GetNextArg (),int);
		for (int i = 0; i < args.GetRemainingCount (); i++) {
			Console.WriteLine (__refvalue (args.GetNextArg (), int));
		}
	}

	public static int Main ()
	{
		int i = 1;
		TypedReference tr = __makeref (i);
		Type t = __reftype (tr);
		if (t != i.GetType ())
			return 1;

		TestRefValue (__arglist (5, 1, 2));

		return 0;
	}
}