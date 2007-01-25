// CS0652: A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `byte'
// Line: 12
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

