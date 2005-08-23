// cs0652-2.cs : Comparison to integral constant is useless; the constant is outside the range of type 'byte'
// Line: 11
// Compiler options: /warn:2 /warnaserror
using System;

public class CS0652 {

	public static void Main () 
	{
		byte b = 0;
		if (b == -1)
			Console.WriteLine (":(");
		else
			Console.WriteLine (":)");
	}
}

