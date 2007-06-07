// gcs0411.cs: The type arguments for method `Test<float>.Foo<V,W>(in V, in V)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 15

class Test<A>
{
	public void Foo<V,W> (V v, V w)
	{ }
}

class X
{
	static void Main ()
	{
		Test<float> test = new Test<float> ();
		test.Foo (8, 9);
	}
}

