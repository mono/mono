// CS0411: The type arguments for method `C.Foo<T>(T[], T[])' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 12

class C
{
	public static void Foo<T> (T[] t1, T[] t2)
	{
	}
	
	public static void Main ()
	{
		Foo (new int[0], new byte[0]);
	}
}
