// CS0411: The type arguments for method `C.Foo<T>(T)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 14
// Compiler options: -unsafe

class C
{
	static void Foo<T> (T t)
	{
	}

	unsafe static void Test ()
	{
		int* i = null;
		Foo (i);
	}
}
