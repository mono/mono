// CS1060: Use of possibly unassigned field `c'
// Line: 25
// Compiler options: -warnaserror

using System;

public class C
{
	public EventHandler v;
}

public struct S2
{
	public C c;
}

public struct S
{
	public S2 s2;
}

public class Test
{
	static void Main ()
	{
		S s;
		s.s2.c.v = null;
	}
}
