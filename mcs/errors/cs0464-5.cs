// CS0464: The result of comparing type `E?' with null is always `false'
// Line: 14
// Compiler options: -warnaserror

enum E
{
}

class X
{
	public static void Main ()
	{
		E u = 0;
		var b = u < (E?) null;
	}
}