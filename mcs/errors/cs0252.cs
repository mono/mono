// CS0252: Possible unintended reference comparison. Consider casting the left side of the expression to `string' to compare the values
// Line: 10
// Compiler options: -warn:2 -warnaserror

using System;

class X {
	static void Main() {
		object a = "11";
		Console.WriteLine(a == "11");
	}
}
