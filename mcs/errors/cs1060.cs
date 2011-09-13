// CS1060: Use of possibly unassigned field `c'
// Line: 19
// Compiler options: -warnaserror

public class C
{
	public int v;
}

public struct S
{
	public C c;
}

public class Test
{
	static void Main ()
	{
		S s;
		s.c.v = 5;
	}
}
