// CS1061: Type `int' does not contain a definition for `Foo' and no extension method `Foo' of type `int' could be found. Are you missing an assembly reference?
// Line: 11

using System;

static class C
{
	static void Main ()
	{
		int i = 1;
		Action a = i.Foo;
	}

	static void Foo (this string s)
	{
	}
}