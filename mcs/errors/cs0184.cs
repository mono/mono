// cs0184.cs: The expression is never of the provided type
// Line:
// Compiler options: -warnaserror -warn:1

class X {

	static void Main ()
	{
		int a = 1;
		
		if (a is byte){
		}
	}
}
