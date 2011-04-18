// CS0411: The type arguments for method `C.Foo<T>(IFoo<T>, IFoo<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 17

interface IFoo<in T>
{
}

class C
{
	public static void Foo<T> (IFoo<T> e1, IFoo<T> e2)
	{
	}
	
	public static void Main ()
	{
		IFoo<int> a = null;
		IFoo<object> b = null;
		Foo (a, b);
	}
}
