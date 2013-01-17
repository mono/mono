// Compiler options: -r:test-399-lib.dll

using System;

class TestVararg
{
	static void F (RuntimeArgumentHandle rah)
	{
	}
	
	static void G (__arglist)
	{
		F (new RuntimeArgumentHandle ());
		F (__arglist);
	}
	
	public static int Main ()
	{
		int result = Vararg.AddABunchOfInts (__arglist ( 2, 3, 4 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 9)
			return 1;

		result = Vararg.AddASecondBunchOfInts (16, __arglist ( 2, 3, 4 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 9)
			return 2;

		Vararg s = new Vararg ();

		result = s.InstAddABunchOfInts (__arglist ( 2, 3, 4, 5 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 14)
			return 3;

		result = s.InstAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6 ));
		Console.WriteLine ("Answer: {0}", result);

		if (result != 20)
			return 4;

		result = s.InstVtAddABunchOfInts (__arglist ( 2, 3, 4, 5 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 14)
			return 5;

		result = s.InstVtAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 20)
			return 6;

		result = Vararg.VtAddABunchOfInts (__arglist ( 2, 3, 4, 5, 1 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 15)
			return 7;

		result = Vararg.VtAddASecondBunchOfInts (16, __arglist ( 2, 3, 4, 5, 6, 1 )).res;
		Console.WriteLine ("Answer: {0}", result);

		if (result != 21)
			return 8;

		return 0;
	}
}
