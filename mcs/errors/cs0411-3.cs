// CS0411: The type arguments for method `C.Foo<T>(T)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 12

class C
{
	static void X ()
	{
	}
	
	static void Foo<T> (T t)
	{
		Foo(X ());
	}
}
