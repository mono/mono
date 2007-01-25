// CS0652: A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `ushort'
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

