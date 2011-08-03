// CS0184: The given expression is never of the provided (`byte') type
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
