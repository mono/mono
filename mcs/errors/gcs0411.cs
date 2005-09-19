// gcs0411.cs: The type arguments for method `Foo' cannot be infered from the usage. Try specifying the type arguments explicitly
// Line: 15

class Test<A>
{
	public void Foo<V> (V v, V w)
	{ }
}

class X
{
	static void Main ()
	{
		Test<float> test = new Test<float> ();
		test.Foo (8, "Hello World");
	}
}

