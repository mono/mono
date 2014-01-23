class Test<A,B>
{
	public void Foo<V,W> (Test<A,W> x, Test<V,B> y)
	{ }
}

class X
{
	public static void Main ()
	{
		Test<float,int> test = new Test<float,int> ();
		test.Foo (test, test);
	}
}
