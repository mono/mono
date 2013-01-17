// CS1060: Use of possibly unassigned field `c'
// Line: 25
// Compiler options: -warnaserror

public class C
{
	public int v;
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
		s.s2.c.v = 9;
	}
}
