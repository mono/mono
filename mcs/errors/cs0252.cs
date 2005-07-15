// cs0252.cs: Possible unintended reference comparison; to get a value comparison, cast the left hand side to type `string'.
// Line: 10
// Compiler options: -warn:2 -warnaserror

using System;

class X {
	static void Main() {
		object a = "11";
		Console.WriteLine(a == "11");
	}
}
