// cs0183.cs: the expression is always of the type `x'
// Line:
// Compiler options: -warnaserror -warn:1

class X {

	static void Main ()
	{
		int i = 5;

		if (i is int){
		}
	}
}
