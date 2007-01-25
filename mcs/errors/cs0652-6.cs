// CS0652: A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `short'
// Line: 12
// Compiler options: /warn:2 /warnaserror

using System;

public class CS0652 {

	public static void Main () 
	{
		short value = 5;
		if (value > char.MaxValue)
			return;
	}
}

