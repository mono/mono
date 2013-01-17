using System;

static partial class C
{
	static partial void Foo_1 (this string s);

	[Obsolete]
	static partial void Foo_2 (string s);

	public static void Main()
	{
	}
}

partial class D
{
	static partial void Method(this int a);
}

static partial class D
{
	static partial void Method(this int a)
	{
	}
}
