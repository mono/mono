// CS0165: Use of unassigned local variable `errors'
// Line: 9
// Compiler options: -langversion:future

class T
{
	static void Main ()
	{
		dynamic errors;
		errors.Call ();
	}
}
