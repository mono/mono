// cs0253.cs: Possible unintended reference comparison; to get a value comparison, cast the right hand side to type `string'.
// Line: 10
// Compiler options: -warn:2 -warnaserror

using System;

class X {
	static void Main() {
		object a = "11";
		Console.WriteLine("11" == a);
	}
}
