// CS0619: `S.S()' is obsolete: `ctor'
// Line: 18

using System;

struct S
{
	[Obsolete ("ctor", true)]
	public S ()
	{
	}
}

class C
{
	public static void Main ()
	{
		new S ();
	}
}
