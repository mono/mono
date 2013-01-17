// CS0170: Use of possibly unassigned field `c'
// Line: 24

public class C
{
	public int v;
}

public struct S
{
	public C c;
}

public struct S2
{
	S s;
}

public class Test
{
	static void Main ()
	{
		S s;
		int xx = s.c.v;
	}
}
