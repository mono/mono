// CS8123: The tuple element name `a' is ignored because a different name or no name is specified by the target type `(int, int)'
// Line: 9
// Compiler options: -warnaserror

class C
{
	public static void Main ()
	{
		(int tt1, int tt2) t = (a: 1, 2);
	}
}