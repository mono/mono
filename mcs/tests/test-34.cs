//
// This test tests both how arguments are selected in the presence
// of ref/out modifiers and the params arguments.
//
using System;

public struct FancyInt {
	public int value;

	public FancyInt (int v)
	{
		value = v;
	}
	
}

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

	static void In (ref int a)
	{
		a++;
	}

	static void Out (ref int a)
	{
		In (ref a);
	}

	static int AddArray (params int [] valores)
	{
		int total = 0;
		
		for (int i = 0; i < valores.Length; i++)
			total += valores [i];

		return total;
	}

	static int AddFancy (params FancyInt [] vals)
	{
		int total = 0;
		
		for (int i = 0; i < vals.Length; i++)
			total += vals [i].value;

		return total;
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

		int k = 10;

		Out (ref k);
		if (k != 11)
			return 10;

		int [] arr2 = new int [2] {1, 2};

		if (AddArray (arr2) != 3)
			return 11;

		FancyInt f_one = new FancyInt (1);
		FancyInt f_two = new FancyInt (2);

		if (AddFancy (f_one) != 1)
			return 12;

		if (AddFancy (f_one, f_two) != 3)
			return 13;

		Console.WriteLine ("Test passes");
		return  0;
	}
}
