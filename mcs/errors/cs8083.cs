// CS8083: An alias-qualified name is not an expression
// Line: 8

class C
{
	static void Main ()
	{
		string s = nameof (global::C);
	}
}