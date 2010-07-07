// CS0253: Possible unintended reference comparison. Consider casting the right side expression to type `string' to get value comparison
// Line: 10
// Compiler options: -warn:2 -warnaserror

using System;

class X {
	static void Main() {
		object a = "11";
		Console.WriteLine("11" == a);
	}
}
