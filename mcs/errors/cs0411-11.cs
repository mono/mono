// CS0411: The type arguments for method `C.Foo<T>(I<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 17

interface I<T>
{
}

class C : I<long>, I<int>
{
	static void Foo<T> (I<T> i)
	{
	}

	static void Main ()
	{
		C c = new C ();
		Foo (c);
	}
}
