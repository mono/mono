//
// This test tests both how arguments are selected in the presence
// of ref/out modifiers and the params arguments.
//
using System;

public class Blah {
	static int got;
	
	public static void Foo (ref int i, ref int j)
	{
		got = 1;
	}

	public static int Bar (int j, params int [] args)
	{
		got = 2;
		int total = 0;
		
		foreach (int i in args){
			Console.WriteLine ("My argument: " + i);
			total += i;
		}

		return total;
	}

	public static void Foo (int i, int j)
	{
		got = 3;
	}

	public static int Main ()
	{
		int i = 1;
		int j = 2;

		int [] arr = new int [2] { 0, 1 };

		Foo (i, j);
		if (got != 3)
			return 1;
		
		Foo (ref i, ref j);
		if (got != 1)
			return 2;

		if (Bar (i, j, 5, 4, 3, 3, 2) != 19)
			return 4;

		//if (Bar (1, arr) != 1)
		//	return 5;
		
		if (got != 2)
			return 3;

		return  0;
	}
}
