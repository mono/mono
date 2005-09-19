// gcs0411.cs: The type arguments for method `Foo' cannot be infered from the usage. Try specifying the type arguments explicitly
// Line: 15

class Test<A,B>
{
	public void Foo<V,W> (Test<A,W> x, Test<V,B> y)
	{ }
}

class X
{
	static void Main ()
	{
		Test<float,int> test = new Test<float,int> ();
		test.Foo (test, test);
	}
}

