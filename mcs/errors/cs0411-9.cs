// CS0411: The type arguments for method `Test.Foo<A>(D<A>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 15

delegate void D<T> (T t);

class Test
{
	public static D<A> Foo<A> (D<A> a)
	{
		return null;
	}
	
	public static void Main ()
	{
		Foo (delegate {});
	}
}

