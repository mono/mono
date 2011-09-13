// Compiler options: -warnaserror

public class C
{
	public int v;
}

public struct S2
{
	public C c;
	public int v;
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
		s.s2.v = 9;
	}
}