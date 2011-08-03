// CS1981: Using `is' to test compatibility with `dynamic' is identical to testing compatibility with `object'
// Line: 10
// Compiler options: -warnaserror

class C
{
	public static void Main ()
	{
		object o = null;
		bool b = o is dynamic;
	}
}
