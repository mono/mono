// CS0253: Possible unintended reference comparison. Consider casting the right side expression to type `System.Action' to get value comparison
// Line: 13
// Compiler options: -warnaserror

using System;

class MainClass
{
	public static void Main ()
	{
		Action a = null;
		object b = null;
		var x = a == b;
	}
}