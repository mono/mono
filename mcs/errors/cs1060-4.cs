// CS1060: Use of possibly unassigned field `x'
// Line: 30
// Compiler options: -warnaserror

public class C
{
	public int v;
}

public struct S
{
	public C c;
}

class X
{
	public S s;
}

struct S2
{
	public X x;
}

public class Test
{
	static void Main ()
	{
		S2 s2;
		s2.x.s.c.v = 5;
	}
}
