// CS0184: The given expression is never of the provided (`string') type
// Line: 7
// Compiler options: -warnaserror -warn:1

class A {
	static void Main () { 
		System.Console.WriteLine (null is string);
	}
}
