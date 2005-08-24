// cs0652-4.cs : Comparison to integral constant is useless; the constant is outside the range of type `short'
// Line: 11
// Compiler options: /warn:2 /warnaserror
using System;

public class CS0652 {

	public static void Main () 
	{
		short us = 0;
		if (us == -10000000)
			Console.WriteLine (":(");
		else
			Console.WriteLine (":)");
	}
}

