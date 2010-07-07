// CS0252: Possible unintended reference comparison. Consider casting the left side expression to type `string' to get value comparison
// Line: 10
// Compiler options: -warn:2 -warnaserror

using System;

class X {
	static void Main() {
		object a = "11";
		Console.WriteLine(a == "11");
	}
}
