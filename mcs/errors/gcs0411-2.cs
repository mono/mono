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

