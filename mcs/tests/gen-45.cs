class Test<A,B>
{
	public void Foo<U> (U u)
	{ }

	public void Foo<V> (V[] v, V w)
	{ }

	public void Hello<V,W> (Test<V,B> v, Test<A,W> w)
	{ }

	public void ArrayMethod<V> (params V[] args)
	{ }
}

class X
{
	static void Main ()
	{
		Test<float,int> test = new Test<float,int> ();
		test.Foo ("Hello World");
		test.Foo (new long[] { 3, 4, 5 }, 9L);
		test.Hello (test, test);
		test.ArrayMethod (3.14F, (float) 9 / 3);
	}
}

