// CS0652: A comparison between a constant and a variable is useless. The constant is out of the range of the variable type `char'
// Line: 12
// Compiler options: /warn:2 /warnaserror

using System;

public class CS0652 {

	public static void Main () 
	{
		char value = 'a';
		if (value < SByte.MinValue)
			return;
	}
}

