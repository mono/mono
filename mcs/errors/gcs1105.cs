// CS1105: `S.Foo(this int?)': Extension methods must be declared static
// Line: 6
// Compiler options: -langversion:linq

static class S
{
	void Foo (this int? s)
	{
	}
}
