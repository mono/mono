// Compiler options: -unsafe 

unsafe class X
{
	struct S
	{
	}

	public class N<T>
	{
		S* s;
	}
}

public class C
{
	public static void Main ()
	{
		new X.N<int> ();
	}
}