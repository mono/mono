// CS1110: `C.Foo(this string)': Extension methods cannot be declared without a reference to System.Core.dll assembly. Add the assembly reference or remove `this' modifer from the first parameter
// Line: 7
// Compiler options: -noconfig

static class C
{
	static void Foo (this string s)
	{
	}
}
