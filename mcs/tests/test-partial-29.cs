using System;

static partial class C
{
	static partial void Foo_1 (this string s);

	[Obsolete]
	static partial void Foo_2 (string s);

	static void Main()
	{
	}
}
