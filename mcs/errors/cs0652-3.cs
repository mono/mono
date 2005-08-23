// cs0652-3.cs : Comparison to integral constant is useless; the constant is outside the range of type 'ushort'
// Line: 11
// Compiler options: /warn:2 /warnaserror
using System;

public class CS0652 {

	public static void Main () 
	{
		ushort us = 0;
		if (us == -1)
			Console.WriteLine (":(");
		else
			Console.WriteLine (":)");
	}
}

