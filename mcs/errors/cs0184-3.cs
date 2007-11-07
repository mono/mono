// CS0184: The given expression is never of the provided (`decimal') type
// Line: 7
// Compiler options: -warnaserror -warn:1

class A
{
	static void Main ()
	{ 
		System.Console.WriteLine (1 is decimal);
	}
}
