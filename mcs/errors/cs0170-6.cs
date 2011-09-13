// CS0170: Use of possibly unassigned field `c'
// Line: 21

public class C
{
	public int v;
}

public struct S
{
	public int a;
	public C c;
}

public class Test
{
	static void Main ()
	{
		S s;
		s.a = 2;
		int xx = s.c.v;
	}
}
