// CS0619: `C' is obsolete: `x'
// Line: 24

using System;

interface I
{
}

[Obsolete ("x", true)]
class C
{
	public void Foo () { }
}

class M
{
	public static void Main ()
	{
	}

	public void Test (C c)
	{
		c.Foo ();
	}
}
