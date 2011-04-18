// CS0183: The given expression is always of the provided (`int') type
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
