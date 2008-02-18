// CS1109: `C.S.Foo(this string)': Extension methods cannot be defined in a nested class
// Line: 8


class C
{
	static class S
	{
		static void Foo (this string s)
		{
		}
	}
}
