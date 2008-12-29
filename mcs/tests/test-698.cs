// Compiler options: -unsafe

struct Foo
{
	public int i;
}

public unsafe class test
{
	static Foo* pFoo;

	public static void Main ()
	{
		int* pi = &pFoo->i;
	}
}
